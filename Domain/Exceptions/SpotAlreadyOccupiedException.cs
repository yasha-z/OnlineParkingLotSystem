namespace OnlineParkingLotSystem.Domain.Exceptions;

public class SpotAlreadyOccupiedException : DomainException
{
    public SpotAlreadyOccupiedException(string spotNumber)
        : base($"Parking spot '{spotNumber}' is already occupied.")
    {
    }
}
