using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private ConcurrentDictionary<string, List<string>> playersLeftToPlay = new();

        private ConcurrentDictionary<string, int> currentRound = new();

        public List<string> StartGame(string roomCode, List<string> players)
        {
            var random = new Random();

            var shuffledPlayers = players.OrderBy(_ => random.Next()).ToList();

            var playersLeft = playersLeftToPlay.GetOrAdd(roomCode, _ => new List<string>());
            lock (playersLeft)
            {
                playersLeft.AddRange(shuffledPlayers);
            }

            return playersLeft;
        }

        public int NewRound(string roomCode)
        {
            currentRound.AddOrUpdate(roomCode, 1, (_, round) => round + 1);
            currentRound.TryGetValue(roomCode, out var round);

            if(playersLeftToPlay.TryGetValue(roomCode, out var playersLeft))
            {
                lock (playersLeft)
                {
                    if (playersPlayed.TryGetValue(roomCode, out var playersDone))
                    {
                        lock (playersDone)
                        {
                            playersLeft.AddRange(playersDone);
                            playersDone.Clear();
                        }
                    }
                }
                
            }

            return round;
        }

        public int GetCurrentRound(string roomCode)
        {
            currentRound.TryGetValue(roomCode, out var round);
            return round;
        }

        public void AddToPlayersLeftToPlay(string roomCode, string player)
        {
            var players = playersLeftToPlay.GetOrAdd(roomCode, _ => new List<string>());
            lock (players)
            {
                players.Add(player);
            }

        }

        //public string? GetAndRemoveRandomPlayer(string roomCode)
        //{
        //    if(!playersLeftToPlay.TryGetValue(roomCode, out var players))
        //    {
        //        return null;
        //    }

        //    lock (players)
        //    {
        //        if (players.Count == 0)
        //        {
        //            if (playersPlayed.TryGetValue(roomCode, out var playersDone))
        //            {
        //                lock (playersDone)
        //                {
        //                    players.AddRange(playersDone);
        //                    playersDone.Clear();
        //                }
        //            }
        //            currentRound.AddOrUpdate(roomCode, 1, (_, round) => round + 1);
        //        }
        //        if (players.Count == 0)
        //        {
        //            return null;
        //        }

        //        var random = new Random();
        //        var index = random.Next(players.Count);

        //        var player = players[index];

        //        players.RemoveAt(index);

        //        return player;
        //    }
        //}

        public void AddToPlayersPlayed(string roomCode, string player)
        {
            var players = playersPlayed.GetOrAdd(roomCode, _ => new List<string>());
            lock (players)
            {
                players.Add(player);
            }
        }

        public void RemoveFromPlayersLeftToPlay(string roomCode, string player)
        {
            if(playersLeftToPlay.TryGetValue(roomCode, out var players))
            {
                lock (players)
                {
                    players.Remove(player);
                }
            }
            
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

        public List<string> GetPlayersLeftToPlay(string roomCode)
        {
            if(playersLeftToPlay.TryGetValue(roomCode, out var playersList))
            {
                lock (playersList)
                {
                    return new List<string>(playersList);
                }
                
            }
            return new List<string>();
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
