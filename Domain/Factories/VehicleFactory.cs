using OnlineParkingLotSystem.Domain.Entities;
using OnlineParkingLotSystem.Domain.Enums;

namespace OnlineParkingLotSystem.Domain.Factories;

public static class VehicleFactory
{//this class creates a vehicle object based on the vehicle type and license plate
    public static Vehicle Create(string licensePlate, VehicleType vehicleType)
    {
        Vehicle vehicle = vehicleType switch
        {
            VehicleType.Motorcycle => new Motorcycle(),
            VehicleType.Car => new Car(),
            VehicleType.Truck => new Truck(),
            _ => throw new ArgumentOutOfRangeException(nameof(vehicleType), vehicleType, "Unsupported vehicle type.")
        };

        vehicle.LicensePlate = licensePlate.Trim().ToUpperInvariant();
        return vehicle;
    }
}
