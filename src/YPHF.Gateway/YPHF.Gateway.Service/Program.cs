/*
 *
 * 版权所有 ：易鹏航服
 * 作   者 : duhuifeng
 *
 */

#region

using YPHF.Core.Web;

#endregion

namespace YPHF.Gateway.Service;

/// <summary>
/// </summary>
public class Program
{
    /// <summary>
    /// </summary>
    public static async Task Main(string[] args)
    {
        var xmls = new List<string>
        {
            "YPHF.Core.*.xml",
            "YPHF.Gateway.*.xml"
        };

        var microService = new BaseMicroService("YPHF.Gateway.Service", xmls)
        {
            OnBuild = builder =>
            {
                builder.Services.AddGateway();
                builder.UseAutoMapper(typeof(AutoMapperConfig));
                builder.UseAutoFac(new AutoFacConfig());
            },
            OnStart = app => { app.UseGateway(); }
        };

        await microService.RunAsync(args);
    }
}