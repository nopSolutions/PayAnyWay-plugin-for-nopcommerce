using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.MonetaAssist
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            //confirm pay
            routes.MapRoute("Plugin.Payments.MonetaAssist.ConfirmPay",
                 "Plugins/MonetaAssist/ConfirmPay",
                 new { controller = "PaymentMonetaAssist", action = "ConfirmPay" },
                 new[] { "Nop.Plugin.Payments.MonetaAssist.Controllers" }
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
