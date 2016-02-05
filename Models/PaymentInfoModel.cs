using System;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.MonetaAssist.Models
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
        [NopResourceDisplayName("Plugins.Payments.MonetaAssist.Fields.Amount")]
        public string MntAmount { get; set; }

        /// <summary>
        /// Order GUID
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
        /// Customer id
        /// </summary>
        [AllowHtml]
        public int MntSubscriberId { get; set; }

        /// <summary>
        /// Hashcode
        /// </summary>
        public int MntHashcode { get; set; }

        /// <summary>
        /// Indicating that the request is made in test mode
        /// </summary>
        public int MntTestMode { get; set; }

        /// <summary>
        /// Code to identify the sender and check the integrity of files. 
        /// </summary>
        [AllowHtml]
        public string MntSignature
        {
            get
            {
                var text =
                    $"{MntId}{MntTransactionId}{MntAmount}{MntCurrencyCode}{MntSubscriberId}{MntTestMode}{MntHashcode}";

                return GetMD5(text);
            }
        }

        /// <summary>
        /// Create MD5 hesh sum from string
        /// </summary>
        /// <param name="strToMD5">string to create MD5 sum</param>
        /// <returns>MD5 hesh sum</returns>
        public string GetMD5(string strToMD5)
        {
            var enc = Encoding.Default.GetEncoder();
            var length = strToMD5.Length;
            var data = new byte[length];
            enc.GetBytes(strToMD5.ToCharArray(), 0, length, data, 0, true);
            byte[] result;

            using (var md5 = new MD5CryptoServiceProvider())
            {
                result = md5.ComputeHash(data);
            }

            return BitConverter.ToString(result)
                .Replace("-", string.Empty).ToLower();
        }

        /// <summary>
        /// MONETA.Assistant url
        /// </summary>
        public string MonetaAssistantUrl
        {
            get
            {
#if DEBUG
                return "https://demo.moneta.ru/assistant.htm";
#endif

#if !DEBUG
                return "https://www.payanyway.ru/assistant.htm";
#endif
            }
        }
    }
}