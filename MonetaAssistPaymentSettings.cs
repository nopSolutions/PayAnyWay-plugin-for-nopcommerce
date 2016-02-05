using Nop.Core.Configuration;

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
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }

        /// <summary>
        /// Additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }
    }

}
