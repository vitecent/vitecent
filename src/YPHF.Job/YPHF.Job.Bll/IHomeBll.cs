﻿/*
 *
 * 版权所有 ：易鹏航服
 * 作   者 : duhuifeng
 *
 */

#region

using YPHF.Core.Data;
using YPHF.Job.Dto.Home;

#endregion

namespace YPHF.Job.Bll;

/// <summary>
/// </summary>
public interface IHomeBll
{
    /// <summary>
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    Task<PageResult<HomeResult>> PageAsync(HomeArgs args);
}