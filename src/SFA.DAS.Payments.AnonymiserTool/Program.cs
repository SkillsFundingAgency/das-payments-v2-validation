using System;
using System.Linq;
using System.Threading.Tasks;
using SDA.DAS.Payments.ConsoleUtilities;
using SFA.DAS.Payments.AnonymiserTool.AnonymiserOutputFiles;
using SFA.DAS.Payments.AnonymiserTool.OutputFiles;
using SFA.DAS.Payments.AnonymiserTool.V2Database;

namespace SFA.DAS.Payments.AnonymiserTool
{
    static class Program
    {
        public static string FileLocationKey = "DcToolOutputDirectory";

        static async Task Main(string[] args)
        {
            try
            {
                await Logger.Log("Starting script generation...");
                // Load the list of changed ULNs and UKPRNs
                var anonymisedProviders = await AnonymisedOutputFileFunctions.ReadAllAnonymisedFiles();
                await Logger.Log($"Found {anonymisedProviders.Count} providers");

                // Load the list of commitments (include child tables)
                var apprenticeshipData = DatabaseUtilities.LoadProductionApprenticeships(anonymisedProviders.Keys.ToList());
                await Logger.Log("Loaded apprenticeships");

                // Alter the commitment ULNs
                var apprenticeshipsToRemove = await DataManipulation.AlterUlns(apprenticeshipData, anonymisedProviders);

                await Logger.Log("Updated commitments");

                await DataManipulation.RemoveApprenticeships(apprenticeshipData, apprenticeshipsToRemove);
                await Logger.Log("Removed existing apprenticeships");

                // Create a script to delete existing commitments with UKPRNs
                var removeByUkprnScript = ScriptGeneration.CreateDeleteByUkprnScript(apprenticeshipData);
                await OutputFileUtilities.SaveScript(removeByUkprnScript, "RemoveUkprns.sql");
                
                // And then insert the altered commitments 
                var createCommitmentsScript = await ScriptGeneration.CreateNewCommitmentsScript(apprenticeshipData);
                for (var i = 0; i < createCommitmentsScript.Count; i++)
                {
                    await OutputFileUtilities.SaveScript(createCommitmentsScript[i], $"CreateCommitments-{i+1}.sql");
                }
                
                // Add a script to remove the orphans
                var removeOrphansScript = ScriptGeneration.CreateRemoveOrphansScript();
                await OutputFileUtilities.SaveScript(removeOrphansScript, "RemoveOrphans.sql");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await Logger.Log(e.Message);
            }

            await Logger.Log("Completed. Press any key to exit");
            Console.ReadKey();
        }
    }
}
