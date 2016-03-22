using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.PayAnyWay
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            //confirm pay
            routes.MapRoute("Plugin.Payments.PayAnyWay.ConfirmPay",
                 "Plugins/PayAnyWay/ConfirmPay",
                 new { controller = "PaymentPayAnyWay", action = "ConfirmPay" },
                 new[] { "Nop.Plugin.Payments.PayAnyWay.Controllers" }
            );
            //cancel
            routes.MapRoute("Plugin.Payments.PayAnyWay.CancelOrder",
                 "Plugins/PayAnyWay/CancelOrder",
                 new { controller = "PaymentPayAnyWay", action = "CancelOrder" },
                 new[] { "Nop.Plugin.Payments.PayAnyWay.Controllers" }
            );
            //success
            routes.MapRoute("Plugin.Payments.PayAnyWay.Success",
                 "Plugins/PayAnyWay/Success",
                 new { controller = "PaymentPayAnyWay", action = "Success" },
                 new[] { "Nop.Plugin.Payments.PayAnyWay.Controllers" }
            );
            //return
            routes.MapRoute("Plugin.Payments.PayAnyWay.Return",
                 "Plugins/PayAnyWay/Return",
                 new { controller = "PaymentPayAnyWay", action = "Return" },
                 new[] { "Nop.Plugin.Payments.PayAnyWay.Controllers" }
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
