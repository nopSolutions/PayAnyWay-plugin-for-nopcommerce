using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.PayAnyWay.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.PayAnyWay.Controllers
{
    public class PaymentPayAnyWayController : BasePaymentController
    {
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;

        public PaymentPayAnyWayController(ILocalizationService localizationService,
            ILogger logger,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPaymentService paymentService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWebHelper webHelper)
        {
            this._localizationService = localizationService;
            this._logger = logger;
            this._orderProcessingService = orderProcessingService;
            this._orderService = orderService;
            this._paymentService = paymentService;
            this._permissionService = permissionService;
            this._settingService = settingService;
            this._storeContext = storeContext;
            this._webHelper = webHelper;
        }

        private bool CheckOrderData(Order order, string operationId, string signature, string currencyCode)
        {
            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var setting = _settingService.LoadSetting<PayAnyWayPaymentSettings>(storeScope);

            var model = PayAnyWayPaymentRequest.CreatePayAnyWayPaymentRequest(setting, order.CustomerId, order.OrderGuid, order.OrderTotal, currencyCode);

            var checkDataString = $"{model.MntId}{model.MntTransactionId}{operationId}{model.MntAmount}{model.MntCurrencyCode.ToUpper()}{model.MntSubscriberId}{model.MntTestMode}{model.MntHashcode}";

            return model.GetMD5(checkDataString) == signature;
        }

        private ContentResult GetResponse(string textToResponse, bool success = false)
        {
            var msg = success ? "SUCCESS" : "FAIL";

            if (!success)
                _logger.Error($"PayAnyWay. {textToResponse}");

            return Content($"{msg}\r\nnopCommerce. {textToResponse}", "text/plain", Encoding.UTF8);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
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
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
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


        public IActionResult ConfirmPay()
        {
            var processor =
                _paymentService.LoadPaymentMethodBySystemName("Payments.PayAnyWay") as PayAnyWayPaymentProcessor;
            if (processor == null ||
                !_paymentService.IsPaymentMethodActive(processor) || !processor.PluginDescriptor.Installed)
                throw new NopException("PayAnyWay module cannot be loaded");

            var orderId = _webHelper.QueryString<string>("MNT_TRANSACTION_ID");
            var signature = _webHelper.QueryString<string>("MNT_SIGNATURE");
            var operationId = _webHelper.QueryString<string>("MNT_OPERATION_ID");
            var currencyCode = _webHelper.QueryString<string>("MNT_CURRENCY_CODE").ToUpper();

            if (!Guid.TryParse(orderId, out Guid orderGuid))
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
                foreach (var kvp in Request.Query)
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

        public IActionResult Success()
        {
            var orderId = _webHelper.QueryString<string>("MNT_TRANSACTION_ID");
            Order order = null;

            if (Guid.TryParse(orderId, out Guid orderGuid))
            {
                order = _orderService.GetOrderByGuid(orderGuid);
            }

            if (order == null)
                return RedirectToAction("Index", "Home", new {area = string.Empty});

            return RedirectToRoute("CheckoutCompleted", new {orderId = order.Id});
        }

        public IActionResult CancelOrder()
        {
            var orderId = _webHelper.QueryString<string>("MNT_TRANSACTION_ID");
            Order order = null;

            if (Guid.TryParse(orderId, out Guid orderGuid))
            {
                order = _orderService.GetOrderByGuid(orderGuid);
            }

            if (order == null)
                return RedirectToAction("Index", "Home", new {area = string.Empty});

            return RedirectToRoute("OrderDetails", new {orderId = order.Id});
        }

        public IActionResult Return()
        {
           return RedirectToAction("Index", "Home", new { area = string.Empty });
        }
    }
}