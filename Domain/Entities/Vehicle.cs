using OnlineParkingLotSystem.Domain.Enums;

namespace OnlineParkingLotSystem.Domain.Entities;

public abstract class Vehicle
{
    public int Id { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public VehicleType VehicleType { get; protected set; }
    public abstract VehicleSize Size { get; }
    public ICollection<ParkingTicket> ParkingTickets { get; set; } = new List<ParkingTicket>();
}
