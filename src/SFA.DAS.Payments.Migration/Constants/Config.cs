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
        }

        public static string PaymentsSchemaPrefix => ConfigurationManager.AppSettings[Keys.PaymentsSchemaPrefixKey];
        public static string PaymentsDatabase => ConfigurationManager.AppSettings[Keys.PaymentsDatabaseKey];
        public static string EarningsDatabase => ConfigurationManager.AppSettings[Keys.EarningsDatabaseKey];
        public static string V2PaymentsDatabase => ConfigurationManager.AppSettings[Keys.V2PaymentsDatabaseKey];
        public const int DecimalPlacesToCompare = 3;
    }
}
