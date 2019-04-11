using System;
using System.Diagnostics;

namespace SFA.DAS.Payments.Verification
{
    static class Log
    {
        private static readonly Stopwatch Stopwatch = new Stopwatch();

        public static void Initialise()
        {
            Stopwatch.Start();
        }

        public static void Write(string message)
        {
            Console.WriteLine($"{Stopwatch.ElapsedMilliseconds/1000/60:D2}:{(Stopwatch.ElapsedMilliseconds/1000) % 60:D2} - {message}");
        }
    }
}
