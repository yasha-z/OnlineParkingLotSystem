namespace OnlineParkingLotSystem.Domain.Exceptions;

public class VehicleAlreadyParkedException : DomainException
{
    public VehicleAlreadyParkedException(string licensePlate)
        : base($"Vehicle with license plate '{licensePlate}' is already parked.")
    {
    }
}
