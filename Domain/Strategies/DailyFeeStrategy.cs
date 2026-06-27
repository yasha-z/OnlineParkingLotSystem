using OnlineParkingLotSystem.Domain.Enums;

namespace OnlineParkingLotSystem.Domain.Strategies;

public class DailyFeeStrategy : IFeeStrategy
{
    private const decimal RatePerDay = 500m;

    public FeeStrategyType StrategyType => FeeStrategyType.Daily;

    public decimal CalculateFee(DateTime entryTime, DateTime exitTime)
    {
        var duration = exitTime - entryTime;
        var days = Math.Max(1, (int)Math.Ceiling(duration.TotalDays));
        return days * RatePerDay;
    }
}
