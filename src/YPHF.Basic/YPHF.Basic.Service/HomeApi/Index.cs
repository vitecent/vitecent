﻿/*
 *
 * 版权所有 ：易鹏航服
 * 作   者 : duhuifeng
 *
 */

#region

using Microsoft.AspNetCore.Mvc;
using YPHF.Basic.Bll;
using YPHF.Basic.Dto.Home;
using YPHF.Core.Data;
using YPHF.Core.Web.Api;

#endregion

namespace YPHF.Basic.Service.HomeApi;

/// <summary>
/// </summary>
[ApiController]
[Route("Home")]
public class Index(IHomeBll bll) : BaseApi<HomeArgs, PageResult<HomeResult>>
{
    /// <summary>
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("Index")]
    public override async Task<PageResult<HomeResult>> InvokeAsync([FromBody] HomeArgs args)
    {
        return await bll.PageAsync(args);
    }
}