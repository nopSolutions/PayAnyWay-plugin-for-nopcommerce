using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.MonetaDirect
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            //ConfirmPay
            routes.MapRoute("Plugin.Payments.MonetaDirect.ConfirmPay",
                 "Plugins/MonetaDirect/ConfirmPay",
                 new { controller = "PaymentMonetaDirect", action = "ConfirmPay" },
                 new[] { "Nop.Plugin.Payments.MonetaDirect.Controllers" }
            );
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
