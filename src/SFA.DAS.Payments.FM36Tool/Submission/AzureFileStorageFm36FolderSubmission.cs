using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Files.Shares;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using SFA.DAS.Payments.FM36Tool.JobContext;

namespace SFA.DAS.Payments.FM36Tool.Submission
{
    public class AzureFileStorageFm36FolderSubmission
    {
        private readonly DcHelper _dcHelper;
        private readonly ShareClient _shareClient;

        public AzureFileStorageFm36FolderSubmission(DcHelper dcHelper, ShareClient shareClient)
        {
            _dcHelper = dcHelper ?? throw new ArgumentNullException(nameof(dcHelper));
            _shareClient = shareClient ?? throw new ArgumentNullException(nameof(shareClient));
        }

        public async Task<List<(long Ukprn, long JobId, int LearnerCount)>> SubmitFolder(string folderName, int academicYear, int collectionPeriod)
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

                var fm36 = Newtonsoft.Json.JsonConvert.DeserializeObject<FM36Global>(fm36Json);

                if (fm36 == null)
                    throw new InvalidOperationException($"Couldn't get the fm36 for file: {shareFileItem.Name} in folder: {folderName}");

                var learners = fm36.Learners;

                var jobId = new Random(Guid.NewGuid().GetHashCode()).Next(int.MaxValue);

                await _dcHelper.SubmitFm36(learners, fm36.UKPRN, (short)academicYear, (byte)collectionPeriod, jobId);
                
                result.Add((fm36.UKPRN, jobId, learners.Count));
            }

            return result;
        }
    }
}