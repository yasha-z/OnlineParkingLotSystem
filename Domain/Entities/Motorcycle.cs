using OnlineParkingLotSystem.Domain.Enums;

namespace OnlineParkingLotSystem.Domain.Entities;

public class Motorcycle : Vehicle
{
    public Motorcycle()
    {
        VehicleType = VehicleType.Motorcycle;
    }

    public override VehicleSize Size => VehicleSize.Small;
}
