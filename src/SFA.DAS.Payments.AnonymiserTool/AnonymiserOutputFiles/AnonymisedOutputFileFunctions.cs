using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SFA.DAS.Payments.AnonymiserTool.Dto;
using SFA.DAS.Payments.AnonymiserTool.OutputFiles;
using TinyCsvParser;

namespace SFA.DAS.Payments.AnonymiserTool.Io
{
    class AnonymisedOutputFileFunctions
    {
        public static Dictionary<long, ReadOptimisedProviderData> ReadAllAnonymisedFiles()
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

                var learners = AnonymisedOutputFileFunctions.ReadAnonymiserOutputFile(file);

                results.Add(ukprn, OptimiseData(ukprn, learners));
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
