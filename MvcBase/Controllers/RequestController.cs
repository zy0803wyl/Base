using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcBase.Controllers
{
    /// <summary>
    /// 创建请求页面的控制器
    /// </summary>
    public class RequestController : Controller
    {
        //
        // GET: /Request/

        public ActionResult Index()
        {
            return View();
        }

    }
}
