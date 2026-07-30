using Microsoft.AspNetCore.SignalR;
using SketchIt.Api.Services;
using System.Collections.Concurrent;

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

}
