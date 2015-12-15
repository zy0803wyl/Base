using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcBase.Controllers
{
    /// <summary>
    /// 默认页面控制器
    /// </summary>
    public class DefaultController : Controller
    {
        //
        // GET: /Default/

        public ActionResult Index()
        {
            return View();
        }

    }
}
