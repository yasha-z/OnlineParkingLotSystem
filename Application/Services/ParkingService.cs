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
    private readonly AppDbContext _context; //connection to the db
    private readonly FeeStrategyResolver _feeStrategyResolver; //to get the right fee strategy

    public ParkingService(AppDbContext context, FeeStrategyResolver feeStrategyResolver)
    {
        _context = context;
        _feeStrategyResolver = feeStrategyResolver;
    }

    public async Task<ParkingTicketResponse> ParkVehicleAsync(//this will be called when a vehicle is parked
    //and it will return the parking ticket response
        ParkVehicleRequest request,
        CancellationToken cancellationToken = default)
    {
        var licensePlate = request.LicensePlate.Trim().ToUpperInvariant();//remove extra spaces and make it uppercase

        var vehicle = await _context.Vehicles
            .Include(existingVehicle => existingVehicle.ParkingTickets)//join with parking tickets to check if the vehicle is already parked
            .FirstOrDefaultAsync(existingVehicle => existingVehicle.LicensePlate == licensePlate, cancellationToken);

        if (vehicle is null)
        {
            vehicle = VehicleFactory.Create(licensePlate, request.VehicleType);//yahan par vehicle factory create kregi jo bhi vehicle type hoga uske hisaab se
            _context.Vehicles.Add(vehicle);//add new vehicle to the database
        }
        else if (vehicle.VehicleType != request.VehicleType)
        {//if the vehicle type is different from the one in the database throw an exception
            throw new VehicleTypeMismatchException(licensePlate, vehicle.VehicleType.ToString());
        }

        if (vehicle.ParkingTickets.Any(ticket => ticket.IsActive))
        {//if the vehicle is already parked throw an exception
            throw new VehicleAlreadyParkedException(licensePlate);
        }

        var unoccupiedSpots = await _context.ParkingSpots //this will get the free parking spots
            .Include(spot => spot.Floor)//konse floor par hai usko bhi include krna hoga
            .Where(spot => !spot.IsOccupied)
            .ToListAsync(cancellationToken);

        var availableSpot = unoccupiedSpots.FirstOrDefault(spot => spot.CanFit(vehicle));//use of polymorphism
        //ussi ka canFit method chalega jo vehicle type hoga
        //spot that can fit the vehicle if no spot is available it will be null
        //matlab uss particular vehicle ke liye koi spot hai ya nahin

        if (availableSpot is null)
        {
            throw new NoAvailableSpotException(request.VehicleType.ToString());
        }

        availableSpot.Occupy();//we arent directly changin the variable to true so this occupy func will change th epriv variable to true
        //use of encapsulation to change the state of the parking spot


        var ticket = new ParkingTicket
        {//create a new parking ticket for the vehicle
            Vehicle = vehicle,
            ParkingSpot = availableSpot,
            EntryTime = DateTime.UtcNow,
            IsActive = true,
            FeeStrategyType = request.FeeStrategyType ?? FeeStrategyType.Hourly//if the fee strategy type is null 
            //then use the default hourly fee strategy
        };

        _context.ParkingTickets.Add(ticket);//now add the ticket to the database
        await _context.SaveChangesAsync(cancellationToken);

        ticket = await LoadTicketAsync(ticket.Id, cancellationToken);
        return ParkingMapper.ToResponse(ticket);//parking mapper will map the ticket to the response DTO
        //cuz obviously client doesnt need extra details
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
        //joins
            .Include(ticket => ticket.Vehicle)//include the vehicle associated with the ticket
            .Include(ticket => ticket.ParkingSpot)//include the parking spot associated with the ticket
            .ThenInclude(spot => spot.Floor)//include the floor associated with the parking spot
            .AsNoTracking()//no tracking means we are not going to modify the ticket toh track krne ki zaroorat nahin
            .FirstOrDefaultAsync(ticket => ticket.Id == ticketId, cancellationToken)
            ?? throw new TicketNotFoundException(ticketId);
    }
}
