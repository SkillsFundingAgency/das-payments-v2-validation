using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Payments.Contingency.CongingencyStrategies;

namespace SFA.DAS.Payments.Contingency
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var period = GetPeriod();

                var contingencyStrategies = new List<IProduceContingencyPayments>
                {
                    new Act1WithPaymentsAndDatalock1And2(),
                    new Act2WithPayments(),
                };

                foreach (var contingencyStrategy in contingencyStrategies)
                {
                    await contingencyStrategy.GenerateContingencyPayments(period);
                    (contingencyStrategy as IDisposable)?.Dispose();
                    GC.Collect();
                }

                Console.WriteLine("Finished - press enter to exit...");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }
        
        private static int GetPeriod()
        {
            while (true)
            {
                Console.WriteLine(
                    "Please enter the collection period: 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 or 14");
                var chosenPeriod = Console.ReadLine();
                if (!int.TryParse(chosenPeriod, out var collectionPeriod) || collectionPeriod < 1 ||
                    collectionPeriod > 14)
                {
                    Console.WriteLine($"Invalid collection period: '{chosenPeriod}'.");
                    continue;
                }

                return collectionPeriod;
            }
        }
    }
}
