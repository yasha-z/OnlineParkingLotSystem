using Microsoft.EntityFrameworkCore;
using OnlineParkingLotSystem.Domain.Entities;

namespace OnlineParkingLotSystem.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.ParkingLots.AnyAsync())
        {
            return;
        }

        var parkingLot = new ParkingLot
        {
            Name = "City Center Parking",
            Location = "Main Street, Downtown",
            Floors =
            [
                CreateFloor(1,
                [
                    ("C-101", new CompactSpot()),
                    ("C-102", new CompactSpot()),
                    ("C-103", new CompactSpot()),
                    ("C-104", new CompactSpot()),
                    ("C-105", new CompactSpot()),
                    ("L-101", new LargeSpot()),
                    ("L-102", new LargeSpot()),
                    ("L-103", new LargeSpot()),
                    ("H-101", new HandicappedSpot()),
                    ("H-102", new HandicappedSpot())
                ]),
                CreateFloor(2,
                [
                    ("C-201", new CompactSpot()),
                    ("C-202", new CompactSpot()),
                    ("C-203", new CompactSpot()),
                    ("C-204", new CompactSpot()),
                    ("C-205", new CompactSpot()),
                    ("L-201", new LargeSpot()),
                    ("L-202", new LargeSpot()),
                    ("L-203", new LargeSpot()),
                    ("H-201", new HandicappedSpot()),
                    ("H-202", new HandicappedSpot())
                ])
            ]
        };

        context.ParkingLots.Add(parkingLot);
        await context.SaveChangesAsync();
    }

    private static Floor CreateFloor(int floorNumber, IEnumerable<(string SpotNumber, ParkingSpot Spot)> spots)
    {
        var floor = new Floor { FloorNumber = floorNumber };

        foreach (var (spotNumber, spot) in spots)
        {
            spot.SpotNumber = spotNumber;
            floor.ParkingSpots.Add(spot);
        }

        return floor;
    }
}
