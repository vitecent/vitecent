﻿/*
 *
 * 版权所有 ：易鹏航服
 * 作   者 : duhuifeng
 *
 */

#region

using log4net;
using Microsoft.AspNetCore.Builder;
using Quartz;
using YPHF.Core.Api.Swagger;
using YPHF.Core.Cache.Redis;
using YPHF.Core.Job.Quartz;
using YPHF.Core.Orm.SqlSugar;
using YPHF.Core.Register.Consul;
using YPHF.Core.Trace.Zipkin;

#endregion

namespace YPHF.Core.Web;

/// <summary>
/// JobMicroService 类，继承自 MicroService，用于配置和启动微服务。
/// </summary>
public class JobMicroService : MicroService
{
    /// <summary>
    /// 日志记录器实例。
    /// </summary>
    private readonly ILog logger;

    /// <summary>
    /// 服务标题。
    /// </summary>
    private readonly string title;

    /// <summary>
    ///   XML 文档列表。
    /// </summary>
    private readonly List<string> xmls;

    /// <summary>
    /// Quartz 调度器实例。
    /// </summary>
    private IScheduler scheduler = default!;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="title">服务标题</param>
    /// <param name="xmls">XML文档列表</param>
    public JobMicroService(string title, List<string> xmls)
    {
        this.title = title;
        this.xmls = xmls;

        logger = BaseLogger.GetLogger();

        logger.Info("开始构建任务微服务");
    }

    /// <summary>
    /// 在构建 WebApplicationBuilder 时执行的操作。
    /// </summary>
    public Action<WebApplicationBuilder> OnBuild { get; set; } = default!;

    /// <summary>
    /// 在注册 Quartz 调度器时执行的操作。
    /// </summary>
    public Action<IScheduler> OnRegist { get; set; } = default!;

    /// <summary>
    /// 异步构建 WebApplicationBuilder。
    /// </summary>
    /// <param name="builder">WebApplicationBuilder 实例。</param>
    /// <returns>异步任务。</returns>
    protected override async Task BuildAsync(WebApplicationBuilder builder)
    {
        // 调用基类的 BuildAsync 方法
        await base.BuildAsync(builder);

        var configuration = builder.Configuration;
        var services = builder.Services;

        logger.Info("开始添加 Redis 服务");
        // 添加 Redis 服务
        services.AddRedis(configuration);

        logger.Info("开始添加 Consul 服务");
        // 添加 Consul 服务
        services.AddConsul(configuration);

        logger.Info("开始 添加 Zipkin 服务");
        // 添加 Zipkin 服务
        services.AddZipkin(configuration);

        logger.Info("开始添加 SqlSugar ORM 服务");
        // 添加 SqlSugar ORM 服务
        services.AddSqlSugger(configuration);

        logger.Info("开始添加 Swagger 服务");
        // 添加 Swagger 服务
        services.AddSwagger(title, xmls);

        logger.Info("开始初始化 Quartz 调度器");
        // 初始化 Quartz 调度器
        scheduler = await services.AddQuarzAsync();

        logger.Info("开始注册 Quartz 调度器");
        // 创建并启动一个新线程来注册 Quartz 调度器
        var job = new Thread(() => OnRegist?.Invoke(scheduler))
        {
            IsBackground = true,
            Priority = ThreadPriority.Highest,
            Name = "Job"
        };

        job.Start();

        logger.Info("开始执行构建回调");

        // 执行构建回调
        OnBuild?.Invoke(builder);
    }

    /// <summary>
    /// 异步启动 WebApplication。
    /// </summary>
    /// <param name="app">WebApplication 实例。</param>
    /// <returns>异步任务。</returns>
    protected override async Task StartAsync(WebApplication app)
    {
        // 调用基类的 StartAsync 方法
        await base.StartAsync(app);

        if (scheduler != null)
        {
            logger.Info("开始使用 Quartz 调度器");
            // 使用 Quartz 调度器
            app.UseQuarz(scheduler);
        }

        logger.Info("开始使用 Consul 中间件");
        // 使用 Consul 中间件
        await app.UseConsulAsync();

        logger.Info("开始使用 Zipkin 中间件");
        // 使用 Zipkin 中间件
        app.UseZipkin();

        logger.Info("开始使用 Swagger 仪表板");
        // 使用 Swagger 仪表板
        app.UseSwaggerDashboard();
    }
}