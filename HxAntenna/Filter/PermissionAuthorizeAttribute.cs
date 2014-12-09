using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HxAntenna.Filter
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class PermissionAuthorizeAttribute : FilterAttribute, IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            
        }
    }
}