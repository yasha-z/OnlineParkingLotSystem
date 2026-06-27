using OnlineParkingLotSystem.Domain.Enums;

namespace OnlineParkingLotSystem.Domain.Entities;

public class Car : Vehicle
{
    public Car()
    {
        VehicleType = VehicleType.Car;
    }

    public override VehicleSize Size => VehicleSize.Medium;
}
