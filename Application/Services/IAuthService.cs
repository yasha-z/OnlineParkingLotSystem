using OnlineParkingLotSystem.Application.DTOs.Requests;
using OnlineParkingLotSystem.Application.DTOs.Responses;

namespace OnlineParkingLotSystem.Application.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
