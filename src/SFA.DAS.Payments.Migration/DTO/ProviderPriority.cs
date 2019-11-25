namespace SFA.DAS.Payments.Migration.DTO
{
    class ProviderPriority
    {
        public long EmployerAccountId { get; set; }
        public long ProviderId { get; set; }
        public long PriorityOrder { get; set; }
    }
}
