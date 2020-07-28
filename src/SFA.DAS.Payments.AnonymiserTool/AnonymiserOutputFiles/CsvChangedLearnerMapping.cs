using SFA.DAS.Payments.AnonymiserTool.Dto;
using TinyCsvParser.Mapping;

namespace SFA.DAS.Payments.AnonymiserTool.AnonymiserOutputFiles
{
    internal class CsvChangedLearnerMapping : CsvMapping<ChangedLearner>
    {
        public CsvChangedLearnerMapping()
        {
            MapProperty(2, x => x.OldUln);
            MapProperty(3, x => x.NewUln);
        }
    }
}