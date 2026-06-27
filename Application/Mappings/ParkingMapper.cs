using OnlineParkingLotSystem.Application.DTOs.Responses;
using OnlineParkingLotSystem.Domain.Entities;

namespace OnlineParkingLotSystem.Application.Mappings;

public static class ParkingMapper
{
    public static ParkingTicketResponse ToResponse(ParkingTicket ticket)
    {
        return new ParkingTicketResponse
        {
            TicketId = ticket.Id,
            LicensePlate = ticket.Vehicle.LicensePlate,
            VehicleType = ticket.Vehicle.VehicleType,
            SpotNumber = ticket.ParkingSpot.SpotNumber,
            SpotType = ticket.ParkingSpot.SpotType,
            FloorNumber = ticket.ParkingSpot.Floor.FloorNumber,
            EntryTime = ticket.EntryTime,
            ExitTime = ticket.ExitTime,
            FeePaid = ticket.FeePaid,
            IsActive = ticket.IsActive,
            FeeStrategyType = ticket.FeeStrategyType
        };
    }

    public static ExitParkingResponse ToExitResponse(ParkingTicket ticket)
    {
        return new ExitParkingResponse
        {
            TicketId = ticket.Id,
            LicensePlate = ticket.Vehicle.LicensePlate,
            FeePaid = ticket.FeePaid ?? 0,
            EntryTime = ticket.EntryTime,
            ExitTime = ticket.ExitTime ?? DateTime.UtcNow,
            SpotNumber = ticket.ParkingSpot.SpotNumber
        };
    }
}
