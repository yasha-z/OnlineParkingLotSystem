using OnlineParkingLotSystem.Domain.Enums;

namespace OnlineParkingLotSystem.Domain.Strategies;

public interface IFeeStrategy
{
    FeeStrategyType StrategyType { get; }
    decimal CalculateFee(DateTime entryTime, DateTime exitTime);
}
