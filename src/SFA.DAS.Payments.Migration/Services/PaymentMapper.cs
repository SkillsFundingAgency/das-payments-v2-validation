using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Payments.Migration.DTO;
using SFA.DAS.Payments.ProviderPayments.Model.V1;

namespace SFA.DAS.Payments.Migration.Services
{
    public class PaymentMapper
    {
        private static readonly HashSet<Guid> ProcessedRequiredPayments = new HashSet<Guid>();
        private static readonly HashSet<int> TransactionTypesForEarnings = new HashSet<int>(new [] {1, 2, 3});

        public List<LegacyAccountTransferModel> MapV2AccountTransfers(List<V2PaymentAndEarning> payments)
        {
            var accountTransfers = new List<LegacyAccountTransferModel>();

            foreach (var paymentModel in payments)
            {
                if (paymentModel.TransferSenderAccountId.HasValue && 
                    paymentModel.ApprenticeshipId.HasValue && 
                    paymentModel.AccountId.HasValue &&
                    paymentModel.FundingSource == 5)
                {
                    accountTransfers.Add(new LegacyAccountTransferModel
                    {
                        Amount = paymentModel.Amount,
                        CollectionPeriodMonth = MonthFromPeriod(paymentModel.CollectionPeriod),
                        CollectionPeriodName = $"{paymentModel.AcademicYear}-R{paymentModel.CollectionPeriod:D2}",
                        CollectionPeriodYear = YearFromPeriod(paymentModel.AcademicYear, paymentModel.CollectionPeriod),
                        TransferType = TransferType.Levy,
                        CommitmentId = paymentModel.ApprenticeshipId.Value,
                        ReceivingAccountId = paymentModel.AccountId.Value,
                        RequiredPaymentId = paymentModel.RequiredPaymentEventId,
                        SendingAccountId = paymentModel.TransferSenderAccountId.Value
                    });
                }
            }

            return accountTransfers;
        }

        public (List<LegacyPaymentModel> payments, List<LegacyRequiredPaymentModel> requiredPayments, List<LegacyEarningModel> earnings)
        MapV2Payments(List<V2PaymentAndEarning> payments, HashSet<Guid> dontCreateRequiredPaymentList)
        {
            
            foreach (var guid in dontCreateRequiredPaymentList)
            {
                ProcessedRequiredPayments.Add(guid);
            }

            var legacyPayments = new List<LegacyPaymentModel>();
            var legacyRequiredPayments = new Dictionary<Guid, LegacyRequiredPaymentModel>();
            var legacyEarnings = new List<LegacyEarningModel>();
            
            foreach (var paymentModel in payments)
            {
                var requiredPayment = new LegacyRequiredPaymentModel
                {
                    Id = paymentModel.RequiredPaymentEventId,
                    AccountId = paymentModel.AccountId,
                    AccountVersionId = string.Empty,
                    AimSeqNumber = paymentModel.LearningAimSequenceNumber,
                    AmountDue = paymentModel.AmountDue,
                    ApprenticeshipContractType = paymentModel.ContractType,
                    CollectionPeriodMonth = MonthFromPeriod(paymentModel.CollectionPeriod),
                    CollectionPeriodName =
                        $"{paymentModel.AcademicYear}-R{paymentModel.CollectionPeriod:D2}",
                    CollectionPeriodYear = YearFromPeriod(paymentModel.AcademicYear,
                        paymentModel.CollectionPeriod),
                    CommitmentId = paymentModel.ApprenticeshipId,
                    CommitmentVersionId = string.Empty,
                    UseLevyBalance = false,
                    DeliveryMonth = MonthFromPeriod(paymentModel.DeliveryPeriod),
                    DeliveryYear = YearFromPeriod(paymentModel.AcademicYear,
                        paymentModel.DeliveryPeriod),
                    FrameworkCode = paymentModel.LearningAimFrameworkCode,
                    FundingLineType = paymentModel.LearningAimFundingLineType,
                    IlrSubmissionDateTime = paymentModel.IlrSubmissionDateTime,
                    LearnAimRef = paymentModel.LearningAimReference,
                    LearnRefNumber = paymentModel.LearnerReferenceNumber,
                    LearningStartDate = paymentModel.StartDate,
                    PathwayCode = paymentModel.LearningAimPathwayCode,
                    PriceEpisodeIdentifier = paymentModel.PriceEpisodeIdentifier,
                    ProgrammeType = paymentModel.LearningAimProgrammeType,
                    SfaContributionPercentage = paymentModel.SfaContributionPercentage,
                    StandardCode = paymentModel.LearningAimStandardCode,
                    TransactionType = paymentModel.TransactionType,
                    Ukprn = paymentModel.Ukprn,
                    Uln = paymentModel.LearnerUln,
                };

                if (!ProcessedRequiredPayments.Contains(requiredPayment.Id))
                {
                    legacyRequiredPayments.Add(requiredPayment.Id, requiredPayment);
                    ProcessedRequiredPayments.Add(requiredPayment.Id);

                    if (TransactionTypesForEarnings.Contains(requiredPayment.TransactionType.Value))
                    {
                        var earning = new LegacyEarningModel
                        {
                            StartDate = paymentModel.EarningsStartDate,
                            RequiredPaymentId = paymentModel.RequiredPaymentEventId,
                            ActualEnddate = paymentModel.EarningsActualEndDate,
                            CompletionAmount = paymentModel.EarningsCompletionAmount,
                            PlannedEndDate = paymentModel.EarningsPlannedEndDate ?? DateTime.MinValue,
                            CompletionStatus = paymentModel.EarningsCompletionStatus,
                            MonthlyInstallment = paymentModel.EarningsInstalmentAmount ?? 0m,
                            TotalInstallments = paymentModel.EarningsNumberOfInstalments ?? 0,
                        };
                        legacyEarnings.Add(earning);
                    }
                }

                var payment = new LegacyPaymentModel
                {
                    RequiredPaymentId = requiredPayment.Id,
                    CollectionPeriodMonth = requiredPayment.CollectionPeriodMonth,
                    CollectionPeriodYear = requiredPayment.CollectionPeriodYear,
                    TransactionType = requiredPayment.TransactionType ?? 0,
                    DeliveryYear = requiredPayment.DeliveryYear ?? 0,
                    CollectionPeriodName = requiredPayment.CollectionPeriodName,
                    DeliveryMonth = requiredPayment.DeliveryMonth ?? 0,
                    Amount = paymentModel.Amount,
                    FundingSource = paymentModel.FundingSource,
                    PaymentId = Guid.NewGuid(),
                };
                legacyPayments.Add(payment);
            }

            return (legacyPayments, legacyRequiredPayments.Values.ToList(), legacyEarnings);
        }

        public static int YearFromPeriod(short academicYear, byte collectionPeriod)
        {
            var ilrStartYear = (academicYear / 100) + 2000;

            if (collectionPeriod > 5)
            {
                return ilrStartYear + 1;
            }

            return ilrStartYear;
        }

        public static int MonthFromPeriod(byte collectionPeriod)
        {
            if (collectionPeriod <= 5)
            {
                return collectionPeriod + 7;
            }

            return collectionPeriod - 5;
        }
    }
}
