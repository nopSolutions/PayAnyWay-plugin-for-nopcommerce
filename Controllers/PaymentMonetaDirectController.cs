using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.MonetaDirect.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.MonetaDirect.Controllers
{
    public class PaymentMonetaDirectController : BasePaymentController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private readonly PaymentSettings _paymentSettings;
        private readonly ILocalizationService _localizationService;
        private IWebHelper _webHelper;

        public PaymentMonetaDirectController(IWorkContext workContext,
            IStoreService storeService, 
            ISettingService settingService, 
            IPaymentService paymentService, 
            IOrderService orderService, 
            IOrderProcessingService orderProcessingService, 
            ILogger logger,
            PaymentSettings paymentSettings, 
            ILocalizationService localizationService, IWebHelper webHelper)
        {
            this._workContext = workContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._logger = logger;
            this._paymentSettings = paymentSettings;
            this._localizationService = localizationService;
            this._webHelper = webHelper;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var monetaDirectPaymentSettings = _settingService.LoadSetting<MonetaDirectPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                MntId = monetaDirectPaymentSettings.MntId,
                MntTestMode = monetaDirectPaymentSettings.MntTestMode,
                HeshCode = monetaDirectPaymentSettings.HeshCode,
                MntCurrencyCode = Convert.ToInt32(monetaDirectPaymentSettings.MntCurrencyCode),
                AdditionalFee = monetaDirectPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = monetaDirectPaymentSettings.AdditionalFeePercentage,
                MntCurrencyCodeValues = monetaDirectPaymentSettings.MntCurrencyCode.ToSelectList(),
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.MntIdOverrideForStore = _settingService.SettingExists(monetaDirectPaymentSettings, x => x.MntId, storeScope);
                model.MntTestModeOverrideForStore = _settingService.SettingExists(monetaDirectPaymentSettings, x => x.MntTestMode, storeScope);
                model.HeshCodeOverrideForStore = _settingService.SettingExists(monetaDirectPaymentSettings, x => x.HeshCode, storeScope);
                model.MntCurrencyCodeOverrideForStore = _settingService.SettingExists(monetaDirectPaymentSettings, x => x.MntCurrencyCode, storeScope);
                model.AdditionalFeeOverrideForStore = _settingService.SettingExists(monetaDirectPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentageOverrideForStore = _settingService.SettingExists(monetaDirectPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
            }

            return View("~/Plugins/Payments.MonetaDirect/Views/PaymentMonetaDirect/Configure.cshtml", model);
        }

        private void UpdateSetting<TPropType>(int storeScope, bool overrideForStore, MonetaDirectPaymentSettings settings, Expression<Func<MonetaDirectPaymentSettings, TPropType>> keySelector)
        {
            if (overrideForStore || storeScope == 0)
                _settingService.SaveSetting(settings, keySelector, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(settings, keySelector, storeScope);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var monetaDirectPaymentSettings = _settingService.LoadSetting<MonetaDirectPaymentSettings>(storeScope);

            //save settings
            monetaDirectPaymentSettings.MntId = model.MntId;
            monetaDirectPaymentSettings.MntCurrencyCode = (CurrencyCodes)model.MntCurrencyCode;
            monetaDirectPaymentSettings.MntTestMode = model.MntTestMode;
            monetaDirectPaymentSettings.HeshCode = model.HeshCode;
            monetaDirectPaymentSettings.AdditionalFee = model.AdditionalFee;
            monetaDirectPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            
            UpdateSetting(storeScope, model.MntIdOverrideForStore, monetaDirectPaymentSettings, x => x.MntId);
            UpdateSetting(storeScope, model.MntCurrencyCodeOverrideForStore, monetaDirectPaymentSettings, x => x.MntCurrencyCode);
            UpdateSetting(storeScope, model.MntTestModeOverrideForStore, monetaDirectPaymentSettings, x => x.MntTestMode);
            UpdateSetting(storeScope, model.HeshCodeOverrideForStore, monetaDirectPaymentSettings, x => x.HeshCode);
            UpdateSetting(storeScope, model.AdditionalFeeOverrideForStore, monetaDirectPaymentSettings, x => x.AdditionalFee);
            UpdateSetting(storeScope, model.AdditionalFeePercentageOverrideForStore, monetaDirectPaymentSettings, x => x.AdditionalFeePercentage);

            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            return View("~/Plugins/Payments.MonetaDirect/Views/PaymentMonetaDirect/PaymentInfo.cshtml");
        }

        [ValidateInput(false)]
        public ActionResult ConfirmPay()
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.MonetaDirect") as MonetaDirectPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("MonetaDirect module cannot be loaded");


            var orderId = _webHelper.QueryString<string>("MNT_TRANSACTION_ID");
            Guid orderGuid;
            if (Guid.TryParse(orderId, out orderGuid))
            {
                var order = _orderService.GetOrderByGuid(orderGuid);
                if (order == null)
                {
                    return Content("<html><body><p>nopCommerce. Order cannot be loaded</p></body></html>");
                }
                
                var customerId =_webHelper.QueryString<int>("MNT_SUBSCRIBER_ID");
                var signature = _webHelper.QueryString<string>("MNT_SIGNATURE");

                var setting = _settingService.LoadSetting<MonetaDirectPaymentSettings>();

                var model = setting.CreatePaymentInfoModel(customerId, orderGuid, order.OrderTotal);
                
                if (customerId != order.CustomerId || model.MntSignature != signature)
                {
                    return Content("<html><body><p>nopCommerce. Invalid order data</p></body></html>");
                }

                if (_orderProcessingService.CanMarkOrderAsPaid(order))
                {
                    _orderProcessingService.MarkOrderAsPaid(order);
                }
            }
            else
            {
                return Content("<html><body><p>nopCommerce. Invalid order id</p></body></html>");
            }
           
            return Content("<html><body><p>Your order has been paid</p></body></html>");
        }
        

        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            return new List<string>();
        }

        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            return new ProcessPaymentRequest();
        }
    }
}