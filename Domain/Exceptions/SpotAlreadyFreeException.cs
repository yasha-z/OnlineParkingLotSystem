namespace OnlineParkingLotSystem.Domain.Exceptions;

public class SpotAlreadyFreeException : DomainException
{
    public SpotAlreadyFreeException(string spotNumber)
        : base($"Parking spot '{spotNumber}' is already free.")
    {
    }
}
