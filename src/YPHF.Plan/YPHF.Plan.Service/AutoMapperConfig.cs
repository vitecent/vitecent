﻿/*
 *
 * 版权所有 ：易鹏航服
 * 作   者 : duhuifeng
 *
 */

using YPHF.Core.Web;
using YPHF.Plan.Dto.Home;
using YPHF.Plan.Model;

namespace YPHF.Plan.Service
{
    /// <summary>
    /// </summary>
    public class AutoMapperConfig : BaseMapperConfig
    {
        /// <summary>
        /// </summary>
        public override void Map()
        {
            CreateMap<HomeModel, HomeResult>();
            CreateMap<HomeResult, HomeModel>();
        }
    }
}