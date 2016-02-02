using System;
using System.Collections.Generic;
using System.Web.Routing;
using Nop.Core.Domain.Orders;
using Nop.Core.Plugins;
using Nop.Services.Payments;

namespace Nop.Plugin.Payments.MonetaDirect
{
    /// <summary>
    /// PayPalDirect payment processor
    /// </summary>
    public class MonetaDirectPaymentProcessor : BasePlugin, IPaymentMethod
    {
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
            throw new NotImplementedException();
        }

        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            throw new NotImplementedException();
        }

        public Type GetControllerType()
        {
            throw new NotImplementedException();
        }

        public bool SupportCapture { get; }
        public bool SupportPartiallyRefund { get; }
        public bool SupportRefund { get; }
        public bool SupportVoid { get; }
        public RecurringPaymentType RecurringPaymentType { get; }
        public PaymentMethodType PaymentMethodType { get; }
        public bool SkipPaymentInfo { get; }

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
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.AdditionalFeePercentage", "");

            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.AdditionalFee", "");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.AdditionalFeePercentage", "Additional fee. Use percentage");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MonetaDirect.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.");


            base.Install();
        }

    }
}