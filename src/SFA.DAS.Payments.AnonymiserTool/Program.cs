using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dapper;
using SDA.DAS.Payments.ConsoleUtilities;
using SFA.DAS.Payments.AnonymiserTool.Constants;
using SFA.DAS.Payments.AnonymiserTool.DatabaseEntities;
using TinyCsvParser;
using TinyCsvParser.Mapping;

namespace SFA.DAS.Payments.AnonymiserTool
{
    class Program
    {
        public static string FileLocationKey = "DcToolOutputDirectory";

        static async Task Main(string[] args)
        {
            try
            {
                await Logger.Log("Starting script generation...");
                // Load the list of changed ULNs and UKPRNs
                var anonymisedProviders = ReadAllAnonymisedFiles();
                await Logger.Log($"Found {anonymisedProviders.Count} providers");

                // Load the list of commitments (include child tables)
                var apprenticeshipData = LoadProductionApprenticeships(anonymisedProviders.Keys.ToList());
                await Logger.Log("Loaded apprenticeships");

                // Alter the commitment ULNs
                var apprenticeshipsToRemove = new List<Apprenticeship>();
                foreach (var apprenticeship in apprenticeshipData.Apprenticeships)
                {
                    var ukprn = apprenticeship.Ukprn;
                    if (!anonymisedProviders.ContainsKey(ukprn))
                    {
                        apprenticeshipsToRemove.Add(apprenticeship);
                        continue;
                    }

                    var providerData = anonymisedProviders[ukprn];
                    if (!providerData.OptimisedLearners.ContainsKey(apprenticeship.Uln))
                    {
                        apprenticeshipsToRemove.Add(apprenticeship);
                        continue;
                    }

                    var listOfChangedLearners = providerData.OptimisedLearners[apprenticeship.Uln];
                    foreach (var changedLearner in listOfChangedLearners)
                    {
                        if (changedLearner.OldUln != apprenticeship.Uln)
                        {
                            await Logger.Log($"Multiple learners for UKPRN: {ukprn} and ULN: {apprenticeship.Uln} - results are not guaranteed");
                            foreach (var learner in listOfChangedLearners)
                            {
                                await Logger.Log($"New ULN: {learner.NewUln}", 1);
                            }
                        }
                        apprenticeship.Uln = changedLearner.NewUln;
                    }
                }

                await Logger.Log("Updated commitments");

                await DataManipulation.RemoveApprenticeships(apprenticeshipData, apprenticeshipsToRemove);
                await Logger.Log("Removed existing apprenticeships");

                // Create a script to delete existing commitments with UKPRNs
                var removeByUkprnScript = ScriptGeneration.CreateDeleteByUkprnScript(apprenticeshipData);
                await SaveScript(removeByUkprnScript, "RemoveUkprns.sql");
                
                // And then insert the altered commitments 
                var createCommitmentsScript = await ScriptGeneration.CreateNewCommitmentsScript(apprenticeshipData);
                await SaveScript(createCommitmentsScript, "CreateCommitments.sql");
                
                // Add a script to remove the orphans
                var removeOrphansScript = ScriptGeneration.CreateRemoveOrphansScript();
                await SaveScript(removeOrphansScript, "RemoveOrphans.sql");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await Logger.Log(e.Message);
                Console.ReadKey();
            }

            await Logger.Log("Completed. Press any key to exit");
            Console.ReadKey();
        }
        
        static async Task SaveScript(string script, string filename)
        {
            var basePath = ConfigurationManager.AppSettings[FileLocationKey];
            var folderPath = Path.Combine(basePath, "Scripts");
            var fullFileName = Path.Combine(folderPath, filename);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            File.WriteAllText(fullFileName, script);
            await Logger.Log($"Saved script: {fullFileName}");
        }

        static ApprenticeshipData LoadProductionApprenticeships(List<long> ukprns)
        {
            var result = new ApprenticeshipData();

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ProductionV2DatabaseConnectionString"].ConnectionString))
            {
                var apprenticeships = connection.Query<Apprenticeship>(Sql.Apprenticeships, new {ukprns}, commandTimeout: 3600);
                var priceEpisodes = connection.Query<ApprenticeshipPriceEpisode>(Sql.ApprenticeshipPriceEpisodes, new {ukprns}, commandTimeout: 3600);
                var pauses = connection.Query<ApprenticeshipPause>(Sql.ApprenticeshipPauses, new {ukprns}, commandTimeout: 3600);

                result.Apprenticeships.AddRange(apprenticeships);
                result.ApprenticeshipPauses.AddRange(pauses);
                result.ApprenticeshipPriceEpisodes.AddRange(priceEpisodes);
            }

            return result;
        }

        static readonly Regex UkprnExtractor = new Regex(@".*?ILR-(?<ukprn>\d{8})-\d{4}-\d{8}.*\.XML\.learnrefs\.csv",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Dictionary<long, ReadOptimisedProviderData> ReadAllAnonymisedFiles()
        {
            var results = new Dictionary<long, ReadOptimisedProviderData>();

            var pathToFiles = ConfigurationManager.AppSettings[FileLocationKey];
            var directoryContents = Directory.GetFiles(pathToFiles, "*.xml.learnrefs.csv");
            foreach (var file in directoryContents)
            {
                var ukprn = ExtractUkprn(file);
                if (ukprn == -1)
                {
                    continue;
                }

                var learners = ReadAnonymiserOutputFile(file);

                results.Add(ukprn, OptimiseData(ukprn, learners));
            }

            return results;
        }

        private static ReadOptimisedProviderData OptimiseData(long ukprn, List<ChangedLearner> learners)
        {
            var result = new ReadOptimisedProviderData();
            result.Ukprn = ukprn;
            var optimisedLearners = learners.ToLookup(x => x.OldUln);
            foreach (var optimisedLearner in optimisedLearners)
            {
                result.OptimisedLearners.Add(optimisedLearner.Key, optimisedLearner.ToList());
            }

            return result;
        }

        private static long ExtractUkprn(string filePath)
        {
            var match = UkprnExtractor.Match(filePath);
            if (match.Success && match.Groups["ukprn"].Success)
            {
                var ukprnAsString = match.Groups["ukprn"].Value;
                if (long.TryParse(ukprnAsString, out long result))
                {
                    return result;
                }
            }

            return -1;
        }

        private static List<ChangedLearner> ReadAnonymiserOutputFile(string filename)
        {
            var csvParserOptions = new CsvParserOptions(false, ',');
            var csvMapper = new CsvChangedLearnerMapping();
            var csvParser = new CsvParser<ChangedLearner>(csvParserOptions, csvMapper);

            var result = csvParser
                .ReadFromFile(filename, Encoding.ASCII)
                .ToList();

            if (result.Any(x => !x.IsValid))
            {
                throw new ApplicationException(string.Join(", ", result.Where(x => !x.IsValid).Select(x => x.Error.ColumnIndex)));
            }

            return result.Select(x => x.Result).ToList();
        }
    }

    class ApprenticeshipData
    {
        public List<Apprenticeship> Apprenticeships { get; set; } = new List<Apprenticeship>();
        public List<ApprenticeshipPause> ApprenticeshipPauses { get; set; } = new List<ApprenticeshipPause>();
        public List<ApprenticeshipPriceEpisode> ApprenticeshipPriceEpisodes { get; set; } = new List<ApprenticeshipPriceEpisode>();
    }

    class ReadOptimisedProviderData
    {
        public long Ukprn { get; set; }
        public Dictionary<long, List<ChangedLearner>> OptimisedLearners { get; set; } = new Dictionary<long, List<ChangedLearner>>();
    }

    class CsvChangedLearnerMapping : CsvMapping<ChangedLearner>
    {
        public CsvChangedLearnerMapping()
        {
            MapProperty(2, x => x.OldUln);
            MapProperty(3, x => x.NewUln);
        }
    }

    class ChangedLearner
    {
        public long OldUln { get; set; }
        public long NewUln { get; set; }
    }
}
