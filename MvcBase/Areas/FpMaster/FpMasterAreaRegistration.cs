using System.Web.Mvc;

namespace MvcBase.Areas.FpMaster
{
    public class FpMasterAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "FpMaster";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "FpMaster_default",
                "FpMaster/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
