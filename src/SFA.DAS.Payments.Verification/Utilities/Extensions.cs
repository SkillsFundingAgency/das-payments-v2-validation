using System.ComponentModel;

namespace SFA.DAS.Payments.Verification.Utilities
{
    public static class Extensions
    {
        public static string Description<T>(this T source) where T : System.Enum
        {
            var fieldInfo = typeof(T).GetField(source.ToString());
            var descriptionAttributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (descriptionAttributes.Length > 0 &&
                descriptionAttributes[0] is DescriptionAttribute descriptionAttribute)
            {
                return descriptionAttribute.Description;
            }

            return source.ToString();
        }
    }
}
