using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace SFA.DAS.Payments.MetricsGeneration
{
   internal class Options
    {
        [Option('c', "collectionPeriod", Required = true, HelpText = "The collection period for which to calculate earnings")]
        public short CollectionPeriod { get; set; }

        [Option('c', "collectionPeriod", Required = true, HelpText = "The collection period for which to calculate earnings")]
        public short AcademicYear { get; set; }
    }
}
