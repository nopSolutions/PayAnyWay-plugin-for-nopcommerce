using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.PayAnyWay
{
    public class PayAnyWayPaymentSettings : ISettings
    {
        /// <summary>
        /// The store identifier in the PayAnyWay
        /// </summary>
        public string MntId { get; set; }

        /// <summary>
        /// Indicates that the request is sent in a test mode
        /// </summary>
        public bool MntTestMode { get; set; }

        /// <summary>
        /// Indicates that the request is sent to the demo area
        /// </summary>
        public bool MntDemoArea { get; set; }

        /// <summary>
        /// Hashcode
        /// </summary>
        public string Hashcode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }

        /// <summary>
        /// Additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }
    }
}
