using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.MonetaAssist.Models;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.MonetaAssist.Controllers
{
    public class PaymentMonetaAssistController : BasePaymentController
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
        private readonly IWebHelper _webHelper;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;

        public PaymentMonetaAssistController(IWorkContext workContext,
            IStoreService storeService, 
            ISettingService settingService, 
            IPaymentService paymentService, 
            IOrderService orderService, 
            IOrderProcessingService orderProcessingService, 
            ILogger logger,
            PaymentSettings paymentSettings, 
            ILocalizationService localizationService, IWebHelper webHelper, CurrencyService currencyService, CurrencySettings currencySettings)
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
            _currencyService = currencyService;
            _currencySettings = currencySettings;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var monetaAssistPaymentSettings = _settingService.LoadSetting<MonetaAssistPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                MntId = monetaAssistPaymentSettings.MntId,
                MntTestMode = monetaAssistPaymentSettings.MntTestMode,
                Hashcode = monetaAssistPaymentSettings.Hashcode,
                AdditionalFee = monetaAssistPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = monetaAssistPaymentSettings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.MntIdOverrideForStore = _settingService.SettingExists(monetaAssistPaymentSettings, x => x.MntId, storeScope);
                model.MntTestModeOverrideForStore = _settingService.SettingExists(monetaAssistPaymentSettings, x => x.MntTestMode, storeScope);
                model.HashcodeOverrideForStore = _settingService.SettingExists(monetaAssistPaymentSettings, x => x.Hashcode, storeScope);
                model.AdditionalFeeOverrideForStore = _settingService.SettingExists(monetaAssistPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentageOverrideForStore = _settingService.SettingExists(monetaAssistPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
            }

            return View("~/Plugins/Payments.MonetaAssist/Views/PaymentMonetaAssist/Configure.cshtml", model);
        }

        private void UpdateSetting<TPropType>(int storeScope, bool overrideForStore, MonetaAssistPaymentSettings settings, Expression<Func<MonetaAssistPaymentSettings, TPropType>> keySelector)
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
            var monetaAssistPaymentSettings = _settingService.LoadSetting<MonetaAssistPaymentSettings>(storeScope);

            //save settings
            monetaAssistPaymentSettings.MntId = model.MntId;
            monetaAssistPaymentSettings.MntTestMode = model.MntTestMode;
            monetaAssistPaymentSettings.Hashcode = model.Hashcode;
            monetaAssistPaymentSettings.AdditionalFee = model.AdditionalFee;
            monetaAssistPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            
            UpdateSetting(storeScope, model.MntIdOverrideForStore, monetaAssistPaymentSettings, x => x.MntId);
            UpdateSetting(storeScope, model.MntTestModeOverrideForStore, monetaAssistPaymentSettings, x => x.MntTestMode);
            UpdateSetting(storeScope, model.HashcodeOverrideForStore, monetaAssistPaymentSettings, x => x.Hashcode);
            UpdateSetting(storeScope, model.AdditionalFeeOverrideForStore, monetaAssistPaymentSettings, x => x.AdditionalFee);
            UpdateSetting(storeScope, model.AdditionalFeePercentageOverrideForStore, monetaAssistPaymentSettings, x => x.AdditionalFeePercentage);

            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            return View("~/Plugins/Payments.MonetaAssist/Views/PaymentMonetaAssist/PaymentInfo.cshtml");
        }

        private bool CheckOrderData(Order order)
        {
            var currencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;

            var setting = _settingService.LoadSetting<MonetaAssistPaymentSettings>();
            var model = PaymentInfoModel.CreatePaymentInfoModel(setting, order.CustomerId, order.OrderGuid, order.OrderTotal, currencyCode);

            var signature = _webHelper.QueryString<string>("MNT_SIGNATURE");
            var operationId = _webHelper.QueryString<string>("MNT_OPERATION_ID");
           
            var checkDtataString =
                $"{ model.MntId}{ model.MntTransactionId}{operationId}{model.MntAmount}{model.MntCurrencyCode}{model.MntSubscriberId}{model.MntTestMode}{model.MntHashcode}";

            return model.GetMD5(checkDtataString) == signature;
        }

        private ContentResult GetResponse(string textToResponse, bool success = false)
        {
            var msg = success ? "SUCCESS" : "FAIL";
            return Content($"{msg}\r\nnopCommerce. {textToResponse}", "text/plain", Encoding.UTF8);
        }

        [ValidateInput(false)]
        public ActionResult ConfirmPay()
        {
            var processor =
                _paymentService.LoadPaymentMethodBySystemName("Payments.MonetaAssist") as MonetaAssistPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("MonetaAssist module cannot be loaded");


            var orderId = _webHelper.QueryString<string>("MNT_TRANSACTION_ID");
            Guid orderGuid;
            if (!Guid.TryParse(orderId, out orderGuid))
            {
                return GetResponse("Invalid order GUID");
            }

            var order = _orderService.GetOrderByGuid(orderGuid);
            if (order == null)
            {
                return GetResponse("Order cannot be loaded");
            }
            
            if (!CheckOrderData(order))
            {
                return GetResponse("Invalid order data");
            }

            if (_orderProcessingService.CanMarkOrderAsPaid(order))
            {
                _orderProcessingService.MarkOrderAsPaid(order);
            }

            return GetResponse("Your order has been paid", true);
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