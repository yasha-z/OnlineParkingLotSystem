using OnlineParkingLotSystem.Domain.Enums;

namespace OnlineParkingLotSystem.Domain.Strategies;

public class FlatFeeStrategy : IFeeStrategy
{
    private const decimal FlatRate = 200m;

    public FeeStrategyType StrategyType => FeeStrategyType.Flat;

    public decimal CalculateFee(DateTime entryTime, DateTime exitTime) => FlatRate;
}
