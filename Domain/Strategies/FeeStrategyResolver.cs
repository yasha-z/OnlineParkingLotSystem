using OnlineParkingLotSystem.Domain.Enums;

namespace OnlineParkingLotSystem.Domain.Strategies;
//this resolver class is responsible for resolving the appropriate fee strategy based on the strategytype


//maine program.cs me aik hee interface ki 3 implementations register ki hain
// to ye resolver class un 3 implementations me se aik ko select krega based on the strategy type jo hum pass krenge
public class FeeStrategyResolver
{
    private readonly IEnumerable<IFeeStrategy> _strategies;//this will hold all the fee strategies that are registered in the DI container
//ienumerable is use as it is a collection of objects that can be enumerated, and it is more efficient than using a list or an array when 
// you only need to iterate over the collection
    public FeeStrategyResolver(IEnumerable<IFeeStrategy> strategies)
    {
        _strategies = strategies;
    }

    public IFeeStrategy Resolve(FeeStrategyType strategyType) =>
        _strategies.First(strategy => strategy.StrategyType == strategyType);//strategy type could be hourly, daily, or monthly and it will return the corresponding strategy
}
