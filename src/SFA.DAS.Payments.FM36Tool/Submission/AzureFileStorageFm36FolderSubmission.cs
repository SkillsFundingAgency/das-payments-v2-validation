using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Files.Shares;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using SFA.DAS.Payments.FM36Tool.ApprenticeshipSetup;
using SFA.DAS.Payments.FM36Tool.JobContext;

namespace SFA.DAS.Payments.FM36Tool.Submission
{
    public class AzureFileStorageFm36FolderSubmission
    {
        private readonly DcHelper _dcHelper;
        private readonly ShareClient _shareClient;
        private readonly ApprenticeshipHelper _apprenticeshipHelper;

        public AzureFileStorageFm36FolderSubmission(DcHelper dcHelper, ShareClient shareClient, ApprenticeshipHelper apprenticeshipHelper)
        {
            _dcHelper = dcHelper ?? throw new ArgumentNullException(nameof(dcHelper));
            _shareClient = shareClient ?? throw new ArgumentNullException(nameof(shareClient));
            _apprenticeshipHelper = apprenticeshipHelper ?? throw new ArgumentNullException(nameof(apprenticeshipHelper));
        }

        public async Task<List<(long Ukprn, long JobId, int LearnerCount)>> SubmitFolder(string folderName, short academicYear, int collectionPeriod)
        {
            var folder = _shareClient.GetDirectoryClient(folderName);

            var files = folder.GetFilesAndDirectories();

            var result = new List<(long Ukprn, long JobId, int LearnerCount)>();

            foreach (var shareFileItem in files)
            {
                if (shareFileItem.IsDirectory)
                    continue;

                var file = folder.GetFileClient(shareFileItem.Name);

                var download = await file.DownloadAsync();

                string fm36Json;

                using (var memoryStream = new MemoryStream())
                {
                    await download.Value.Content.CopyToAsync(memoryStream);

                    memoryStream.Position = 0;

                    fm36Json = await (new StreamReader(memoryStream)).ReadToEndAsync();
                }

                var fm36 = Newtonsoft.Json.JsonConvert.DeserializeObject<FM36Global>(UpliftFm36ToCurrentAcademicYear(fm36Json, academicYear));

                if (fm36 == null)
                    throw new InvalidOperationException($"Couldn't get the fm36 for file: {shareFileItem.Name} in folder: {folderName}");

                var learners = fm36.Learners;

                //try
                //{
                //    await _apprenticeshipHelper.SetupTestApprenticeshipData(fm36, academicYear);
                //}
                //catch (Exception e)
                //{
                //    Console.WriteLine(e);
                //    throw;
                //}

                var jobId = new Random(Guid.NewGuid().GetHashCode()).Next(int.MaxValue);

                await _dcHelper.SubmitFm36(learners, fm36.UKPRN, academicYear, (byte)collectionPeriod, jobId);

                result.Add((fm36.UKPRN, jobId, learners.Count));
            }

            return result;
        }

        private static string UpliftFm36ToCurrentAcademicYear(string fm36, short academicYear)
        {
            ////2022
            //var calendarYear = academicYear / 100 + 2000;
            ////2021
            //var previousYear = calendarYear - 1;
            ////2020
            //var previousTwoYear = previousYear - 1;
            ////2019
            //var previousThreeYear = previousTwoYear - 1;
            ////2023
            //var nextYear = calendarYear + 1;

            //fm36 = fm36.Replace(calendarYear.ToString(), nextYear.ToString());

            //fm36 = fm36.Replace(previousYear.ToString(), calendarYear.ToString());

            //fm36 = fm36.Replace(previousTwoYear.ToString(), previousYear.ToString());

            //fm36 = fm36.Replace(previousThreeYear.ToString(), previousTwoYear.ToString());

            return fm36;
        }
    }
}