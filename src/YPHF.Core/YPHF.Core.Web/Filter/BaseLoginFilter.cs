﻿/*
 *
 * 版权所有 ：易鹏航服
 * 作   者 : duhuifeng
 *
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using YPHF.Core.Data;

namespace YPHF.Core.Web.Filter
{
    /// <summary>
    /// </summary>
    public class BaseLoginFilter : ActionFilterAttribute
    {
        /// <summary>
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            dynamic controller = context.Controller;

            if (controller?.User == null)
            {
                context.Result = new JsonResult(new BaseResult(301, "登录超时,请重新登录"));

                return;
            }
        }
    }
}