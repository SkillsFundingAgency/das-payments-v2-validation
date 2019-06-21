using System.Configuration;

namespace SFA.DAS.Payments.Verification.Constants
{
    public static class Config
    {
        private static class Keys
        {
            public static readonly string PaymentsSchemaPrefixKey = "payments.schema.prefix";
            public static readonly string PaymentsDatabaseKey = "payments.database.name";
            public static readonly string EarningsDatabaseKey = "earnings.database.name";
            public static readonly string V2PaymentsDatabaseKey = "v2.payments.database.name";
            public static readonly string UkprnListKey = "ukprn.list";
            public static readonly string PeriodListKey = "period.list";
        }

        public static string PaymentsSchemaPrefix => ConfigurationManager.AppSettings[Keys.PaymentsSchemaPrefixKey];
        public static string PaymentsDatabase => ConfigurationManager.AppSettings[Keys.PaymentsDatabaseKey];
        public static string EarningsDatabase => ConfigurationManager.AppSettings[Keys.EarningsDatabaseKey];
        public static string V2PaymentsDatabase => ConfigurationManager.AppSettings[Keys.V2PaymentsDatabaseKey];
        public const int AmountDP = 3;

        public static string UkprnList
        {
            get => ConfigurationManager.AppSettings[Keys.UkprnListKey];
            set
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings[Keys.UkprnListKey].Value = value;
                config.Save();
                ConfigurationManager.RefreshSection("appSettings");
            } 
        }

        public static string PeriodList
        {
            get => ConfigurationManager.AppSettings[Keys.PeriodListKey];
            set
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings[Keys.PeriodListKey].Value = value;
                config.Save();
                ConfigurationManager.RefreshSection("appSettings");
            }
        }
    }
}
