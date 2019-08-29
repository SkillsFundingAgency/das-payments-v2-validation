namespace SFA.DAS.Payments.Contingency.Constants
{
    public static class Sql
    {
        // DAS_PeriodEnd (DEDS)
        public const string Datalocks1819R12 = @"
            SELECT Uln, Ukprn, LearnRefNumber, AimSeqNumber
            FROM PaymentsDue.NonPayableEarnings E

            WHERE CollectionPeriodName = '1819-R12'
            AND DeliveryYear = 2019
            AND DeliveryMonth = 7
            AND (PaymentFailureReason = 0 OR PaymentFailureReason = 2)
            AND ApprenticeshipContractType = 1
            GROUP BY Uln, Ukprn, LearnRefNumber, AimSeqNumber

            ";

        // DS_SILR1819_Collection (DEDS)
        public const string Datalocks1819R13 = @"
            SELECT M.Ukprn, M.LearnRefNumber, Uln, AimSeqNumber
            FROM Datalock.PriceEpisodePeriodMatch M
            JOIN Valid.Learner L
	            ON M.Ukprn = L.UKPRN
	            AND M.LearnRefNumber = L.LearnRefNumber

            WHERE M.CollectionPeriodName = '1819-R13'
            AND Period = 12
            AND Payable = 0
            AND TransactionTypesFlag = 1

            GROUP BY M.Ukprn, M.LearnRefNumber, Uln, AimSeqNumber
            ";

        // DasPayments
        public const string Commitments1920 = @"
            SELECT StandardCode, ProgrammeType, FrameworkCode, PathwayCode, StartDate, Ukprn, Uln, Ape.Cost [Amount]
            FROM Payments2.Apprenticeship A
            JOIN Payments2.ApprenticeshipPriceEpisode APE
            ON A.Id = APE.ApprenticeshipId

            WHERE Ape.Removed = 0
            AND A.Status = 1

            ";

        // ILR1920Data
        public const string Earnings = @"
                WITH 

                RawEarnings AS (
	                SELECT
		                APEP.LearnRefNumber,
		                APEP.Ukprn,
		                APE.PriceEpisodeAimSeqNumber [AimSeqNumber],
		                APEP.PriceEpisodeIdentifier,
		                APE.EpisodeStartDate,
		                APE.EpisodeEffectiveTNPStartDate,
		                APEP.[Period],
		                L.ULN,
		                COALESCE(LD.ProgType, 0) [ProgrammeType],
		                COALESCE(LD.FworkCode, 0) [FrameworkCode],
		                COALESCE(LD.PwayCode, 0) [PathwayCode],
		                COALESCE(LD.StdCode, 0) [StandardCode],
		                COALESCE(APEP.PriceEpisodeSFAContribPct, 0) [SfaContributionPercentage],
		                APE.PriceEpisodeFundLineType [FundingLineType],
		                LD.LearnAimRef,
		                LD.LearnStartDate [LearningStartDate],
		                COALESCE(APEP.PriceEpisodeOnProgPayment, 0) [TransactionType01],
		                COALESCE(APEP.PriceEpisodeCompletionPayment, 0) [TransactionType02],
		                COALESCE(APEP.PriceEpisodeBalancePayment, 0) [TransactionType03],
		                COALESCE(APEP.PriceEpisodeFirstEmp1618Pay, 0) [TransactionType04],
		                COALESCE(APEP.PriceEpisodeFirstProv1618Pay, 0) [TransactionType05],
		                COALESCE(APEP.PriceEpisodeSecondEmp1618Pay, 0) [TransactionType06],
		                COALESCE(APEP.PriceEpisodeSecondProv1618Pay, 0) [TransactionType07],
		                COALESCE(APEP.PriceEpisodeApplic1618FrameworkUpliftOnProgPayment, 0) [TransactionType08],
		                COALESCE(APEP.PriceEpisodeApplic1618FrameworkUpliftCompletionPayment, 0) [TransactionType09],
		                COALESCE(APEP.PriceEpisodeApplic1618FrameworkUpliftBalancing, 0) [TransactionType10],
		                COALESCE(APEP.PriceEpisodeFirstDisadvantagePayment, 0) [TransactionType11],
		                COALESCE(APEP.PriceEpisodeSecondDisadvantagePayment, 0) [TransactionType12],
		                0 [TransactionType13],
		                0 [TransactionType14],
		                COALESCE(APEP.PriceEpisodeLSFCash, 0) [TransactionType15],
		                COALESCE([APEP].[PriceEpisodeLearnerAdditionalPayment], 0) [TransactionType16],
		                CASE WHEN APE.PriceEpisodeContractType = 'Contract for services with the employer' THEN 1 ELSE 2 END [ApprenticeshipContractType],
		                PriceEpisodeTotalTNPPrice [TotalPrice],
		                0 [MathsAndEnglish]
	                FROM Rulebase.AEC_ApprenticeshipPriceEpisode_Period APEP
	                INNER JOIN Rulebase.AEC_ApprenticeshipPriceEpisode APE
		                on APEP.UKPRN = APE.UKPRN
		                and APEP.LearnRefNumber = APE.LearnRefNumber
		                and APEP.PriceEpisodeIdentifier = APE.PriceEpisodeIdentifier
	                JOIN Valid.Learner L
		                on L.UKPRN = APEP.Ukprn
		                and L.LearnRefNumber = APEP.LearnRefNumber
	                JOIN Valid.LearningDelivery LD
		                on LD.UKPRN = APEP.Ukprn
		                and LD.LearnRefNumber = APEP.LearnRefNumber
		                and LD.AimSeqNumber = APE.PriceEpisodeAimSeqNumber
	                where (
		                APEP.PriceEpisodeOnProgPayment != 0
		                or APEP.PriceEpisodeCompletionPayment != 0
		                or APEP.PriceEpisodeBalancePayment != 0
		                or APEP.PriceEpisodeFirstEmp1618Pay != 0
		                or APEP.PriceEpisodeFirstProv1618Pay != 0
		                or APEP.PriceEpisodeSecondEmp1618Pay != 0
		                or APEP.PriceEpisodeSecondProv1618Pay != 0
		                or APEP.PriceEpisodeApplic1618FrameworkUpliftOnProgPayment != 0
		                or APEP.PriceEpisodeApplic1618FrameworkUpliftCompletionPayment != 0
		                or APEP.PriceEpisodeApplic1618FrameworkUpliftBalancing != 0
		                or APEP.PriceEpisodeFirstDisadvantagePayment != 0
		                or APEP.PriceEpisodeSecondDisadvantagePayment != 0
		                or APEP.PriceEpisodeLSFCash != 0
		                )
		                AND APEP.Period = 1
                )

                , RawEarningsMathsAndEnglish AS (
	                select 
		                LDP.LearnRefNumber,
		                LDP.Ukprn,
		                LDP.AimSeqNumber,
		                NULL [PriceEpisodeIdentifier],
		                NULL [EpisodeStartDate],
		                NULL [EpisodeEffectiveTNPStartDate],
		                LDP.[Period],
		                L.ULN,
		                COALESCE(LD.ProgType, 0) [ProgrammeType],
		                COALESCE(LD.FworkCode, 0) [FrameworkCode],
		                COALESCE(LD.PwayCode, 0) [PathwayCode],
		                COALESCE(LD.StdCode, 0) [StandardCode],
		                COALESCE(LDP.[LearnDelSFAContribPct], 0) [SfaContributionPercentage],
		                LDP.FundLineType [FundingLineType],
		                LD.LearnAimRef,
		                LD.LearnStartDate [LearningStartDate],
		                0 [TransactionType01],
		                0 [TransactionType02],
		                0 [TransactionType03],
		                0 [TransactionType04],
		                0 [TransactionType05],
		                0 [TransactionType06],
		                0 [TransactionType07],
		                0 [TransactionType08],
		                0 [TransactionType09],
		                0 [TransactionType10],
		                0 [TransactionType11],
		                0 [TransactionType12],
		                COALESCE(MathEngOnProgPayment, 0) [TransactionType13],
		                COALESCE(MathEngBalPayment, 0) [TransactionType14],
		                COALESCE(LearnSuppFundCash, 0) [TransactionType15],
		                0 [TransactionType16],
		                CASE WHEN LDP.LearnDelContType = 'Contract for services with the employer' THEN 1 ELSE 2 END [ApprenticeshipContractType],
		                0 [TotalPrice],
		                1 [MathsAndEnglish]
	                FROM Rulebase.AEC_LearningDelivery_Period LDP
	                INNER JOIN Valid.LearningDelivery LD
		                on LD.UKPRN = LDP.UKPRN
		                and LD.LearnRefNumber = LDP.LearnRefNumber
		                and LD.AimSeqNumber = LDP.AimSeqNumber
	                JOIN Valid.Learner L
		                on L.UKPRN = LD.Ukprn
		                and L.LearnRefNumber = LD.LearnRefNumber
	                where (
		                MathEngOnProgPayment != 0
		                or MathEngBalPayment != 0
		                or LearnSuppFundCash != 0
		                )
		                and LD.LearnAimRef != 'ZPROG001'
		                AND Period = 1
                )

                , AllAct1Earnings AS (
	                SELECT * 
	                FROM RawEarnings
	                WHERE ApprenticeshipContractType = 1

	                UNION

	                SELECT * 
	                FROM RawEarningsMathsAndEnglish
	                WHERE ApprenticeshipContractType = 1
                )


                SELECT SUM(TransactionType01 + TransactionType02 + TransactionType03 + TransactionType04 + TransactionType05 + TransactionType06 + 
	                TransactionType07 + TransactionType08 + TransactionType09 + TransactionType10 + TransactionType11 + TransactionType12 + TransactionType13 +
	                TransactionType14 + TransactionType15 + TransactionType16) [Amount], LearnRefNumber, Ukprn, EpisodeEffectiveTNPStartDate, 
	                ULN, ProgrammeType, FrameworkCode, PathwayCode, StandardCode, SfaContributionPercentage, FundingLineType, AimSeqNumber, TotalPrice, MathsAndEnglish
                FROM AllAct1Earnings

                GROUP BY LearnRefNumber, Ukprn, EpisodeEffectiveTNPStartDate, 
	                ULN, ProgrammeType, FrameworkCode, PathwayCode, StandardCode, SfaContributionPercentage, FundingLineType, AimSeqNumber, TotalPrice, MathsAndEnglish

            ";
    }
}
