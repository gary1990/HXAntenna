using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HxAntenna.Controllers
{
    public class QualityManageHomeController : Controller
    {
        List<string> path = new List<string>();
        public QualityManageHomeController() 
        {
            path.Add("质量管理");
        }
        public ActionResult Index()
        {
            ViewBag.path = path;
            return View();
        }
	}
}