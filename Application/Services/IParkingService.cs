using OnlineParkingLotSystem.Application.DTOs.Requests;
using OnlineParkingLotSystem.Application.DTOs.Responses;

namespace OnlineParkingLotSystem.Application.Services;

public interface IParkingService
{
    Task<ParkingTicketResponse> ParkVehicleAsync(ParkVehicleRequest request, CancellationToken cancellationToken = default);
    Task<ExitParkingResponse> ExitVehicleAsync(int ticketId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicketResponse>> GetActiveTicketsAsync(CancellationToken cancellationToken = default);
    Task<ParkingTicketResponse> GetTicketByIdAsync(int ticketId, CancellationToken cancellationToken = default);
}
