using System.Threading.Tasks;

namespace SFA.DAS.Payments.Contingency.CongingencyStrategies
{
    public interface IProduceContingencyPayments
    {
        Task GenerateContingencyPayments();
    }
}
