/*
 *
 * ��Ȩ���� ����������
 * ��   �� : duhuifeng
 *
 */

using YPHF.Core.Web;

namespace YPHF.Statistics.Service
{
    /// <summary>
    /// </summary>
    public class Program
    {
        /// <summary>
        /// </summary>
        /// <param name="args"></param>
        public static async Task Main(string[] args)
        {
            var xmls = new List<string>()
            {
                //"YPHF.Statistics.Service"
            };

            var microService = new BaseMicroService("YPHF.Statistics.Service", xmls)
            {
                OnBuild = (builder) =>
                {
                    builder.UseAutoMapper(typeof(AutoMapperConfig));
                    builder.UseAutoFac(new AutoFacConfig());
                }
            };

            await microService.RunAsync(args);
        }
    }
}