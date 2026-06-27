namespace OnlineParkingLotSystem.Domain.Exceptions;

public class NoAvailableSpotException : DomainException
{
    public NoAvailableSpotException(string vehicleType)
        : base($"No available parking spot found for vehicle type '{vehicleType}'.")
    {
    }
}
