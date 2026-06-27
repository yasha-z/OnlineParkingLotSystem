using Microsoft.EntityFrameworkCore;
using OnlineParkingLotSystem.Application.DTOs.Requests;
using OnlineParkingLotSystem.Application.DTOs.Responses;
using OnlineParkingLotSystem.Application.Mappings;
using OnlineParkingLotSystem.Domain.Entities;
using OnlineParkingLotSystem.Domain.Enums;
using OnlineParkingLotSystem.Domain.Exceptions;
using OnlineParkingLotSystem.Domain.Factories;
using OnlineParkingLotSystem.Domain.Strategies;
using OnlineParkingLotSystem.Infrastructure.Data;

namespace OnlineParkingLotSystem.Application.Services;

public class ParkingService : IParkingService
{
    private readonly AppDbContext _context;
    private readonly FeeStrategyResolver _feeStrategyResolver;

    public ParkingService(AppDbContext context, FeeStrategyResolver feeStrategyResolver)
    {
        _context = context;
        _feeStrategyResolver = feeStrategyResolver;
    }

    public async Task<ParkingTicketResponse> ParkVehicleAsync(
        ParkVehicleRequest request,
        CancellationToken cancellationToken = default)
    {
        var licensePlate = request.LicensePlate.Trim().ToUpperInvariant();

        var vehicle = await _context.Vehicles
            .Include(existingVehicle => existingVehicle.ParkingTickets)
            .FirstOrDefaultAsync(existingVehicle => existingVehicle.LicensePlate == licensePlate, cancellationToken);

        if (vehicle is null)
        {
            vehicle = VehicleFactory.Create(licensePlate, request.VehicleType);
            _context.Vehicles.Add(vehicle);
        }
        else if (vehicle.VehicleType != request.VehicleType)
        {
            throw new VehicleTypeMismatchException(licensePlate, vehicle.VehicleType.ToString());
        }

        if (vehicle.ParkingTickets.Any(ticket => ticket.IsActive))
        {
            throw new VehicleAlreadyParkedException(licensePlate);
        }

        var unoccupiedSpots = await _context.ParkingSpots
            .Include(spot => spot.Floor)
            .Where(spot => !spot.IsOccupied)
            .ToListAsync(cancellationToken);

        var availableSpot = unoccupiedSpots.FirstOrDefault(spot => spot.CanFit(vehicle));

        if (availableSpot is null)
        {
            throw new NoAvailableSpotException(request.VehicleType.ToString());
        }

        availableSpot.Occupy();

        var ticket = new ParkingTicket
        {
            Vehicle = vehicle,
            ParkingSpot = availableSpot,
            EntryTime = DateTime.UtcNow,
            IsActive = true,
            FeeStrategyType = request.FeeStrategyType ?? FeeStrategyType.Hourly
        };

        _context.ParkingTickets.Add(ticket);
        await _context.SaveChangesAsync(cancellationToken);

        ticket = await LoadTicketAsync(ticket.Id, cancellationToken);
        return ParkingMapper.ToResponse(ticket);
    }

    public async Task<ExitParkingResponse> ExitVehicleAsync(
        int ticketId,
        CancellationToken cancellationToken = default)
    {
        var ticket = await LoadTicketAsync(ticketId, cancellationToken);

        if (!ticket.IsActive)
        {
            throw new TicketAlreadyClosedException(ticketId);
        }

        var exitTime = DateTime.UtcNow;
        var feeStrategy = _feeStrategyResolver.Resolve(ticket.FeeStrategyType);
        var fee = feeStrategy.CalculateFee(ticket.EntryTime, exitTime);

        ticket.ExitTime = exitTime;
        ticket.FeePaid = fee;
        ticket.IsActive = false;
        ticket.ParkingSpot.Release();

        await _context.SaveChangesAsync(cancellationToken);

        return ParkingMapper.ToExitResponse(ticket);
    }

    public async Task<IReadOnlyList<ParkingTicketResponse>> GetActiveTicketsAsync(
        CancellationToken cancellationToken = default)
    {
        var tickets = await _context.ParkingTickets
            .AsNoTracking()
            .Where(ticket => ticket.IsActive)
            .Include(ticket => ticket.Vehicle)
            .Include(ticket => ticket.ParkingSpot)
            .ThenInclude(spot => spot.Floor)
            .OrderBy(ticket => ticket.EntryTime)
            .ToListAsync(cancellationToken);

        return tickets.Select(ParkingMapper.ToResponse).ToList();
    }

    public async Task<ParkingTicketResponse> GetTicketByIdAsync(
        int ticketId,
        CancellationToken cancellationToken = default)
    {
        var ticket = await LoadTicketAsync(ticketId, cancellationToken);
        return ParkingMapper.ToResponse(ticket);
    }

    private async Task<ParkingTicket> LoadTicketAsync(int ticketId, CancellationToken cancellationToken)
    {
        return await _context.ParkingTickets
            .Include(ticket => ticket.Vehicle)
            .Include(ticket => ticket.ParkingSpot)
            .ThenInclude(spot => spot.Floor)
            .FirstOrDefaultAsync(ticket => ticket.Id == ticketId, cancellationToken)
            ?? throw new TicketNotFoundException(ticketId);
    }
}
