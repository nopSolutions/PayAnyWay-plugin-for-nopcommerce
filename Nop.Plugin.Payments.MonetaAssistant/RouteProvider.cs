using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.MonetaAssistant
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            //confirm pay
            routes.MapRoute("Plugin.Payments.MonetaAssistant.ConfirmPay",
                 "Plugins/MonetaAssistant/ConfirmPay",
                 new { controller = "PaymentMonetaAssistant", action = "ConfirmPay" },
                 new[] { "Nop.Plugin.Payments.MonetaAssistant.Controllers" }
            );
            //cancel
            routes.MapRoute("Plugin.Payments.MonetaAssistant.CancelOrder",
                 "Plugins/MonetaAssistant/CancelOrder",
                 new { controller = "PaymentMonetaAssistant", action = "CancelOrder" },
                 new[] { "Nop.Plugin.Payments.MonetaAssistant.Controllers" }
            );
            //success
            routes.MapRoute("Plugin.Payments.MonetaAssistant.Success",
                 "Plugins/MonetaAssistant/Success",
                 new { controller = "PaymentMonetaAssistant", action = "Success" },
                 new[] { "Nop.Plugin.Payments.MonetaAssistant.Controllers" }
            );
            //return
            routes.MapRoute("Plugin.Payments.MonetaAssistant.Return",
                 "Plugins/MonetaAssistant/Return",
                 new { controller = "PaymentMonetaAssistant", action = "Return" },
                 new[] { "Nop.Plugin.Payments.MonetaAssistant.Controllers" }
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
