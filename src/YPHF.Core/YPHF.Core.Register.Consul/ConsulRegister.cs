﻿/*
 *
 * 版权所有 ：易鹏航服
 * 作   者 : duhuifeng
 *
 */

using Consul;
using YPHF.Core.Cache;

namespace YPHF.Core.Register.Consul
{
    /// <summary>
    /// Class ConsulRegister. Implements the <see cref="YPHF.Core.Register.IRegister"/>
    /// </summary>
    /// <seealso cref="YPHF.Core.Register.IRegister"/>
    /// <param name="uri"></param>
    /// <param name="cahce"></param>
    public class ConsulRegister(string uri, IBaseCache cahce) : IRegister
    {
        /// <summary>
        /// The key
        /// </summary>
        private static readonly object key = new();

        /// <summary>
        /// The client
        /// </summary>
        private readonly ConsulClient client = new(x =>
        {
            x.Address = new Uri(uri);
        });

        /// <summary>
        /// Deregisters the asynchronous.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task DeregisterAsync(string serviceId)
        {
            await client.Agent.ServiceDeregister(serviceId);
        }

        /// <summary>
        /// Discovers the asynchronous.
        /// </summary>
        /// <returns>A Task&lt;Dictionary`2&gt; representing the asynchronous operation.</returns>
        public async Task<Dictionary<string, List<ServiceConfig>>> DiscoverAsync()
        {
            return await Task.Run(() =>
              {
                  var result = new Dictionary<string, List<ServiceConfig>>();

                  var cacheService = cahce.GetString<Dictionary<string, List<ServiceConfig>>>("CacheService");

                  if (cacheService == null)
                  {
                      lock (key)
                      {
                          if (cacheService == null)
                          {
                              var services = client.Agent.Services().Result;

                              foreach (var service in services.Response)
                              {
                                  var item = service.Value;

                                  if (result.TryGetValue(item.Service.ToLower(), out List<ServiceConfig>? list))
                                  {
                                      list.Add(new()
                                      {
                                          Id = service.Key,
                                          Name = item.Service,
                                          Address = item.Address,
                                          Port = item.Port,
                                      });
                                  }
                                  else
                                  {
                                      list = [
                                          new()
                                          {
                                              Id = service.Key,
                                              Name = item.Service,
                                              Address = item.Address,
                                              Port = item.Port,
                                          }
                                      ];

                                      result.Add(item.Service.ToLower(), list);
                                  }
                              }

                              cahce.SetString("CacheService", result, TimeSpan.FromSeconds(15));
                          }
                      }
                  }

                  return result;
              });
        }

        /// <summary>
        /// Register as an asynchronous operation.
        /// </summary>
        /// <param name="microService">The micro service.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task RegisterAsync(ServiceConfig microService)
        {
            var service = new AgentServiceRegistration()
            {
                ID = microService.Id,
                Name = microService.Name,
                Address = microService.Address,
                Port = microService.Port,
                Check = new AgentServiceCheck()
                {
                    Interval = TimeSpan.FromSeconds(30),
                    HTTP = $"http://{microService.Address}:{microService.Port}{microService.Check}",
                    Timeout = TimeSpan.FromSeconds(microService.Timeout),
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(microService.Deregister)
                }
            };

            if (microService.IsHttps)
            {
                service.Check.HTTP = service.Check.HTTP.Replace("http://", "https://");
            }

            await client.Agent.ServiceRegister(service);
        }
    }
}