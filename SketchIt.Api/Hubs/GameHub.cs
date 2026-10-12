using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.SignalR;
using SketchIt.Api.Models;
using SketchIt.Api.Services;
using System.Collections.Concurrent;
using System.Drawing;

namespace SketchIt.Api.Hubs;

public class GameHub: Hub
{
    private readonly RoomState _roomState;
    private readonly ScoreState _scoreState;
    private readonly RoomService _roomService;
    private List<string> reservedRoomCodes = new();

    public GameHub(RoomState roomState, RoomService roomService, ScoreState scoreState)
    {
        _roomState = roomState;
        _roomService = roomService;
        _scoreState = scoreState;
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

            await Clients.Group(roomCode).SendAsync("GameStarted", playersLeft);
            await NewPlayerToDraw(roomCode);
        }
        
    }

    public async Task GuessWord(string roomCode, string guess)
    {
        var player = _roomState.GetPlayerFromConnection(Context.ConnectionId);
        var correctWord = _roomState.GetChosenWord(roomCode);
        var guessedPlayers = _scoreState.GetGuessedPlayers(roomCode);

        var allPlayersExceptDrawer = _roomState.GetPlayersExceptDrawer(roomCode);

        if (guess == correctWord)
        {
            _scoreState.PlayerGuessedCorrectly(roomCode, player);

            guessedPlayers = _scoreState.GetGuessedPlayers(roomCode);

            await Clients.Caller.SendAsync("CorrectGuess", correctWord);
            await Clients.Group(roomCode).SendAsync("ReceiveMessage", "guessed correctly!", player, true, guessedPlayers);


            bool allPlayersGuessed = guessedPlayers.OrderBy(x => x).SequenceEqual(allPlayersExceptDrawer.OrderBy(x => x)); ;

            if (allPlayersGuessed)
            {
                await Clients.Group(roomCode).SendAsync("SystemMessage", "All Players Guessed Correctly!");
                await Task.Delay(3000);
                await NewPlayerToDraw(roomCode);
            }
        }
        else if(guess != correctWord)
        {
            await Clients.Group(roomCode).SendAsync("ReceiveMessage", guess, player, false, guessedPlayers);
        }

    }

    public async Task ChosenWord(string roomCode, string chosenWord)
    {
        var currentPlayer = _roomState.GetCurrentPlayer(roomCode);

        var connectionId = _roomState.GetConnectionId(roomCode, currentPlayer);

        if(Context.ConnectionId != connectionId)
        {
            return;
        }

        _roomState.SetChosenWord(roomCode, chosenWord);

        await Clients.Group(roomCode).SendAsync("StartDrawing");
        await Clients.GroupExcept(roomCode, connectionId).SendAsync("WordLength", chosenWord.Length);
        await Clients.Client(connectionId).SendAsync("ChosenWord", chosenWord);
    }

    public async Task NewPlayerToDraw(string roomCode)
    {
        var roundBefore = _roomState.GetCurrentRound(roomCode);
        var (currentPlayer, round, word1, word2, word3) =  _roomState.GetAndAdvanceDrawer(roomCode);

        //make new round function maybe

        if(roundBefore != 0 && round > roundBefore)
        {
            await Clients.Group(roomCode).SendAsync("SystemMessage", "Round " + _roomState.GetCurrentRound(roomCode) + " starting soon!");
            await Task.Delay(3000);
        }

        _scoreState.ClearPlayersGuessedCorrectly(roomCode);
        if (currentPlayer != null)
        {
            var drawerConnectionId = _roomState.GetConnectionId(roomCode, currentPlayer);

            _roomState.ClearDrawing(roomCode);
            await Clients.Group(roomCode).SendAsync("NewPlayerToDraw", currentPlayer, round);
            if(drawerConnectionId != null)
            {
                await Clients.Client(drawerConnectionId).SendAsync("ChooseWord", word1, word2, word3);
            }
        }
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
