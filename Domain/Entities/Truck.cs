using OnlineParkingLotSystem.Domain.Enums;

namespace OnlineParkingLotSystem.Domain.Entities;

public class Truck : Vehicle
{
    public Truck()
    {
        VehicleType = VehicleType.Truck;
    }

    public override VehicleSize Size => VehicleSize.Large;
}
