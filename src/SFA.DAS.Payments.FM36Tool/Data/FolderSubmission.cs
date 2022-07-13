using System;

namespace SFA.DAS.Payments.FM36Tool.Data
{
    public class FolderSubmission
    {
        public void InitialiseAcademicYear(string folderName)
        {
            var month = DateTime.UtcNow.Month;
            var year = DateTime.UtcNow.Year;

            var tryParseResult = short.TryParse(folderName.Split('-')[0], out var yearFromFolderName);

            if (tryParseResult)
            {
                var previousAcademicYear = 2000 + (yearFromFolderName / 100);
                var currentAcademicYear = 2000 + (yearFromFolderName % 100);
            
                if (previousAcademicYear > 2016 && currentAcademicYear < 2099 && currentAcademicYear - previousAcademicYear == 1)
                {
                    AcademicYear = yearFromFolderName;
                }
                else
                {
                    if (month < 8)
                    {
                        AcademicYear = (short)((year - 2000 - 1) * 100 + (year - 2000));
                    }
                    else
                    {
                        AcademicYear = (short)((year - 2000) * 100 + (year - 2000 + 1));
                    }

                }
            }

            if (month < 8)
            {
                CollectionPeriod = (byte)(month + 5);
            }
            else
            {
                CollectionPeriod = (byte)(month - 7);
            }    
        }

        public short AcademicYear { get; set; }
        public int CollectionPeriod { get; set; }
    }
}