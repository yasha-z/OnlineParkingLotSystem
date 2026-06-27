using OnlineParkingLotSystem.Domain.Enums;

namespace OnlineParkingLotSystem.Application.DTOs.Responses;

public class ParkingTicketResponse
{
    public int TicketId { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public VehicleType VehicleType { get; set; }
    public string SpotNumber { get; set; } = string.Empty;
    public SpotType SpotType { get; set; }
    public int FloorNumber { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public decimal? FeePaid { get; set; }
    public bool IsActive { get; set; }
    public FeeStrategyType FeeStrategyType { get; set; }
}
