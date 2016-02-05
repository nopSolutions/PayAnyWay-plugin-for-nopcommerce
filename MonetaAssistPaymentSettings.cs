using System;
using System.Globalization;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.MonetaAssist.Models;
using Nop.Services.Localization;

namespace Nop.Plugin.Payments.MonetaAssist
{
    public class MonetaAssistPaymentSettings : ISettings
    {
        /// <summary>
        /// The store identifier in the MONETA.RU.
        /// </summary>
        public string MntId { get; set; }

        /// <summary>
        /// Indicating that the request is made in test mode
        /// </summary>
        public bool MntTestMode { get; set; }

        /// <summary>
        /// Hashcode
        /// </summary>
        public int Hashcode { get; set; }

        /// <summary>
        /// ISO currency code.
        /// </summary>
        public CurrencyCodes MntCurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }

        /// <summary>
        /// Additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }

        /// <summary>
        /// Create PaymentInfoModel by settings
        /// </summary>
        /// <param name="customerId">Customer id</param>
        /// <param name="orderGuid">Order GUID</param>
        /// <param name="orderTotal">Total sum</param>
        public PaymentInfoModel CreatePaymentInfoModel(int customerId, Guid orderGuid, decimal orderTotal)
        {
            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
                var workContext = EngineContext.Current.Resolve<IWorkContext>();

                return new PaymentInfoModel
                {
                    MntId = MntId,
                    MntTestMode = MntTestMode ? 1 : 0,
                    MntHashcode = Hashcode,
                    MntCurrencyCode = MntCurrencyCode.GetLocalizedEnum(localizationService, workContext).Replace(" ", ""),
                    MntSubscriberId = customerId,
                    MntTransactionId = orderGuid.ToString(),
                    MntAmount = String.Format(CultureInfo.InvariantCulture, "{0:0.00}", orderTotal)
                };
            
        }
    }

}
