using SketchIt.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SketchIt.Api.Services
{
    public class RoomService
    {
        private readonly SketchItDbContext _db;

        public RoomService(SketchItDbContext db)
        {
            _db = db;
        }

        public async Task ChangeRoomStatus(string roomCode, RoomStatus status)
        {
            var room = await _db.Rooms.FirstOrDefaultAsync(r => r.Code == roomCode);

            if (room != null)
            {
                room.Status = status;
                await _db.SaveChangesAsync();
            }
        }

        public async Task RemoveRoom(string roomCode)
        {
            var room = await _db.Rooms.FirstOrDefaultAsync(r => r.Code == roomCode);
            if (room != null)
            {
                _db.Rooms.Remove(room);
                await _db.SaveChangesAsync();
            }
                
        }

        public async Task<string?> GetHost(string roomCode)
        {
            var room = await _db.Rooms.FirstOrDefaultAsync(r => r.Code == roomCode);
            if (room != null)
            {
                return room.Host;
            }
            return null;
        }

        public async Task<string?> HostLeftChangeHost(string roomCode, List<string> allPlayers)
        {
            var room = await _db.Rooms.FirstOrDefaultAsync(r => r.Code == roomCode);
            if (room != null && allPlayers.Count > 0)
            {
                var random = new Random();
                room.Host = allPlayers[random.Next(allPlayers.Count)];

                await _db.SaveChangesAsync();

                return room.Host;
            }
            return null;
            
        }
    }
}
