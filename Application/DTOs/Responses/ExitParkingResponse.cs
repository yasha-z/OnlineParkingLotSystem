namespace OnlineParkingLotSystem.Application.DTOs.Responses;

public class ExitParkingResponse
{
    public int TicketId { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public decimal FeePaid { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime ExitTime { get; set; }
    public string SpotNumber { get; set; } = string.Empty;
}
