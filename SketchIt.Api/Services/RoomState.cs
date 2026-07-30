using System.Collections.Concurrent;

namespace SketchIt.Api.Services
{
    public class RoomState
    {
        private readonly ConcurrentDictionary<string, List<string>> playersList = new();

        private readonly ConcurrentDictionary<string, (string roomCode, string playerName)> roomMapping = new();


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
