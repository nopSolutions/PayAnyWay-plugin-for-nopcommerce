using System;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.MonetaDirect.Models
{
    public class PaymentInfoModel : BaseNopModel
    {
        /// <summary>
        /// The store identifier in the MONETA.RU.
        /// </summary>
        [AllowHtml]
        public string MntId { get; set; }

        /// <summary>
        /// Amount
        /// </summary>
        [AllowHtml]
        [NopResourceDisplayName("Plugins.Payments.MonetaDirect.Fields.Amount")]
        public string MntAmount { get; set; }

        /// <summary>
        /// Order ID
        /// </summary>
        [AllowHtml]
        public string MntTransactionId { get; set; }

        /// <summary>
        /// ISO currency code.
        /// 
        /// Possible values: RUB, USD, EUR 
        /// </summary>
        [AllowHtml]
        public string MntCurrencyCode { get; set; }

        /// <summary>
        /// User id
        /// </summary>
        [AllowHtml]
        public string MntSubscriberId { get; set; }

        /// <summary>
        /// Hesh-code
        /// </summary>
        public int HeshCode { get; set; }

        /// <summary>
        /// Indicating that the request is made in test mode
        /// </summary>
        public bool MntTestMode { get; set; }

        /// <summary>
        /// Code to identify the sender and check the integrity of files. 
        /// </summary>
        [AllowHtml]
        public string MntSignature
        {
            get
            {
                var text = $"{MntId}{MntTransactionId}{MntAmount}{MntCurrencyCode}{MntSubscriberId}{(MntTestMode ? 1 : 0)}{HeshCode}";

                var enc = Encoding.Default.GetEncoder();
                var length = text.Length;
                var data = new byte[length];
                enc.GetBytes(text.ToCharArray(), 0, length, data, 0, true);
                byte[] result;
                
                using (var md5 = new MD5CryptoServiceProvider())
                {
                    result = md5.ComputeHash(data);
                }

                return BitConverter.ToString(result)
                    .Replace("-", string.Empty).ToLower();
            }
        }
    }
}