using System.ComponentModel.DataAnnotations;
using OnlineParkingLotSystem.Domain.Enums;

namespace OnlineParkingLotSystem.Application.DTOs.Requests;

public class ParkVehicleRequest
{
    [Required]
    [StringLength(20, MinimumLength = 2)]
    public string LicensePlate { get; set; } = string.Empty;

    [Required]
    [EnumDataType(typeof(VehicleType))]
    public VehicleType VehicleType { get; set; }

    [EnumDataType(typeof(FeeStrategyType))]
    public FeeStrategyType? FeeStrategyType { get; set; }
}
