﻿using System;

namespace SFA.DAS.Payments.Migration.DTO
{
    public class LegacyEarningModel
    {
        public Guid RequiredPaymentId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ActualEnddate { get; set; }
        public int? CompletionStatus { get; set; }
        public decimal? CompletionAmount { get; set; }
        public decimal MonthlyInstallment { get; set; }
        public int TotalInstallments { get; set; }
        public string EndpointAssessorId { get; set; }
    }
}
