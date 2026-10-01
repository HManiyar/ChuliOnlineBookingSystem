using ChuliTirth.Data;
using ChuliTirth.Interfaces;
using ChuliTirth.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChuliTirth.Services;

public class RoomService : IRoomService
{
    private readonly ApplicationDbContext _db;

    public RoomService(ApplicationDbContext db) => _db = db;

    public Task<List<RoomType>> GetActiveRoomTypesAsync() =>
        _db.RoomTypes.Where(r => r.IsActive)
            .Include(r => r.Images)
            .Include(r => r.RoomAmenities).ThenInclude(ra => ra.Amenity)
            .OrderBy(r => r.DisplayOrder)
            .AsNoTracking()
            .ToListAsync();

    public Task<RoomType?> GetRoomTypeByIdAsync(int id) =>
        _db.RoomTypes
            .Include(r => r.Images)
            .Include(r => r.RoomAmenities).ThenInclude(ra => ra.Amenity)
            .Include(r => r.Rooms)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

    public Task<List<RoomType>> GetAllRoomTypesAsync() =>
        _db.RoomTypes.OrderBy(r => r.DisplayOrder).ToListAsync();

    public async Task<RoomType> CreateRoomTypeAsync(RoomType roomType)
    {
        _db.RoomTypes.Add(roomType);
        await _db.SaveChangesAsync();
        return roomType;
    }

    public async Task UpdateRoomTypeAsync(RoomType roomType)
    {
        _db.RoomTypes.Update(roomType);
        await _db.SaveChangesAsync();
    }

    public async Task DeactivateRoomTypeAsync(int id)
    {
        var rt = await _db.RoomTypes.FindAsync(id);
        if (rt is null) return;
        rt.IsActive = false;
        await _db.SaveChangesAsync();
    }

    public Task<List<Room>> GetRoomsByTypeAsync(int roomTypeId) =>
        _db.Rooms.Where(r => r.RoomTypeId == roomTypeId).OrderBy(r => r.RoomNumber).ToListAsync();

    public Task<List<Room>> GetAllRoomsAsync() =>
        _db.Rooms.Include(r => r.RoomType).OrderBy(r => r.RoomTypeId).ThenBy(r => r.RoomNumber).ToListAsync();

    public async Task<Room> CreateRoomAsync(Room room)
    {
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();
        return room;
    }

    public async Task UpdateRoomAsync(Room room)
    {
        _db.Rooms.Update(room);
        await _db.SaveChangesAsync();
    }

    public async Task SetRoomStatusAsync(int roomId, RoomStatus status)
    {
        var room = await _db.Rooms.FindAsync(roomId);
        if (room is null) return;
        room.Status = status;
        await _db.SaveChangesAsync();
    }
}
