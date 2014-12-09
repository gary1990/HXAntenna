using HxAntenna.Lib;
using HxAntenna.Models;
using HxAntenna.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace HxAntenna.Controllers
{
    public class TestRecordController : Controller
    {
        List<string> path = new List<string> { };
        private UnitOfWork unitOfWork = new UnitOfWork();
        public string ViewPath1 = "~/Views/";
        public string ViewPath = "TestRecord";
        public string ViewPathBase = "TestRecord";
        public string ViewPath2 = "/";

        public TestRecordController() {
            path.Add("质量管理");
            path.Add("测试记录");
            ViewBag.path = path;
            ViewBag.Name = "测试记录";
            ViewBag.Title = "测试记录";
        }
        public ActionResult Index(int page = 1, string filter = null)
        {
            ViewBag.RV = new RouteValueDictionary { { "tickTime", DateTime.Now.ToLongTimeString() }, { "returnRoot", "Index" }, { "actionAjax", "Get" }, { "page", page }, { "filter", filter } };
            return View();
        }

        public virtual PartialViewResult Get(string returnRoot, string actionAjax = "", int page = 1, bool includeSoftDeleted = false, string filter = null)
        {
            var results = Common<TestResult>.GetQuery(unitOfWork, filter);

            results = results.OrderByDescending(a => a.TestTime);

            var rv = new RouteValueDictionary { { "tickTime", DateTime.Now.ToLongTimeString() }, { "returnRoot", returnRoot }, { "actionAjax", actionAjax }, { "page", page }, { "filter", filter } };
            return PartialView(ViewPath1 + ViewPath + ViewPath2 + "Get.cshtml", Common<TestResult>.Page(this, rv, results));
        }
	}
}