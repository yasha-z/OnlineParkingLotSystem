namespace OnlineParkingLotSystem.Domain.Exceptions;

public class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException()
        : base("Invalid username or password.")
    {
    }
}
