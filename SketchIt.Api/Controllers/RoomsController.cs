using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SketchIt.Api.Models;

[ApiController]
[Route("api/[controller]")]

public class RoomsController: ControllerBase
{
    private readonly SketchItDbContext _db;

    public RoomsController(SketchItDbContext db)
    {
        _db = db;
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetRoom(string Code)
    {
        var room = await _db.Rooms.FirstOrDefaultAsync(r => r.Code == Code);

        return room == null ? NotFound() : Ok(room);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoom()
    {
        var alphabet = "abcdefghijklmnopqrstuvwxyz";
        Random random = new Random();
        string tempCode;
        do
        {
            tempCode = "";

            for (int i = 0; i < 4; i++)
            {
                tempCode += alphabet[random.Next(0, 25)];
            }
        }
        while (await _db.Rooms.AnyAsync(r => r.Code == tempCode));

        tempCode = tempCode.ToUpper();
        var room = new Room();
        room.Id = Guid.NewGuid();
        room.Code = tempCode;
        room.CreatedAt = DateTime.Now;
        room.Status = RoomStatus.Waiting;

        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();

        return Ok(room);
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableRooms()
    {
        var roomCodes = await _db.Rooms
            .Select(r => r.Code)
            .ToListAsync();

        return Ok(roomCodes);
    }
}