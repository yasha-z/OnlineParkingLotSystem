using Microsoft.EntityFrameworkCore;
using OnlineParkingLotSystem.Domain.Entities;
using OnlineParkingLotSystem.Domain.Enums;

namespace OnlineParkingLotSystem.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ParkingLot> ParkingLots => Set<ParkingLot>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<ParkingSpot> ParkingSpots => Set<ParkingSpot>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<ParkingTicket> ParkingTickets => Set<ParkingTicket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ParkingLot>(entity =>
        {
            entity.HasKey(parkingLot => parkingLot.Id);
            entity.Property(parkingLot => parkingLot.Name).HasMaxLength(100).IsRequired();
            entity.Property(parkingLot => parkingLot.Location).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<Floor>(entity =>
        {
            entity.HasKey(floor => floor.Id);
            entity.HasOne(floor => floor.ParkingLot)
                .WithMany(parkingLot => parkingLot.Floors)
                .HasForeignKey(floor => floor.ParkingLotId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ParkingSpot>(entity =>
        {
            entity.HasKey(spot => spot.Id);
            entity.Property(spot => spot.SpotNumber).HasMaxLength(20).IsRequired();
            entity.Property(spot => spot.IsOccupied).HasField("_isOccupied");
            entity.HasDiscriminator(spot => spot.SpotType)
                .HasValue<CompactSpot>(SpotType.Compact)
                .HasValue<LargeSpot>(SpotType.Large)
                .HasValue<HandicappedSpot>(SpotType.Handicapped);
            entity.HasOne(spot => spot.Floor)
                .WithMany(floor => floor.ParkingSpots)
                .HasForeignKey(spot => spot.FloorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(vehicle => vehicle.Id);
            entity.Property(vehicle => vehicle.LicensePlate).HasMaxLength(20).IsRequired();
            entity.HasIndex(vehicle => vehicle.LicensePlate).IsUnique();
            entity.HasDiscriminator(vehicle => vehicle.VehicleType)
                .HasValue<Motorcycle>(VehicleType.Motorcycle)
                .HasValue<Car>(VehicleType.Car)
                .HasValue<Truck>(VehicleType.Truck);
        });

        modelBuilder.Entity<ParkingTicket>(entity =>
        {
            entity.HasKey(ticket => ticket.Id);
            entity.Property(ticket => ticket.FeePaid).HasPrecision(10, 2);
            entity.HasOne(ticket => ticket.Vehicle)
                .WithMany(vehicle => vehicle.ParkingTickets)
                .HasForeignKey(ticket => ticket.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(ticket => ticket.ParkingSpot)
                .WithMany()
                .HasForeignKey(ticket => ticket.ParkingSpotId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
