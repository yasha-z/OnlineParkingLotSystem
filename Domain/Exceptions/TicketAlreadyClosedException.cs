namespace OnlineParkingLotSystem.Domain.Exceptions;

public class TicketAlreadyClosedException : DomainException
{
    public TicketAlreadyClosedException(int ticketId)
        : base($"Parking ticket with id '{ticketId}' is already closed.")
    {
    }
}
