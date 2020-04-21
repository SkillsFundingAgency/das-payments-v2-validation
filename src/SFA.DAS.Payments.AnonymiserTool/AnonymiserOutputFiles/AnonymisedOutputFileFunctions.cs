using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SFA.DAS.Payments.AnonymiserTool.Dto;
using TinyCsvParser;

namespace SFA.DAS.Payments.AnonymiserTool.AnonymiserOutputFiles
{
    static class AnonymisedOutputFileFunctions
    {
        public static async Task<Dictionary<long, ReadOptimisedProviderData>> ReadAllAnonymisedFiles()
        {
            var results = new Dictionary<long, ReadOptimisedProviderData>();

            var pathToFiles = ConfigurationManager.AppSettings[Program.FileLocationKey];
            var directoryContents = Directory.GetFiles(pathToFiles, "*.xml.learnrefs.csv");
            foreach (var file in directoryContents)
            {
                var ukprn = ExtractUkprn(file);
                if (ukprn == -1)
                {
                    continue;
                }

                var learners = ReadAnonymiserOutputFile(file);

                results.Add(ukprn, await OptimiseData(learners));
            }

            return results;
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


        private static Task<ReadOptimisedProviderData> OptimiseData(List<ChangedLearner> learners)
        {
            var result = new ReadOptimisedProviderData();
            var optimisedLearners = learners
                .ToLookup(x => x.OldUln)
                .ToDictionary(x => x.Key, x => x.ToList());

            result.OptimisedLearners = optimisedLearners;
            return Task.FromResult(result);
        }

        private static readonly Regex UkprnExtractor = new Regex(@".*?ILR-(?<ukprn>\d{8})-\d{4}-\d{8}.*\.XML\.learnrefs\.csv",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
    }
}
