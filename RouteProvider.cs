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
            //Cancel
            routes.MapRoute("Plugin.Payments.MonetaAssist.CancelOrder",
                 "Plugins/MonetaAssist/CancelOrder",
                 new { controller = "PaymentMonetaAssist", action = "CancelOrder" },
                 new[] { "Nop.Plugin.Payments.MonetaAssist.Controllers" }
            );
            //Succes
            routes.MapRoute("Plugin.Payments.MonetaAssist.Succes",
                 "Plugins/MonetaAssist/Succes",
                 new { controller = "PaymentMonetaAssist", action = "Succes" },
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
