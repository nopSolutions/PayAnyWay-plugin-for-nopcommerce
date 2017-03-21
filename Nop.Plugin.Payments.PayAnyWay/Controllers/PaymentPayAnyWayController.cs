using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.PayAnyWay.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.PayAnyWay.Controllers
{
    public class PaymentPayAnyWayController : BasePaymentController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private readonly PaymentSettings _paymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;

        public PaymentPayAnyWayController(IWorkContext workContext,
            IStoreService storeService, 
            ISettingService settingService, 
            IPaymentService paymentService, 
            IOrderService orderService, 
            IOrderProcessingService orderProcessingService, 
            ILogger logger,
            PaymentSettings paymentSettings, 
            ILocalizationService localizationService, IWebHelper webHelper)
        {
            this._workContext = workContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._logger = logger;
            this._paymentSettings = paymentSettings;
            this._localizationService = localizationService;
            this._webHelper = webHelper;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var payAnyWayPaymentSettings = _settingService.LoadSetting<PayAnyWayPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                MntId = payAnyWayPaymentSettings.MntId,
                MntTestMode = payAnyWayPaymentSettings.MntTestMode,
                MntDemoArea = payAnyWayPaymentSettings.MntDemoArea,
                Hashcode = payAnyWayPaymentSettings.Hashcode,
                AdditionalFee = payAnyWayPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = payAnyWayPaymentSettings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.MntIdOverrideForStore = _settingService.SettingExists(payAnyWayPaymentSettings, x => x.MntId, storeScope);
                model.MntTestModeOverrideForStore = _settingService.SettingExists(payAnyWayPaymentSettings, x => x.MntTestMode, storeScope);
                model.MntDemoAreaOverrideForStore = _settingService.SettingExists(payAnyWayPaymentSettings, x => x.MntDemoArea, storeScope);
                model.HashcodeOverrideForStore = _settingService.SettingExists(payAnyWayPaymentSettings, x => x.Hashcode, storeScope);
                model.AdditionalFeeOverrideForStore = _settingService.SettingExists(payAnyWayPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentageOverrideForStore = _settingService.SettingExists(payAnyWayPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
            }

            return View("~/Plugins/Payments.PayAnyWay/Views/Configure.cshtml", model);
        }
        
        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var payAnyWayPaymentSettings = _settingService.LoadSetting<PayAnyWayPaymentSettings>(storeScope);

            //save settings
            payAnyWayPaymentSettings.MntId = model.MntId;
            payAnyWayPaymentSettings.MntTestMode = model.MntTestMode;
            payAnyWayPaymentSettings.MntDemoArea = model.MntDemoArea;
            payAnyWayPaymentSettings.Hashcode = model.Hashcode;
            payAnyWayPaymentSettings.AdditionalFee = model.AdditionalFee;
            payAnyWayPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(payAnyWayPaymentSettings, x => x.MntId, model.MntIdOverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(payAnyWayPaymentSettings, x => x.MntTestMode, model.MntTestModeOverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(payAnyWayPaymentSettings, x => x.MntDemoArea, model.MntDemoAreaOverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(payAnyWayPaymentSettings, x => x.Hashcode, model.HashcodeOverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(payAnyWayPaymentSettings, x => x.AdditionalFee, model.AdditionalFeeOverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(payAnyWayPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentageOverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            return View("~/Plugins/Payments.PayAnyWay/Views/PaymentInfo.cshtml");
        }

        private bool CheckOrderData(Order order, string operationId, string signature, string currencyCode)
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var setting = _settingService.LoadSetting<PayAnyWayPaymentSettings>(storeScope);

            var model = PayAnyWayPaymentRequest.CreatePayAnyWayPaymentRequest(setting, order.CustomerId, order.OrderGuid, order.OrderTotal, currencyCode);
            
            var checkDataString = String.Format("{0}{1}{2}{3}{4}{5}{6}{7}", model.MntId, model.MntTransactionId, operationId, model.MntAmount, model.MntCurrencyCode, model.MntSubscriberId, model.MntTestMode, model.MntHashcode);

            return model.GetMD5(checkDataString) == signature;
        }

        private ContentResult GetResponse(string textToResponse, bool success = false)
        {
            var msg = success ? "SUCCESS" : "FAIL";

            if (!success)
                _logger.Error(String.Format("PayAnyWay. {0}", textToResponse));
           
            return Content(String.Format("{0}\r\nnopCommerce. {1}", msg, textToResponse), "text/plain", Encoding.UTF8);
        }

        [ValidateInput(false)]
        public ActionResult ConfirmPay()
        {
            var processor =
                _paymentService.LoadPaymentMethodBySystemName("Payments.PayAnyWay") as PayAnyWayPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("PayAnyWay module cannot be loaded");

            var orderId = _webHelper.QueryString<string>("MNT_TRANSACTION_ID");
            var signature = _webHelper.QueryString<string>("MNT_SIGNATURE");
            var operationId = _webHelper.QueryString<string>("MNT_OPERATION_ID");
            var currencyCode = _webHelper.QueryString<string>("MNT_CURRENCY_CODE");

            Guid orderGuid;
            if (!Guid.TryParse(orderId, out orderGuid))
            {
                return GetResponse("Invalid order GUID");
            }

            var order = _orderService.GetOrderByGuid(orderGuid);
            if (order == null)
            {
                return GetResponse("Order cannot be loaded");
            }

            var sb = new StringBuilder();
            sb.AppendLine("PayAnyWay:");
            try
            {
                foreach (KeyValuePair<string, string> kvp in HttpContext.Request.QueryString)
                {
                    sb.AppendLine(kvp.Key + ": " + kvp.Value);
                }
            }
            catch (InvalidCastException)
            {
                _logger.Warning("PayAnyWay. Can't cast HttpContext.Request.QueryString");
            }

            //order note
            order.OrderNotes.Add(new OrderNote
            {
                Note = sb.ToString(),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);

            //check order data by signature
            if (!CheckOrderData(order, operationId, signature, currencyCode))
            {
                return GetResponse("Invalid order data");
            }

            //mark order as paid
            if (_orderProcessingService.CanMarkOrderAsPaid(order))
            {
                _orderProcessingService.MarkOrderAsPaid(order);
            }

            return GetResponse("Your order has been paid", true);
        }

        public ActionResult Success(FormCollection form)
        {
            var orderId = _webHelper.QueryString<string>("MNT_TRANSACTION_ID");
            Order order = null;

            Guid orderGuid;
            if (Guid.TryParse(orderId, out orderGuid))
            {
                order = _orderService.GetOrderByGuid(orderGuid);
            }

            return order != null ? RedirectToRoute("CheckoutCompleted", new { orderId = order.Id }) : RedirectToAction("Index", "Home", new { area = String.Empty });
        }

        public ActionResult CancelOrder(FormCollection form)
        {
            var orderId = _webHelper.QueryString<string>("MNT_TRANSACTION_ID");
            Order order = null;
           
            Guid orderGuid;
            if (Guid.TryParse(orderId, out orderGuid))
            {
                order = _orderService.GetOrderByGuid(orderGuid);
            }

            return order != null ? RedirectToRoute("OrderDetails", new { orderId = order.Id }) : RedirectToAction("Index", "Home", new { area = String.Empty });
        }

        public ActionResult Return(FormCollection form)
        {
           return RedirectToAction("Index", "Home", new { area = String.Empty });
        }

        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            return new List<string>();
        }

        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            return new ProcessPaymentRequest();
        }
    }
}