using HxAntenna.Interface;
using HxAntenna.Models.Base;
using HxAntenna.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace HxAntenna.Controllers
{
    public class BaseModelController<Model> : Controller where Model : BaseModel, IEditable<Model>
    {
        public UnitOfWork UW;
        public GenericRepository<Model> GR;
        public string ViewPathStart = "~/Views/";
        public string ViewPath = "BaseModel";
        public string ViewPathBase = "BaseModel";
        public string ViewPathEnd = "/";
        public BaseModelController() 
        {
            UW = new UnitOfWork();
        }
        public virtual ActionResult Index(int page = 1, bool includeSoftDeleted = false, string filter = null)
        {
            ViewBag.RV = new RouteValueDictionary { 
                                                    { "tickTime", DateTime.Now.ToLongTimeString() }, 
                                                    { "returnRoot", "Index" }, 
                                                    { "actionAjax", "Get" }, 
                                                    { "page", page }, 
                                                    { "includeSoftDeleted", includeSoftDeleted },
                                                    { "filter", filter}
                                                  };
            return View(ViewPathStart + ViewPath + ViewPathEnd + "Index.cshtml");
        }
	}
}