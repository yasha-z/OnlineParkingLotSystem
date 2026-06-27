namespace OnlineParkingLotSystem.Domain.Entities;

public class ParkingLot
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public ICollection<Floor> Floors { get; set; } = new List<Floor>();
}
