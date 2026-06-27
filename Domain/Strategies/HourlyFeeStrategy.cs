using OnlineParkingLotSystem.Domain.Enums;

namespace OnlineParkingLotSystem.Domain.Strategies;

public class HourlyFeeStrategy : IFeeStrategy
{
    private const decimal RatePerHour = 50m;

    public FeeStrategyType StrategyType => FeeStrategyType.Hourly;

    public decimal CalculateFee(DateTime entryTime, DateTime exitTime)
    {
        var duration = exitTime - entryTime;
        var hours = Math.Max(1, (int)Math.Ceiling(duration.TotalHours));
        return hours * RatePerHour;
    }
}
