using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDA.DAS.Payments.ConsoleUtilities;
using SFA.DAS.Payments.AnonymiserTool.DatabaseEntities;
using SFA.DAS.Payments.AnonymiserTool.Dto;

namespace SFA.DAS.Payments.AnonymiserTool.AnonymiserOutputFiles
{
    class ScriptGeneration
    {
        public static string CreateDeleteByUkprnScript(ApprenticeshipData apprenticeshipData)
        {
            var ukprns = apprenticeshipData.Apprenticeships
                .Select(x => x.Ukprn)
                .Distinct()
                .ToList();

            var stringBuilder = new StringBuilder();

            var position = 0;

            var ukprnsToRemove = ukprns.Skip(position).Take(500).ToList();
            while (ukprnsToRemove.Any())
            {
                stringBuilder.AppendLine("DELETE Payments2.ApprenticeshipPriceEpisode");
                stringBuilder.AppendLine("WHERE ApprenticeshipId IN (SELECT Id ");
                stringBuilder.AppendLine("FROM Payments2.Apprenticeship WHERE Ukprn IN @ukprns)");

                stringBuilder.AppendLine("DELETE Payments2.ApprenticeshipPause");
                stringBuilder.AppendLine("WHERE ApprenticeshipId IN (SELECT Id ");
                stringBuilder.AppendLine("FROM Payments2.Apprenticeship WHERE Ukprn IN @ukprns)");

                stringBuilder.AppendLine("DELETE Payments2.Apprenticeship ");
                stringBuilder.AppendLine("WHERE Ukprn IN (");
                stringBuilder.AppendLine(string.Join(",", ukprnsToRemove));
                stringBuilder.AppendLine(")");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();

                position += 500;
                ukprnsToRemove = ukprns.Skip(position).Take(500).ToList();
            }

            return stringBuilder.ToString();
        }

        public static string CreateRemoveOrphansScript()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("DELETE Payments2.ApprenticeshipPause");
            stringBuilder.AppendLine("WHERE ApprenticeshipId NOT IN (SELECT Id FROM Payments2.Apprenticeship)");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("DELETE Payments2.ApprenticeshipPriceEpisode");
            stringBuilder.AppendLine("WHERE ApprenticeshipId NOT IN (SELECT Id FROM Payments2.Apprenticeship)");
            stringBuilder.AppendLine();

            return stringBuilder.ToString();
        }

        public static async Task<List<string>> CreateNewCommitmentsScript(ApprenticeshipData apprenticeshipData)
        {
            var results = new List<string>();

            var stringBuilder = new StringBuilder();

            await Logger.Log("Structuring the data for easier lookups");

            var priceEpisdesByApprenticeshipId = apprenticeshipData
                .ApprenticeshipPriceEpisodes
                .ToLookup(x => x.ApprenticeshipId);

            var pausesByApprenticeshipId = apprenticeshipData
                .ApprenticeshipPauses
                .ToLookup(x => x.ApprenticeshipId);

            var position = 0;
            var batch = 100;
           
            var apprenticeships = apprenticeshipData.Apprenticeships.Skip(position).Take(batch).ToList();
            while (apprenticeships.Any())
            {
                /*      DELETE EXISTING     */
                var apprenticeshipIds = apprenticeships.Select(x => x.Id).ToList();
                stringBuilder.Append(CreateDeleteByApprenticeshipId(apprenticeshipIds));


                /*--------------------- APPRENTICESHIPS -------------------------*/
                /* --------------------------------------------------------------*/

                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("INSERT INTO Payments2.Apprenticeship ");
                stringBuilder.AppendLine("(Id, AccountId, AgreementId, AgreedOnDate, ULN, UKPRN, " +
                                         "EstimatedStartDate, EstimatedEndDate, Priority, StandardCode, " +
                                         "ProgrammeType, FrameworkCode, PathwayCode, LegalEntityName, " +
                                         "TransferSendingEmployerAccountId, StopDate, [Status], IsLevyPayer, " +
                                         "ApprenticeshipEmployerType)");
                stringBuilder.AppendLine("VALUES");

                foreach (var apprenticeship in apprenticeships)
                {
                    stringBuilder.AppendLine($"({apprenticeship.Id}, " +
                                             $"{apprenticeship.AccountId}, '{apprenticeship.AgreementId}', " +
                                             $"{DateToSql(apprenticeship.AgreedOnDate)}, " +
                                             $"{apprenticeship.Uln}, {apprenticeship.Ukprn}, " +
                                             $"{DateToSql(apprenticeship.EstimatedStartDate)}, " +
                                             $"{DateToSql(apprenticeship.EstimatedEndDate)}, " +
                                             $"{apprenticeship.Priority}, {apprenticeship.StandardCode}, " +
                                             $"{apprenticeship.ProgrammeType}, {apprenticeship.FrameworkCode}, " +
                                             $"{apprenticeship.PathwayCode}, '{apprenticeship.LegalEntityName.Replace("'","''")}', " +
                                             $"{LongToSql(apprenticeship.TransferSendingemployerAccountId)}," +
                                             $"{DateToSql(apprenticeship.StopDate)}, {apprenticeship.Status}," +
                                             $"{BoolToSql(apprenticeship.IsLevyPayer)}, {apprenticeship.ApprenticeshipEmployerType}),");
                }

                stringBuilder.Remove(stringBuilder.Length - 3, 1);

                /*--------------- APPRENTICESHIP PRICE EPISODES -----------------*/
                /* --------------------------------------------------------------*/

                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("INSERT INTO Payments2.ApprenticeshipPriceEpisode");
                stringBuilder.AppendLine("(ApprenticeshipId, StartDate, EndDate, Cost, Removed)");
                stringBuilder.AppendLine("VALUES");

                foreach (var apprenticeship in apprenticeships)
                {
                    if (!priceEpisdesByApprenticeshipId.Contains(apprenticeship.Id))
                    {
                        continue;
                    }

                    var priceEpisodes = priceEpisdesByApprenticeshipId[apprenticeship.Id];
                    foreach (var apprenticeshipPriceEpisode in priceEpisodes)
                    {
                        stringBuilder.AppendLine($"({apprenticeship.Id}, " +
                                                 $"{DateToSql(apprenticeshipPriceEpisode.StartDate)}, " +
                                                 $"{DateToSql(apprenticeshipPriceEpisode.EndDate)}, " +
                                                 $"{apprenticeshipPriceEpisode.Cost}, " +
                                                 $"0),");
                    }
                }

                stringBuilder.Remove(stringBuilder.Length - 3, 1);

                /*--------------- APPRENTICESHIP Pauses -------------------------*/
                /* --------------------------------------------------------------*/

                var pausesToAdd = new List<ApprenticeshipPause>();
                foreach (var apprenticeship in apprenticeships)
                {
                    if (!pausesByApprenticeshipId.Contains(apprenticeship.Id))
                    {
                        continue;
                    }

                    var pauses = pausesByApprenticeshipId[apprenticeship.Id];
                    pausesToAdd.AddRange(pauses);
                }

                if (pausesToAdd.Any())
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine("INSERT INTO Payments2.ApprenticeshipPause");
                    stringBuilder.AppendLine("(ApprenticeshipId, PauseDate, ResumeDate)");
                    stringBuilder.AppendLine("VALUES");
                    
                    foreach (var pause in pausesToAdd)
                    {
                        stringBuilder.AppendLine($"({pause.ApprenticeshipId}, " +
                                                 $"{DateToSql(pause.PauseDate)}, " +
                                                 $"{DateToSql(pause.ResumeDate)}),");
                    }

                    stringBuilder.Remove(stringBuilder.Length - 3, 1);
                }

                stringBuilder.AppendLine("GO");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();

                /*--------------- END LOOP  -------------------------------------*/
                /* --------------------------------------------------------------*/

                position += batch;
                apprenticeships = apprenticeshipData.Apprenticeships.Skip(position).Take(batch).ToList();

                if (position % 10000 == 0)
                {
                    await Logger.Log($"Processed {position} apprenticeships", 1);
                }

                if (position % 100000 == 0)
                {
                    results.Add(stringBuilder.ToString());
                    stringBuilder.Clear();
                }
            }

            results.Add(stringBuilder.ToString());

            return results;
        }

        private static StringBuilder CreateDeleteByApprenticeshipId(List<long> apprenticeshipIds)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("DELETE Payments2.ApprenticeshipPriceEpisode");
            stringBuilder.AppendLine("WHERE ApprenticeshipId IN (");
            stringBuilder.AppendLine(string.Join(",", apprenticeshipIds));
            stringBuilder.AppendLine(")");

            stringBuilder.AppendLine("DELETE Payments2.ApprenticeshipPause");
            stringBuilder.AppendLine("WHERE ApprenticeshipId IN (");
            stringBuilder.AppendLine(string.Join(",", apprenticeshipIds));
            stringBuilder.AppendLine(")");

            stringBuilder.AppendLine("DELETE Payments2.Apprenticeship ");
            stringBuilder.AppendLine("WHERE Id IN (");
            stringBuilder.AppendLine(string.Join(",", apprenticeshipIds));
            stringBuilder.AppendLine(")");

            return stringBuilder;
        }

        private static string DateToSql(DateTime? input)
        {
            if (input == null)
            {
                return "NULL";
            }

            return $"'{input?.ToString("O")}'";
        }

        private static string LongToSql(long? input)
        {
            if (input == null)
            {
                return "NULL";
            }

            return input.ToString();
        }

        private static string BoolToSql(bool input)
        {
            return input ? "1" : "0";
        }
    }
}
