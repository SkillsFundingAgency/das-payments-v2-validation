using System;
using System.IO;
using System.Threading.Tasks;

namespace SDA.DAS.Payments.ConsoleUtilities
{
    public class Logger
    {
        public static async Task Log(string message, int offset = 0)
        {
            var tabs = string.Empty;
            for (var i = 0; i < offset; i++)
            {
                tabs += "\t";
            }

            var modifiedMessage = $"{DateTime.Now:hh:mm:ss}: {tabs}{message}";
            using (var file = File.AppendText("log.txt"))
            {
                await file.WriteLineAsync(modifiedMessage);
            }
            Console.WriteLine(modifiedMessage);
        }
    }
}
