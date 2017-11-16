using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.PayAnyWay.Components
{
    [ViewComponent(Name = "PaymentPayAnyWay")]
    public class PaymentPayAnyWayViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.PayAnyWay/Views/PaymentInfo.cshtml");
        }
    }
}
