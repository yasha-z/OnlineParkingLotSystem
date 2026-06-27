using OnlineParkingLotSystem.Domain.Enums;

namespace OnlineParkingLotSystem.Domain.Strategies;

public class FeeStrategyResolver
{
    private readonly IEnumerable<IFeeStrategy> _strategies;

    public FeeStrategyResolver(IEnumerable<IFeeStrategy> strategies)
    {
        _strategies = strategies;
    }

    public IFeeStrategy Resolve(FeeStrategyType strategyType) =>
        _strategies.First(strategy => strategy.StrategyType == strategyType);
}
