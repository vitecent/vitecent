﻿/*
 *
 * 版权所有 ：易鹏航服
 * 作   者 : duhuifeng
 *
 */

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Polly;
using Polly.Timeout;
using YPHF.Core.Cache;
using YPHF.Core.Data;
using YPHF.Core.Register;

namespace YPHF.Core.Web
{
    /// <summary>
    /// Class BaseGateway.
    /// </summary>
    /// <param name="next"></param>
    /// <param name="httpClient"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="cache"></param>
    public class BaseGateway(RequestDelegate next, IHttpClientFactory httpClient, IServiceProvider serviceProvider, IBaseCache cache)
    {
        /// <summary>
        /// The register
        /// </summary>
        private readonly IRegister register = serviceProvider.GetService<IRegister>() ?? default!;

        /// <summary>
        /// Invokes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public async Task Invoke(HttpContext context)
        {
            var TraceingId = string.Empty;

            if (context.Request.Headers.TryGetValue(Const.TraceingId, out StringValues value))
            {
                TraceingId = value.ToString();
            }

            if (string.IsNullOrWhiteSpace(TraceingId))
            {
                TraceingId = Guid.NewGuid().ToString("N");
                context.Request.Headers.Remove(Const.TraceingId);
                context.Request.Headers.TryAdd(Const.TraceingId, TraceingId);
            }

            context.Response.Headers.TryAdd(Const.TraceingId, TraceingId);

            var uri = await GetServiceUri(context);

            if (!string.IsNullOrWhiteSpace(uri))
            {
                var timeoutPolicy = Policy.TimeoutAsync(15);

                var retryPolicy = Policy.Handle<Exception>()
                    .Or<TimeoutRejectedException>()
                    .WaitAndRetryAsync([
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(20)
                    ]);

                var breakerPolicy = Policy.Handle<Exception>()
                    .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

                var policyWrap = timeoutPolicy.WrapAsync(retryPolicy).WrapAsync(breakerPolicy);

                await policyWrap.ExecuteAsync(async () =>
                {
                    var request = new HttpRequestMessage
                    {
                        Method = new HttpMethod(context.Request.Method),
                        RequestUri = new Uri(uri),
                        Content = new StreamContent(context.Request.Body)
                    };

                    foreach (var header in context.Request.Headers)
                    {
                        if (!request.Headers.TryAddWithoutValidation(header.Key, [.. header.Value]))
                        {
                            request.Content?.Headers.TryAddWithoutValidation(header.Key, [.. header.Value]);
                        }
                    }

                    using var response = await httpClient.CreateClient().SendAsync(request);
                    context.Response.StatusCode = (int)response.StatusCode;

                    foreach (var header in response.Headers)
                    {
                        context.Response.Headers[header.Key] = header.Value.ToArray();
                    }

                    foreach (var header in response.Content.Headers)
                    {
                        context.Response.Headers[header.Key] = header.Value.ToArray();
                    }

                    context.Response.Headers.Remove("Transfer-Encoding");

                    await response.Content.CopyToAsync(context.Response.Body);
                });

                return;
            }

            await next(context);
        }

        /// <summary>
        /// Gets the service URI.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>System.String.</returns>
        private async Task<string> GetServiceUri(HttpContext context)
        {
            var baseUri = new Uri(context.Request.GetDisplayUrl());

            var uri = string.Empty;

            var pathAndQuery = baseUri.PathAndQuery.ToLower();

            var key = pathAndQuery.Split("/", StringSplitOptions.RemoveEmptyEntries).Where(x => x != "api").FirstOrDefault() ?? "";

            if (key == "check" && key == "gateway")
            {
                return default!;
            }

            var services = new Dictionary<string, List<ServiceConfig>>();

            if (!cache.HasKey(Const.RegistServices))
            {
                services = await register.DiscoverAsync();

                cache.SetString(Const.RegistServices, services, TimeSpan.FromMinutes(1));
            }
            else
            {
                services = cache.GetString<Dictionary<string, List<ServiceConfig>>>(Const.RegistServices);
            }

            if (services.TryGetValue(key, out List<ServiceConfig>? list))
            {
                var microService = BaseService.GetServiceRandom(list);

                if (microService != null)
                {
                    uri = $"http://{microService.Address}:{microService.Port}{pathAndQuery.Replace($"/{key}", "")}";

                    if (microService.IsHttps)
                    {
                        uri = uri.Replace("http://", "https://");
                    }
                }

                return uri;
            }

            return default!;
        }
    }
}