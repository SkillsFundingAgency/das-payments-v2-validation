using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using SDA.DAS.Payments.ConsoleUtilities;

namespace SFA.DAS.Payments.AnonymiserTool.OutputFiles
{
    internal static class OutputFileUtilities
    {
        public static async Task SaveScript(string script, string filename)
        {
            var basePath = ConfigurationManager.AppSettings[Program.FileLocationKey];
            var folderPath = Path.Combine(basePath, "Scripts");
            var fullFileName = Path.Combine(folderPath, filename);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            File.WriteAllText(fullFileName, script);
            await Logger.Log($"Saved script: {fullFileName}");
        }
    }
}
