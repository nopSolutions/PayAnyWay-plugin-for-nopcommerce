using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.PayAnyWay
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //confirm pay
            routeBuilder.MapRoute("Plugin.Payments.PayAnyWay.ConfirmPay",
                 "Plugins/PayAnyWay/ConfirmPay",
                 new { controller = "PaymentPayAnyWay", action = "ConfirmPay" });
            //cancel
            routeBuilder.MapRoute("Plugin.Payments.PayAnyWay.CancelOrder",
                 "Plugins/PayAnyWay/CancelOrder",
                 new { controller = "PaymentPayAnyWay", action = "CancelOrder" });
            //success
            routeBuilder.MapRoute("Plugin.Payments.PayAnyWay.Success",
                 "Plugins/PayAnyWay/Success",
                 new { controller = "PaymentPayAnyWay", action = "Success" });
            //return
            routeBuilder.MapRoute("Plugin.Payments.PayAnyWay.Return",
                 "Plugins/PayAnyWay/Return",
                 new { controller = "PaymentPayAnyWay", action = "Return" });
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
