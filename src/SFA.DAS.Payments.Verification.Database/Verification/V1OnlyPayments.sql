﻿CREATE TABLE [Verification].[V1OnlyPayments]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [CommitmentId] BIGINT NOT NULL, 
    [AccountId] BIGINT NOT NULL, 
    [LearnerReferenceNumber] NVARCHAR(50) NOT NULL, 
    [Ukprn] BIGINT NOT NULL, 
    [IlrSubmissionDateTime] DATETIME2 NOT NULL, 
    [PriceEpisodeIdentifier] NVARCHAR(50) NOT NULL, 
    [LearningAimStandardCode] INT NOT NULL, 
    [LearningAimProgrammeType] INT NOT NULL, 
    [LearningAimFrameworkCode] INT NOT NULL, 
    [LearningAimPathwayCode] INT NOT NULL, 
    [ContractType] INT NOT NULL, 
    [CollectionPeriodName] NVARCHAR(50) NOT NULL, 
    [TransactionType] INT NOT NULL, 
    [SfaContributionPercentage] DECIMAL(18, 5) NOT NULL, 
    [LearningAimFundingLineType] NVARCHAR(150) NOT NULL, 
    [LearningAimReference] NVARCHAR(50) NOT NULL, 
    [DeliveryPeriod] INT NOT NULL,
	AcademicYear int NOT NULL,
	FundingSource int NOT NULL,
	Amount money NOT NULL, 
    [LearnerUln] BIGINT NOT NULL
)
