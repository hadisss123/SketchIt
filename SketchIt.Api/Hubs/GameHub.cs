using Microsoft.AspNetCore.SignalR;
using SketchIt.Api.Services;
using System.Collections.Concurrent;
using System.Drawing;

namespace SketchIt.Api.Hubs;

public class GameHub: Hub
{
    private readonly RoomState _roomState;

    public GameHub(RoomState roomState)
    {
        _roomState = roomState;
    }
    public async Task JoinRoom (string roomCode, string displayName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
        await Clients.OthersInGroup(roomCode).SendAsync("NewPlayer", displayName);
        _roomState.AddToRoom(roomCode, displayName, Context.ConnectionId);
        await Clients.Group(roomCode).SendAsync("AllPlayers", _roomState.GetPlayers(roomCode));

    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var result = _roomState.RemoveFromRoom(Context.ConnectionId);
        if(result != null)
        {
            var roomCode = result?.roomCode;
            var displayName = result?.displayName;
            if (roomCode != null && displayName != null) {
                await Clients.OthersInGroup(roomCode).SendAsync("PlayerLeft", displayName);
                await Clients.Group(roomCode).SendAsync("AllPlayers", _roomState.GetPlayers(roomCode));
            }

        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendDrawSegment (string roomCode, double x1, double y1, double x2, double y2)
    {
        _roomState.AddToPoints(roomCode, x1, y1, x2, y2);

        await Clients.OthersInGroup(roomCode).SendAsync("ReceiveDrawing", x1, y1, x2, y2);
    }

    public async Task GetEntireDrawing (string roomCode)
    {
        await Clients.Caller.SendAsync("ReceiveEntireDrawing", _roomState.GetAllPoints(roomCode));
    }
}
