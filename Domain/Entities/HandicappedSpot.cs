using OnlineParkingLotSystem.Domain.Enums;

namespace OnlineParkingLotSystem.Domain.Entities;

public class HandicappedSpot : ParkingSpot
{
    public HandicappedSpot()
    {
        SpotType = SpotType.Handicapped;
    }

    public override bool CanFit(Vehicle vehicle) =>
        vehicle.Size is VehicleSize.Small or VehicleSize.Medium;
}
