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

        private ConcurrentDictionary<string, string> currentWord = new();

        private ConcurrentDictionary<string, List<string>> playersLeftToPlay = new();

        private ConcurrentDictionary<string, int> currentRound = new();

        private readonly string[] wordBank = { "car", "cat", "window", "door", "office", "table" };


        public int GetCurrentRound(string roomCode)
        {
            currentRound.TryGetValue(roomCode, out var round);
            return round;
        }

        public List<string> GetPlayersExceptDrawer(string roomCode)
        {
            var allPlayers = GetPlayers(roomCode);
            var playerDrawing = GetCurrentPlayer(roomCode);

            var allPlayersCopy = new List<string>(allPlayers);
            allPlayersCopy.Remove(playerDrawing); 
            return allPlayersCopy;
        }

        public string GetPlayerFromConnection(string connectionId)
        {
            roomMapping.TryGetValue(connectionId, out var player);

            return player.playerName;
        }

        public string GetCurrentPlayer(string roomCode)
        {
            return playerPlaying[roomCode];
        }

        public string GetChosenWord(string roomCode)
        {
            currentWord.TryGetValue(roomCode, out var word);
            if(word != null)
            {
                return word;
            }
            return "No Chosen Word";
        }

        public void SetChosenWord(string roomCode, string chosenWord)
        {
            currentWord.AddOrUpdate(roomCode, chosenWord, (_, _) => chosenWord);
        }

        public string? GetConnectionId(string roomCode, string displayName)
        {
            
                var result = roomMapping.FirstOrDefault(x =>
                    x.Value.roomCode == roomCode &&
                    x.Value.playerName == displayName);

                return result.Key;
            
        }

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

        public (string?, int currentRound, string word1, string word2, string word3) GetAndAdvanceDrawer(string roomCode)
        {
            var playersLeft = playersLeftToPlay.GetOrAdd(roomCode, _ => new List<string>());
            var playersDone = playersPlayed.GetOrAdd(roomCode, _ => new List<string>());
            if (!currentRound.ContainsKey(roomCode))
            {
                currentRound.AddOrUpdate(roomCode, 1, (_, round) => round + 1);
            }


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


                        var random = new Random();

                        var word1 = wordBank[random.Next(0, 6)];

                        var word2 = wordBank[random.Next(0, 6)];
                        while (word2 == word1)
                            word2 = wordBank[random.Next(0, 6)];

                        var word3 = wordBank[random.Next(0, 6)];
                        while (word3 == word1 || word3 == word2)
                            word3 = wordBank[random.Next(0, 6)];

                        
                        currentRound.TryGetValue(roomCode, out var round);
                        return (playerPlaying[roomCode], round, word1, word2, word3);
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
                    return players;
                }
                
            }
            return new List<string>();
        }

    }
}
