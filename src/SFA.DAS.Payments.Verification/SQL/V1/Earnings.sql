
WITH RawEarnings AS (
	SELECT
		[APEP].[LearnRefNumber] [LearnerReferenceNumber]
		,[APEP].[Ukprn]
		,[APEP].[PriceEpisodeIdentifier]
		,[APE].[EpisodeStartDate]
		,[APE].[EpisodeEffectiveTNPStartDate]
		,CASE WHEN [APEP].[Period] < 8 THEN [APEP].[Period] + 5 ELSE [APEP].[Period] - 7 END [DeliveryPeriod]   
		,[L].[ULN] [LearnerUln]
		,COALESCE([LD].[ProgType], 0) [LearningAimProgrammeType]
		,COALESCE([LD].[FworkCode], 0) [LearningAimFrameworkCode]
		,COALESCE([LD].[PwayCode], 0) [LearningAimPathwayCode]
		,COALESCE([LD].[StdCode], 0) [LearningAimStandardCode]
		,COALESCE([APEP].[PriceEpisodeSFAContribPct], 0) [SfaContributionPercentage]
		,[APE].[PriceEpisodeFundLineType] [LearningAimFundingLineType]
		,[LD].[LearnAimRef] [LearningAimReference]
		,[LD].[LearnStartDate] [LearningStartDate]
		,COALESCE([APEP].[PriceEpisodeOnProgPayment], 0) [TransactionType01]
		,COALESCE([APEP].[PriceEpisodeCompletionPayment], 0) [TransactionType02]
		,COALESCE([APEP].[PriceEpisodeBalancePayment], 0) [TransactionType03]
		,COALESCE([APEP].[PriceEpisodeFirstEmp1618Pay], 0) [TransactionType04]
		,COALESCE([APEP].[PriceEpisodeFirstProv1618Pay], 0) [TransactionType05]
		,COALESCE([APEP].[PriceEpisodeSecondEmp1618Pay], 0) [TransactionType06]
		,COALESCE([APEP].[PriceEpisodeSecondProv1618Pay], 0) [TransactionType07]
		,COALESCE([APEP].[PriceEpisodeApplic1618FrameworkUpliftOnProgPayment], 0) [TransactionType08]
		,COALESCE([APEP].[PriceEpisodeApplic1618FrameworkUpliftCompletionPayment], 0) [TransactionType09]
		,COALESCE([APEP].[PriceEpisodeApplic1618FrameworkUpliftBalancing], 0) [TransactionType10]
		,COALESCE([APEP].[PriceEpisodeFirstDisadvantagePayment], 0) [TransactionType11]
		,COALESCE([APEP].[PriceEpisodeSecondDisadvantagePayment], 0) [TransactionType12]
		,0 [TransactionType13]
		,0 [TransactionType14]
		,COALESCE([APEP].[PriceEpisodeLSFCash], 0) [TransactionType15]
		,COALESCE([APEP].[PriceEpisodeLearnerAdditionalPayment], 0) [TransactionType16]
		,CASE WHEN [APE].[PriceEpisodeContractType] = 'Levy Contract' THEN 1 ELSE 2 END [ContractType]
		,[APE].[PriceEpisodeFirstAdditionalPaymentThresholdDate] [FirstIncentiveCensusDate]
		,[APE].[PriceEpisodeSecondAdditionalPaymentThresholdDate] [SecondIncentiveCensusDate]
		,[APE].[PriceEpisodeLearnerAdditionalPaymentThresholdDate] [LearnerAdditionalPaymentsDate]
		,[APE].[PriceEpisodeTotalTnpPrice] [AgreedPrice]
		,[APE].[PriceEpisodeActualEndDate] [EndDate]
		,[APE].[PriceEpisodeCumulativePMRs] [CumulativePmrs]
		,[APE].[PriceEpisodeCompExemCode] [ExemptionCodeForCompletionHoldback]
	FROM [DS_SILR1819_Collection].[Rulebase].[AEC_ApprenticeshipPriceEpisode_Period] [APEP]
	INNER JOIN [DS_SILR1819_Collection].[Rulebase].[AEC_ApprenticeshipPriceEpisode] [APE]
		ON [APEP].[UKPRN] = [APE].[UKPRN]
		AND [APEP].[LearnRefNumber] = [APE].[LearnRefNumber]
		AND [APEP].[PriceEpisodeIdentifier] = [APE].[PriceEpisodeIdentifier]
	JOIN [DS_SILR1819_Collection].[Valid].[Learner] L
		ON [L].[UKPRN] = [APEP].[Ukprn]
		AND [L].[LearnRefNumber] = [APEP].[LearnRefNumber]
	JOIN [DS_SILR1819_Collection].[Valid].[LearningDelivery] LD
		ON [LD].[UKPRN] = [APEP].[Ukprn]
		AND [LD].[LearnRefNumber] = [APEP].[LearnRefNumber]
		AND [LD].[AimSeqNumber] = [APE].[PriceEpisodeAimSeqNumber]
	WHERE (
		[APEP].[PriceEpisodeOnProgPayment] != 0
		OR [APEP].[PriceEpisodeCompletionPayment] != 0
		OR [APEP].[PriceEpisodeBalancePayment] != 0
		OR [APEP].[PriceEpisodeFirstEmp1618Pay] != 0
		OR [APEP].[PriceEpisodeFirstProv1618Pay] != 0
		OR [APEP].[PriceEpisodeSecondEmp1618Pay] != 0
		OR [APEP].[PriceEpisodeSecondProv1618Pay] != 0
		OR [APEP].[PriceEpisodeApplic1618FrameworkUpliftOnProgPayment] != 0
		OR [APEP].[PriceEpisodeApplic1618FrameworkUpliftCompletionPayment] != 0
		OR [APEP].[PriceEpisodeApplic1618FrameworkUpliftBalancing] != 0
		OR [APEP].[PriceEpisodeFirstDisadvantagePayment] != 0
		OR [APEP].[PriceEpisodeSecondDisadvantagePayment] != 0
		OR [APEP].[PriceEpisodeLSFCash] != 0
		OR [APEP].[PriceEpisodeLearnerAdditionalPayment] != 0
		OR [APEP].[Period] = 1
		)
		AND [L].[Uln] IN (SELECT ULN FROM ##Learners)
	
	UNION

	select
		[LDP].[LearnRefNumber] [LearnerReferenceNumber]
		,[LDP].[Ukprn]
		,NULL [PriceEpisodeIdentifier]
		,NULL [EpisodeStartDate]
		,NULL [EpisodeEffectiveTNPStartDate]
		,CASE WHEN [LDP].[Period] < 8 THEN [LDP].[Period] + 5 ELSE [LDP].[Period] - 7 END [DeliveryPeriod]  
		,[L].[ULN] [LearnerUln]
		,COALESCE([LD].[ProgType], 0) [LearningAimProgrammeType] 
		,COALESCE([LD].[FworkCode], 0) [LearningAimFrameworkCode]
		,COALESCE([LD].[PwayCode], 0) [LearningAimPathwayCode]
		,COALESCE([LD].[StdCode], 0) [LearningAimStandardCode]
		,COALESCE([LDP].[LearnDelSFAContribPct], 0) [SfaContributionPercentage]
		,[LDP].[FundLineType] [LearningAimFundingLineType]
		,[LD].[LearnAimRef] [LearningAimReference]
		,[LD].[LearnStartDate] [LearningStartDate]
		,0 [TransactionType01]
		,0 [TransactionType02]
		,0 [TransactionType03]
		,0 [TransactionType04]
		,0 [TransactionType05]
		,0 [TransactionType06]
		,0 [TransactionType07]
		,0 [TransactionType08]
		,0 [TransactionType09]
		,0 [TransactionType10]
		,0 [TransactionType11]
		,0 [TransactionType12]
		,COALESCE([MathEngOnProgPayment], 0) [TransactionType13]
		,COALESCE([MathEngBalPayment], 0) [TransactionType14]
		,COALESCE([LearnSuppFundCash], 0) [TransactionType15]
		,0 [TransactionType16]
		,CASE WHEN [LDP].[LearnDelContType] = 'Levy Contract' THEN 1 ELSE 2 END [ContractType]
		,NULL [FirstIncentiveCensusDate]
		,NULL [SecondIncentiveCensusDate]
		,NULL [LearnerAdditionalPaymentsDate]
		,NULL [AgreedPrice]
		,NULL [EndDate]
		,NULL [CumulativePmrs]
		,NULL [ExemptionCodeForCompletionHoldback]
	FROM [DS_SILR1819_Collection].[Rulebase].[AEC_LearningDelivery_Period] LDP
	INNER JOIN [DS_SILR1819_Collection].[Valid].[LearningDelivery] LD
		ON [LD].[UKPRN] = [LDP].[UKPRN]
		AND [LD].[LearnRefNumber] = [LDP].[LearnRefNumber]
		AND [LD].[AimSeqNumber] = [LDP].[AimSeqNumber]
	JOIN [DS_SILR1819_Collection].[Valid].[Learner] L
		ON [L].[UKPRN] = [LD].[Ukprn]
		AND [L].[LearnRefNumber] = [LD].[LearnRefNumber]
	WHERE (
		MathEngOnProgPayment != 0
		OR MathEngBalPayment != 0
		OR LearnSuppFundCash != 0
		)
		AND LD.LearnAimRef != 'ZPROG001'
		AND [L].[ULN] IN (SELECT ULN FROM ##Learners)
)

SELECT * FROM RawEarnings

WHERE DeliveryPeriod IN @periods

Order by UKPRN, learneruln, DeliveryPeriod