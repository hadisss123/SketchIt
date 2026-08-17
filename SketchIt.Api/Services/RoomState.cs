using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.IdentityModel.Tokens;
using SketchIt.Api.Models;
using System.Collections.Concurrent;
using System.Numerics;


namespace SketchIt.Api.Services
{

    public class RoomState
    {
        //Mapping roomCode to players list
        private readonly ConcurrentDictionary<string, List<string>> playersList = new();

        //Mapping connectionId to tuple of roomCode and playerName to know what player & roomCode a connectionId points to
        private readonly ConcurrentDictionary<string, (string roomCode, string playerName)> roomMapping = new();

        //Mapping roomCode to drawings on the canvas so each player gets the same drawings
        private readonly ConcurrentDictionary<string, List<DrawPoint>> drawings = new();

        private ConcurrentDictionary<string, List<string>> playersPlayed = new();

        private ConcurrentDictionary<string, string> playerPlaying = new();

        private ConcurrentDictionary<string, List<string>> playersLeftToPlay = new();

        private ConcurrentDictionary<string, int> currentRound = new();


        public void ClearDrawing(string roomCode)
        {
            lock (drawings)
            {
                drawings.TryGetValue(roomCode, out var canvasDrawings);
                if (canvasDrawings != null)
                {
                    canvasDrawings.Clear();
                }
            }
           
            
        }

        public (string?, int currentRound) GetAndAdvanceDrawer(string roomCode)
        {
            var playersLeft = playersLeftToPlay.GetOrAdd(roomCode, _ => new List<string>());
            var playersDone = playersPlayed.GetOrAdd(roomCode, _ => new List<string>());
            

            lock (playersDone)
            {
                lock (playersLeft)
                {
                    lock (playerPlaying)
                    {
                        if (!playerPlaying.ContainsKey(roomCode))
                        {
                            playerPlaying.GetOrAdd(roomCode, _ => playersLeft[0]);
                            playersLeft.RemoveAt(0);

                        }
                        else
                        {
                            playersDone.Add(playerPlaying[roomCode]);

                            if (playersLeft.Count == 0)
                            {
                                playersLeft.AddRange(playersDone);
                                playersDone.Clear();
                                currentRound.AddOrUpdate(roomCode, 1, (_, round) => round + 1);
                            }
                            playerPlaying[roomCode] = playersLeft[0];
                            playersLeft.RemoveAt(0);
                        }


                        currentRound.TryGetValue(roomCode, out var round);
                        return (playerPlaying[roomCode], round);
                    }
                    
                }
            }
        }

        public List<string> StartGame(string roomCode, List<string> players)
        {
            

            var random = new Random();

            var shuffledPlayers = players.OrderBy(_ => random.Next()).ToList();

            var playersLeft = playersLeftToPlay.GetOrAdd(roomCode, _ => new List<string>());
            lock (playersLeft)
            {
                playersLeft.Clear();
                playersLeft.AddRange(shuffledPlayers);
            }

            return playersLeft;
        }




        public void ClearRoom(string roomCode)
        {
            playerPlaying.TryRemove(roomCode, out _);
            playersLeftToPlay.TryRemove(roomCode, out _);
            playersPlayed.TryRemove(roomCode, out _);
            roomMapping.TryRemove(roomCode, out _);
            drawings.TryRemove(roomCode, out _);
            currentRound.TryRemove(roomCode, out _);
        }


        public void RemoveFromPlayersPlayed(string roomCode, string player)
        {
            if(playersPlayed.TryGetValue(roomCode, out var players)){
                lock (players)
                {
                    players.Remove(player);
                }
            }
            
        }

       
        public List<string> GetPlayersPlayed(string roomCode)
        {
            if (playersPlayed.TryGetValue(roomCode, out var playersList))
            {
                lock (playersList)
                {
                    return new List<string>(playersList);
                }
                    
            }
            return new List<string>();
        }

        public class DrawPoint
        {
            public double X { get; set; }
            public double Y { get; set; }

            public DrawPoint(double x, double y)
            {
                X = x;
                Y = y;
            }
        }



        public void AddToPoints(string roomCode, double x1, double y1, double x2, double y2)
        {
            var points = drawings.GetOrAdd(roomCode, _ => new List<DrawPoint>());

            lock (points)
            {
                points.Add(new DrawPoint(x1, y1));
                points.Add(new DrawPoint(x2, y2));
            }

            
        }

        public List<DrawPoint> GetAllPoints(string roomCode)
        {
            if (drawings.TryGetValue(roomCode, out var points))
            {
                lock (points)
                {
                    return new List<DrawPoint>(points);
                }
            }
            return new List<DrawPoint>();
        }

        public void AddToRoom(string roomCode, string displayName, string connectionId)
        {
            var players = playersList.GetOrAdd(roomCode, _ => new List<string>());
            lock (players)
            {
                players.Add(displayName);
            }

            roomMapping.TryAdd(connectionId, (roomCode, displayName));
        }
        public (string roomCode, string displayName, bool isRoomEmpty)? RemoveFromRoom(string connectionId)
        {
            if(roomMapping.TryGetValue(connectionId, out var roomPlayer))
            {
                var roomCode = roomPlayer.roomCode;
                var displayName = roomPlayer.playerName;
                var isRoomEmpty = false;

                if (playersList.TryGetValue(roomCode, out var players))
                {
                    lock (players)
                    {
                        players.Remove(displayName);
                        if (players.Count == 0)
                        {
                            isRoomEmpty = true;

                            playersList.TryRemove(roomCode, out _);
                            drawings.TryRemove(roomCode, out _);

                            playersPlayed.TryRemove(roomCode, out _);
                            playersLeftToPlay.TryRemove(roomCode, out _);

                            currentRound.TryRemove(roomCode, out _);
                        }
                    }
                }

                roomMapping.Remove(connectionId, out _);

                return (roomCode, displayName, isRoomEmpty);
            }

            return null;
            
        }

        public List<string> GetPlayers(string roomCode)
        {
            if(playersList.TryGetValue(roomCode, out var players))
            {
                lock (players)
                {
                    return new List<string>(players);
                }
                
            }
            return new List<string>();
        }

    }
}
