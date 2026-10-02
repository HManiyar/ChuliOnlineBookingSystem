using ChuliTirth.Models.Entities;

namespace ChuliTirth.Interfaces;

public interface IRoomService
{
    Task<List<RoomType>> GetActiveRoomTypesAsync();
    Task<RoomType?> GetRoomTypeByIdAsync(int id);
    Task<List<RoomType>> GetAllRoomTypesAsync();
    Task<RoomType> CreateRoomTypeAsync(RoomType roomType);
    Task UpdateRoomTypeAsync(RoomType roomType);
    Task DeactivateRoomTypeAsync(int id);

    Task<List<Room>> GetAllRoomsAsync();
    Task<Room> CreateRoomAsync(Room room);
    Task UpdateRoomAsync(Room room);
    Task SetRoomStatusAsync(int roomId, RoomStatus status);
}
