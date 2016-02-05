using System;
using System.Collections.Generic;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.MonetaAssist.Controllers;
using Nop.Plugin.Payments.MonetaAssist.Models;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework;

namespace Nop.Plugin.Payments.MonetaAssist
{

    public class MonetaAssistPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly MonetaAssistPaymentSettings _monetaAssistPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        #endregion

        #region Ctor

        public MonetaAssistPaymentProcessor(MonetaAssistPaymentSettings monetaAssistPaymentSettings,
            ISettingService settingService,
            ICurrencyService currencyService, ICustomerService customerService,
            CurrencySettings currencySettings, IWebHelper webHelper,
            IOrderTotalCalculationService orderTotalCalculationService)
        {
            this._monetaAssistPaymentSettings = monetaAssistPaymentSettings;
            this._settingService = settingService;
            this._currencyService = currencyService;
            this._customerService = customerService;
            this._currencySettings = currencySettings;
            this._webHelper = webHelper;
            this._orderTotalCalculationService = orderTotalCalculationService;
        }

        #endregion

        #region Methods

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult {NewPaymentStatus = PaymentStatus.Pending};
        }

        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var customerId = postProcessPaymentRequest.Order.CustomerId;
            var orderGuid = postProcessPaymentRequest.Order.OrderGuid;
            var orderTotal = postProcessPaymentRequest.Order.OrderTotal;

            var currencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;

            var model = PaymentInfoModel.CreatePaymentInfoModel(_monetaAssistPaymentSettings, customerId, orderGuid, orderTotal, currencyCode);
           
            var post = new RemotePost
            {
                FormName = "PayPoint",
                Url = model.MonetaAssistantUrl
            };
            post.Add("MNT_ID", model.MntId);
            post.Add("MNT_TRANSACTION_ID", model.MntTransactionId);
            post.Add("MNT_CURRENCY_CODE", model.MntCurrencyCode);
            post.Add("MNT_AMOUNT", model.MntAmount);
            post.Add("MNT_TEST_MODE", model.MntTestMode.ToString());
            post.Add("MNT_SUBSCRIBER_ID", model.MntSubscriberId.ToString());
            post.Add("MNT_SIGNATURE", model.MntSignature);
            post.Post();
        }

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            return false;
        }

        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
                _monetaAssistPaymentSettings.AdditionalFee, _monetaAssistPaymentSettings.AdditionalFeePercentage);
            return result;
        }

        #region Not implemented methods
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }

        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }
        #endregion

        public bool CanRePostProcessPayment(Order order)
        {
            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            return !((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5);
        }

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentMonetaAssist";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.MonetaAssist.Controllers" }, { "area", null } };
        }

        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentMonetaAssist";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.MonetaAssist.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentMonetaAssistController);
        }
 
        public override void Install()
        {
            //settings
            var settings = new MonetaAssistPaymentSettings
            {
                MntTestMode = true,
            };
            _settingService.SaveSetting(settings);

            //locales

            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaAssist.Fields.MntId", "Store identifier");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaAssist.Fields.MntId.Hint", "Specify the account ID of your store on the website moneta.ru (MNT_ID)");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaAssist.Fields.MntTestMode", "Test mode");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaAssist.Fields.MntTestMode.Hint", "Check to enable test mode");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaAssist.Fields.Hashcode", "Hashcode");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaAssist.Fields.Hashcode.Hint", "Set the data integrity code");
            
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaAssist.Fields.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaAssist.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaAssist.Fields.AdditionalFeePercentage", "Additional fee. Use percentage");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaAssist.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.");

            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaAssist.Fields.RedirectionTip",
                "For payment you will be redirected to the website MONETA.RU");

            base.Install();
        }

        public override void Uninstall()
        {
            _settingService.DeleteSetting<MonetaAssistPaymentSettings>();

            this.DeletePluginLocaleResource("Plugins.Payments.MonetaAssist.Fields.MntId");
            this.DeletePluginLocaleResource("Plugins.Payments.MonetaAssist.Fields.MntTestMode");
            this.DeletePluginLocaleResource("Plugins.Payments.MonetaAssist.Fields.Hashcode");

            this.DeletePluginLocaleResource("Plugins.Payments.MonetaAssist.Fields.MntId.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.MonetaAssist.Fields.MntTestMode.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.MonetaAssist.Fields.Hashcode.Hint");

            this.DeletePluginLocaleResource("Plugins.Payments.MonetaAssist.Fields.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.MonetaAssist.Fields.AdditionalFee.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.MonetaAssist.Fields.AdditionalFeePercentage");
            this.DeletePluginLocaleResource("Plugins.Payments.MonetaAssist.Fields.AdditionalFeePercentage.Hint");

            this.DeletePluginLocaleResource("Plugins.Payments.MonetaAssist.Fields.RedirectionTip");

            base.Uninstall();
        }

        #endregion

        #region Properties
        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => false;

        public bool SupportRefund => false;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public bool SkipPaymentInfo => false;

        #endregion
    }
}