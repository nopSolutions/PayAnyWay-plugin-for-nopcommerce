﻿using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.MonetaAssist.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }


        /// <summary>
        /// The store identifier in the MONETA.RU.
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.MonetaAssist.Fields.MntId")]
        public string MntId { get; set; }
        public bool MntIdOverrideForStore { get; set; }

        /// <summary>
        /// Indicating that the request is made in test mode
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.MonetaAssist.Fields.MntTestMode")]
        public bool MntTestMode { get; set; }
        public bool MntTestModeOverrideForStore { get; set; }

        /// <summary>
        /// Hesh-code
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.MonetaAssist.Fields.HeshCode")]
        public int HeshCode { get; set; }
        public bool HeshCodeOverrideForStore { get; set; }

        /// <summary>
        /// ISO currency code.
        /// </summary>
        public int MntCurrencyCode { get; set; }
        public bool MntCurrencyCodeOverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Payments.MonetaAssist.Fields.MntCurrencyCode")]
        public SelectList MntCurrencyCodeValues { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.MonetaAssist.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentageOverrideForStore { get; set; }

        /// <summary>
        /// Additional fee
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.MonetaAssist.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFeeOverrideForStore { get; set; }
    }
}