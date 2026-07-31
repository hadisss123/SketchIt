using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;

namespace SketchIt.Api.Services
{
    public class RoomState
    {
        private readonly ConcurrentDictionary<string, List<string>> playersList = new();

        private readonly ConcurrentDictionary<string, (string roomCode, string playerName)> roomMapping = new();

        private readonly ConcurrentDictionary<string, List<DrawPoint>> drawings = new();

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
                    return points;
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
        public (string roomCode, string displayName)? RemoveFromRoom(string connectionId)
        {
            if(roomMapping.TryGetValue(connectionId, out var roomPlayer))
            {
                var roomCode = roomPlayer.roomCode;
                var displayName = roomPlayer.playerName;

                if (playersList.TryGetValue(roomCode, out var players))
                {
                    lock (players)
                    {
                        players.Remove(displayName);
                        if (players.Count == 0)
                        {
                            playersList.TryRemove(roomCode, out _);
                            drawings.TryRemove(roomCode, out _);
                        }
                    }
                }

                roomMapping.Remove(connectionId, out _);

                return (roomCode, displayName);
            }

            return null;
            
        }

        public List<string> GetPlayers(string roomCode)
        {
            if(playersList.TryGetValue(roomCode, out var players))
            {
                return players;
            }
            return new List<string>();
        }
        

    }
}
