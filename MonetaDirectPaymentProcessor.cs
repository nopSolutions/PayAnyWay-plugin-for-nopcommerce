using System;
using System.Collections.Generic;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Plugins;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;

namespace Nop.Plugin.Payments.MonetaDirect
{

    public class MonetaDirectPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly MonetaDirectPaymentSettings _monetaDirectPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        #endregion

        #region Ctor

        public MonetaDirectPaymentProcessor(MonetaDirectPaymentSettings monetaDirectPaymentSettings,
            ISettingService settingService,
            ICurrencyService currencyService, ICustomerService customerService,
            CurrencySettings currencySettings, IWebHelper webHelper,
            IOrderTotalCalculationService orderTotalCalculationService)
        {
            this._monetaDirectPaymentSettings = monetaDirectPaymentSettings;
            this._settingService = settingService;
            this._currencyService = currencyService;
            this._customerService = customerService;
            this._currencySettings = currencySettings;
            this._webHelper = webHelper;
            this._orderTotalCalculationService = orderTotalCalculationService;
        }

        #endregion

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            throw new NotImplementedException();
        }

        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            throw new NotImplementedException();
        }

        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            throw new NotImplementedException();
        }

        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public bool CanRePostProcessPayment(Order order)
        {
            throw new NotImplementedException();
        }

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentMonetaDirect";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.MonetaDirect.Controllers" }, { "area", null } };
        }

        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            throw new NotImplementedException();
        }

        public Type GetControllerType()
        {
            throw new NotImplementedException();
        }
 
        public override void Install()
        {
            //settings
            var settings = new MonetaDirectPaymentSettings
            {
                MntTestMode = true,
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.Amount", "Amount");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.MntId", "Store identifier");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.MntTestMode", "Is made in test mode");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.HeshCode", "Hesh-code");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.MntCurrencyCode", "ISO currency code");
           
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.AdditionalFeePercentage", "Additional fee. Use percentage");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.");

            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.RedirectionTip",
                "For payment you will be redirected to the website MONETA.RU");

            base.Install();
        }

        public override void Uninstall()
        {
            _settingService.DeleteSetting<MonetaDirectPaymentSettings>();

            this.DeletePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.Amount");
            this.DeletePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.MntId");
            this.DeletePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.MntTestMode");
            this.DeletePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.HeshCode");
            this.DeletePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.MntCurrencyCode");
    
            this.DeletePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.AdditionalFee.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.AdditionalFeePercentage");
            this.DeletePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.AdditionalFeePercentage.Hint");

            this.DeletePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.RedirectionTip");

            base.Uninstall();
        }
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