using OnlineParkingLotSystem.Domain.Enums;

namespace OnlineParkingLotSystem.Domain.Entities;

public class CompactSpot : ParkingSpot
{
    public CompactSpot()
    {
        SpotType = SpotType.Compact;
    }

    public override bool CanFit(Vehicle vehicle) =>
        vehicle.Size is VehicleSize.Small or VehicleSize.Medium;
}
