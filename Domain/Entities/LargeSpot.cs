using OnlineParkingLotSystem.Domain.Enums;

namespace OnlineParkingLotSystem.Domain.Entities;

public class LargeSpot : ParkingSpot
{
    public LargeSpot()
    {
        SpotType = SpotType.Large;
    }

    public override bool CanFit(Vehicle vehicle) =>
        vehicle.Size is VehicleSize.Small or VehicleSize.Medium or VehicleSize.Large;
}
