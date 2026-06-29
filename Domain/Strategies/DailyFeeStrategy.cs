using OnlineParkingLotSystem.Domain.Enums;

namespace OnlineParkingLotSystem.Domain.Strategies;

public class DailyFeeStrategy : IFeeStrategy//implementing the ifeestrategy interface
{
    //interface + polymorphism
    private const decimal RatePerDay = 500m;// decimal type

    public FeeStrategyType StrategyType => FeeStrategyType.Daily;

    public decimal CalculateFee(DateTime entryTime, DateTime exitTime)
    {
        var duration = exitTime - entryTime;
        var days = Math.Max(1, (int)Math.Ceiling(duration.TotalDays));//if the duration is less than 1 day charge for 1 day
        //calculate the num of days then round up to the nearest whole no, at least 1 day is charged
        return days * RatePerDay;
    }
}
