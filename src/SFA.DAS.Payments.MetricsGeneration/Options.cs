using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace SFA.DAS.Payments.MetricsGeneration
{

    internal enum FilterMode
    {
        None,
        OnlySuccessful
    }

   internal class Options
    {
        [Option('c', "collectionPeriod", Required = true, HelpText = "The collection period for which to calculate earnings")]
        public short CollectionPeriod { get; set; }

        [Option('a', "academicYear", Required = true, HelpText = "The academic year for which to calculate earnings")]
        public short? AcademicYear { get; set; }

        [Option('f', "filterMode", HelpText = "valid options are \"None\" (Default) or OnlySuccessful")]
        public FilterMode ProcessingFilterMode { get; set; }
    }
}
