using Microsoft.AspNetCore.SignalR;
using SketchIt.Api.Models;
using SketchIt.Api.Services;
using System.Collections.Concurrent;
using System.Drawing;

namespace SketchIt.Api.Hubs;

public class GameHub: Hub
{
    private readonly RoomState _roomState;
    private readonly RoomService _roomService;
    private List<string> reservedRoomCodes = new();

    public GameHub(RoomState roomState, RoomService roomService)
    {
        _roomState = roomState;
        _roomService = roomService;
        reservedRoomCodes.Add("NAAL");
    }
    public async Task JoinRoom (string roomCode, string displayName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
        await Clients.OthersInGroup(roomCode).SendAsync("NewPlayer", displayName);
        _roomState.AddToRoom(roomCode, displayName, Context.ConnectionId);
        await Clients.Group(roomCode).SendAsync("AllPlayers", _roomState.GetPlayers(roomCode));

    }

    public async Task StartGame(string roomCode, string displayName)
    {
        var host = await _roomService.GetHost(roomCode);
        if (host == displayName)
        {
            await _roomService.ChangeRoomStatus(roomCode, RoomStatus.InProgress);

            var players = _roomState.GetPlayers(roomCode);

            var playersLeft = _roomState.StartGame(roomCode, players);
            //_roomState.StartGame(roomCode);

            await Clients.Group(roomCode).SendAsync("GameStarted", playersLeft);
            await NewPlayerToDraw(roomCode);
        }
        
    }
    public async Task NewPlayerToDraw(string roomCode)
    {
        var (currentPlayer, round) =  _roomState.GetAndAdvanceDrawer(roomCode);
        _roomState.ClearDrawing(roomCode);
        await Clients.Group(roomCode).SendAsync("NewPlayerToDraw", currentPlayer, round);

    }




    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var result = _roomState.RemoveFromRoom(Context.ConnectionId);
        if(result != null)
        {
            var roomCode = result?.roomCode;
            var displayName = result?.displayName;
            var isRoomEmpty = result?.isRoomEmpty ?? true;
            if (roomCode != null && displayName != null) {
                var hostName = await _roomService.GetHost(roomCode);
                var allPlayers = _roomState.GetPlayers(roomCode);
                if (isRoomEmpty)
                {
                    _roomState.ClearRoom(roomCode);
                    await _roomService.ChangeRoomStatus(roomCode, RoomStatus.Waiting);
                    if (!reservedRoomCodes.Contains(roomCode))
                    {
                        await _roomService.RemoveRoom(roomCode);
                    }
                    
                }
                else 
                {
                    if (displayName == hostName)
                    {
                        var newHost = await _roomService.HostLeftChangeHost(roomCode, allPlayers);
                        await Clients.Group(roomCode).SendAsync("HostChanged", newHost);
                    }

                    var playersPlayed = _roomState.GetPlayersPlayed(roomCode);
                    var playerToDraw = playersPlayed.Last();
                    if (displayName == playerToDraw)
                    {
                        _roomState.RemoveFromPlayersPlayed(roomCode, playerToDraw);
                        await NewPlayerToDraw(roomCode);
                    }
                }
                
                await Clients.OthersInGroup(roomCode).SendAsync("PlayerLeft", displayName);
                await Clients.Group(roomCode).SendAsync("AllPlayers", allPlayers);
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
