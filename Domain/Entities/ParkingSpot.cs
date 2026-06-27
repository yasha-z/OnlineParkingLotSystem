using OnlineParkingLotSystem.Domain.Enums;
using OnlineParkingLotSystem.Domain.Exceptions;

namespace OnlineParkingLotSystem.Domain.Entities;

public abstract class ParkingSpot
{
    public int Id { get; set; }
    public string SpotNumber { get; set; } = string.Empty;
    public SpotType SpotType { get; protected set; }
    public int FloorId { get; set; }
    public Floor Floor { get; set; } = null!;

    private bool _isOccupied;
    public bool IsOccupied => _isOccupied;

    public abstract bool CanFit(Vehicle vehicle);

    public void Occupy()
    {
        if (_isOccupied)
        {
            throw new SpotAlreadyOccupiedException(SpotNumber);
        }

        _isOccupied = true;
    }

    public void Release()
    {
        if (!_isOccupied)
        {
            throw new SpotAlreadyFreeException(SpotNumber);
        }

        _isOccupied = false;
    }
}
