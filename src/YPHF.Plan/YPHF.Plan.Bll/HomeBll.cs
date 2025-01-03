﻿/*
 *
 * 版权所有 ：易鹏航服
 * 作   者 : duhuifeng
 *
 */

#region

using AutoMapper;
using YPHF.Core.Data;
using YPHF.Core.Orm;
using YPHF.Core.Orm.SqlSugar;
using YPHF.Plan.Dal;
using YPHF.Plan.Dto.Home;
using YPHF.Plan.Model;

#endregion

namespace YPHF.Plan.Bll;

/// <summary>
/// </summary>
public class HomeBll : BaseBll<HomeModel>, IHomeBll
{
    /// <summary>
    /// </summary>
    private readonly HomeDal dal;

    /// <summary>
    /// </summary>
    private readonly IMapper mapper;

    /// <summary>
    /// </summary>
    public HomeBll()
    {
        dal = new HomeDal();

        var context = BaseHttpContext.Context;
        mapper = context.RequestServices.GetService(typeof(IMapper)) as IMapper ?? default!;
    }

    /// <summary>
    /// </summary>
    public override IBaseDal<HomeModel> DAL => dal;

    /// <summary>
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public async Task<PageResult<HomeResult>> PageAsync(HomeArgs args)
    {
        var model = await base.PageAsync(args);
        var result = mapper.Map<List<HomeResult>>(model);

        return new PageResult<HomeResult>(args.Offset, args.Limit, args.Total, result);
    }
}