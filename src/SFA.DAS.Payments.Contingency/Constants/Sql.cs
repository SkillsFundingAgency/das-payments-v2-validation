namespace SFA.DAS.Payments.Contingency.Constants
{
    public static class Sql
    {
        // DS_SILR1819_Collection (DEDS)
        public const string Datalocks1819 = @"
            WITH R13Datalocks AS (
	            SELECT M.Ukprn, M.LearnRefNumber, Uln, AimSeqNumber, Payable
                FROM [DS_SILR1819_Collection].Datalock.PriceEpisodePeriodMatch M
                JOIN [DS_SILR1819_Collection].Valid.Learner L
	                ON M.Ukprn = L.UKPRN
	                AND M.LearnRefNumber = L.LearnRefNumber

                WHERE M.CollectionPeriodName = '1819-R14'
                AND Period = 12
                AND TransactionTypesFlag = 1

                GROUP BY M.Ukprn, M.LearnRefNumber, Uln, AimSeqNumber, Payable
            )



            SELECT Uln, Ukprn, LearnRefNumber, AimSeqNumber
            FROM [DAS_PeriodEnd].PaymentsDue.NonPayableEarnings E

            WHERE CollectionPeriodName = '1819-R12'
            AND DeliveryYear = 2019
            AND DeliveryMonth = 7
            AND (PaymentFailureReason = 0 OR PaymentFailureReason = 2)
            AND ApprenticeshipContractType = 1

            AND NOT EXISTS (
	            SELECT *
	            FROM R13Datalocks R13
	            WHERE R13.Ukprn = E.Ukprn
	            AND R13.ULN = E.Uln
	            AND R13.LearnRefNumber = E.LearnRefNumber
	            AND R13.AimSeqNumber = E.AimSeqNumber
	            AND R13.Payable = 1
            )

            GROUP BY Uln, Ukprn, LearnRefNumber, AimSeqNumber


            UNION


            SELECT Uln, Ukprn, LearnRefNumber, AimSeqNumber
            FROM R13Datalocks
            WHERE Payable = 0
            ";

        // Das_CommitmentsReferenceData
        public const string Commitments1920 = @"
            SELECT StandardCode, ProgrammeType, FrameworkCode, PathwayCode, 
                    EffectiveFromDate [StartDate], Ukprn, Uln, AgreedCost [Amount]
                FROM DasCommitments C1
                WHERE PaymentStatus = 1
            ";

        // ILR1920Data
        public const string YtdEarnings = @"

WITH RawEarnings AS (
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
      	CASE WHEN [APE].[PriceEpisodeContractType] = 'Levy Contract' THEN 1 
            WHEN [APE].[PriceEpisodeContractType] = 'Contract for services with the employer' THEN 1 
            WHEN [APE].[PriceEpisodeContractType] = 'None' THEN 0 
            WHEN [APE].[PriceEpisodeContractType] = 'Non-Levy Contract' THEN 2 
            WHEN [APE].[PriceEpisodeContractType] = 'Contract for services with the ESFA' THEN 2 
            ELSE -1 END [ApprenticeshipContractType],
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
    WHERE (
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
        or APEP.PriceEpisodeLearnerAdditionalPayment != 0
        )
        AND APEP.Period <= @collectionperiod
)
, RawEarningsMathsAndEnglish AS (
    SELECT 
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
			CASE WHEN LDP.LearnDelContType = 'Levy Contract' THEN 1 
                WHEN LDP.LearnDelContType = 'Contract for services with the employer' THEN 1 
                WHEN LDP.LearnDelContType = 'None' THEN 0 
                WHEN LDP.LearnDelContType = 'Non-Levy Contract' THEN 2 
                WHEN LDP.LearnDelContType = 'Contract for services with the ESFA' THEN 2 
                ELSE -1 END [ApprenticeshipContractType],
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
    WHERE (
        MathEngOnProgPayment != 0
        or MathEngBalPayment != 0
        or LearnSuppFundCash != 0
        )
        AND LD.LearnAimRef != 'ZPROG001'
        AND Period <= @collectionperiod
)

, AllAct1Earnings AS (
	SELECT * 
	FROM RawEarnings
	--WHERE ApprenticeshipContractType = 1

	UNION

	SELECT * 
	FROM RawEarningsMathsAndEnglish
	--WHERE ApprenticeshipContractType = 1
)

SELECT *
FROM AllAct1Earnings
            ";

        public const string BasicV2Apprenticeships = @"
SELECT Uln, Ukprn
FROM Payments2.Apprenticeship
";

        public const string R13Payments = @"
                SELECT Ukprn, LearnerUln [Uln],
                    ContractType, 
                    LearningAimFundingLineType [FundingLineType],
                    Amount,
                    TransactionType
                FROM Payments2.Payment
                WHERE AcademicYear = 1819
                AND CollectionPeriod = 13
            ";

        public const string YtdV2Payments = @"

WITH YtdPayments AS (
	SELECT 
		CASE WHEN TransactionType < 4 THEN Amount ELSE 0 END [OnProg],
		CASE WHEN TransactionType > 3 THEN Amount ELSE 0 END [Incentives],
		Ukprn, LearnerUln [Uln],
		LearningAimFundingLineType [FundingLineType],
		ContractType
	FROM Payments2.Payment
	WHERE AcademicYear = 1920
)

SELECT SUM(OnProg) [OnProgPayments], SUM(Incentives) [IncentivePayments],
	Ukprn, Uln, FundingLineType, ContractType
FROM YtdPayments
GROUP BY Ukprn, Uln, FundingLineType, ContractType
ORDER BY Ukprn, Uln


";
    }
}

