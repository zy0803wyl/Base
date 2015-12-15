using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcBase.Controllers
{
    /// <summary>
    /// 测试数据控制器
    /// </summary>
    public class TestDataController : Controller
    {
        //
        // GET: /TestData/

        public ActionResult Index()
        {
            return View();
        }

    }
}
