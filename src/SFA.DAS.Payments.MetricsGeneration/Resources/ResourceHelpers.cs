using System.IO;
using System.Reflection;

namespace SFA.DAS.Payments.MetricsGeneration.Resources
{
    public static class ResourceHelpers
    {
        internal static Stream OpenResource(string filename)
        {
            var assembly = Assembly.GetEntryAssembly();

            var stream = assembly
                .GetManifestResourceStream($"{assembly.GetName().Name}.Resources.{filename}");
            return stream;
        }


        internal static string ReadResource(string filename)
        {
            var stream = OpenResource(filename);
            string text;
            using (var reader = new StreamReader(stream))
            {
                text = reader.ReadToEnd();
            }

            return text;
        }
    }
}