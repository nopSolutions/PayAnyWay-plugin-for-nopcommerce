using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Nop.Plugin.Payments.PayAnyWay
{
    public class PayAnyWayPaymentRequest
    {
        /// <summary>
        /// The store identifier in the MONETA.RU
        /// </summary>
        public string MntId { get; set; }

        /// <summary>
        /// Amount
        /// </summary>
       
        public string MntAmount { get; set; }

        /// <summary>
        /// Order GUID
        /// </summary>
        public string MntTransactionId { get; set; }

        /// <summary>
        /// ISO currency code
        /// </summary>
        public string MntCurrencyCode { get; set; }

        /// <summary>
        /// Customer id
        /// </summary>
        public int MntSubscriberId { get; set; }

        /// <summary>
        /// Hashcode
        /// </summary>
        public string MntHashcode { get; set; }

        /// <summary>
        /// Indicates that the request is sent in a test mode
        /// </summary>
        public int MntTestMode { get; set; }

        /// <summary>
        /// Code to identify the sender and check integrity of files
        /// </summary>
        public string MntSignature
        {
            get
            {
                var text =
                    String.Format("{0}{1}{2}{3}{4}{5}{6}", MntId, MntTransactionId, MntAmount, MntCurrencyCode,
                        MntSubscriberId, MntTestMode, MntHashcode);

                return GetMD5(text);
            }
        }

        /// <summary>
        /// Creates an MD5 hash sum from string
        /// </summary>
        /// <param name="strToMD5">String to create an MD5 hash sum</param>
        /// <returns>MD5 hash sum</returns>
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
        /// Creates a PayAnyWayPaymentRequest
        /// </summary>
        /// <param name="settings">PayAnyWay payment settings</param>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="orderGuid">Order GUID</param>
        /// <param name="orderTotal">Total sum</param>
        /// <param name="currencyCode">ISO currency code</param>
        public static PayAnyWayPaymentRequest CreatePayAnyWayPaymentRequest(PayAnyWayPaymentSettings settings,
            int customerId, Guid orderGuid, decimal orderTotal, string currencyCode)
        {
            return new PayAnyWayPaymentRequest
            {
                MntId = settings.MntId,
                MntTestMode = settings.MntTestMode ? 1 : 0,
                MntHashcode = settings.Hashcode,
                MntSubscriberId = customerId,
                MntTransactionId = orderGuid.ToString(),
                MntCurrencyCode = currencyCode,
                MntAmount = String.Format(CultureInfo.InvariantCulture, "{0:0.00}", orderTotal)
            };
        }
    }
}