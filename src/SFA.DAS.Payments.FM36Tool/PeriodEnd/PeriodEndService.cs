using System;
using System.Threading.Tasks;
using SFA.DAS.Payments.FM36Tool.JobContext;

namespace SFA.DAS.Payments.FM36Tool.PeriodEnd
{
    public class PeriodEndService
    {
        private readonly DcHelper _dcHelper;

        public PeriodEndService(DcHelper dcHelper)
        {
            _dcHelper = dcHelper ?? throw new ArgumentNullException(nameof(dcHelper));
        }

        public async Task <long> SendPeriodEndTask(PeriodEndTask periodEndTask, short academicYear, int collectionPeriod)
        {
            var jobId = new Random(Guid.NewGuid().GetHashCode()).Next(int.MaxValue);
            await _dcHelper.SendPeriodEndTask(academicYear, (byte) collectionPeriod, jobId, periodEndTask);
            return jobId;
        }
    }
}