namespace OnlineParkingLotSystem.Domain.Exceptions;

public class TicketNotFoundException : DomainException
{
    public TicketNotFoundException(int ticketId)
        : base($"Parking ticket with id '{ticketId}' was not found.")
    {
    }
}
