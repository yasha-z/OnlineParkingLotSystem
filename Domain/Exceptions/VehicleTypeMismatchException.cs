namespace OnlineParkingLotSystem.Domain.Exceptions;

public class VehicleTypeMismatchException : DomainException
{
    public VehicleTypeMismatchException(string licensePlate, string expectedType)
        : base($"Vehicle type mismatch for license plate '{licensePlate}'. Expected '{expectedType}'.")
    {
    }
}
