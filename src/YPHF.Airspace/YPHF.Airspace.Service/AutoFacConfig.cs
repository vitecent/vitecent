﻿/*
 *
 * 版权所有 ：易鹏航服
 * 作   者 : duhuifeng
 *
 */

#region

using Autofac;
using YPHF.Airspace.Bll;

#endregion

namespace YPHF.Airspace.Service;

/// <summary>
/// </summary>
public class AutoFacConfig : Module
{
    /// <summary>
    /// </summary>
    /// <param name="builder"></param>
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);
        builder.RegisterType(typeof(HomeBll)).As(typeof(IHomeBll));
    }
}