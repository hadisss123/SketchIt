using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.IdentityModel.Tokens;
using SketchIt.Api.Models;
using System.Collections.Concurrent;
using System.Numerics;


namespace SketchIt.Api.Services
{

    public class ScoreState
    {
        private readonly ConcurrentDictionary<string, List<string>> playersGuessedCorrectly = new();

        public List<string> GetGuessedPlayers(string roomCode)
        {
            playersGuessedCorrectly.TryGetValue(roomCode, out var playersGuessed);
            if(playersGuessed != null)
            {
                return playersGuessed;
            }

            return new List<string>();
        }

        public void PlayerGuessedCorrectly(string roomCode, string playerName)
        {
            var playersGuessed = playersGuessedCorrectly.GetOrAdd(roomCode, new List<string>());

            lock (playersGuessed)
            {
                playersGuessed.Add(playerName);
            }
        }

        /*public bool AllPlayersGuessedCorrectly(string roomCode, List<string> playingPlayers)
        {
            bool same = false;
            playersGuessedCorrectly.TryGetValue(roomCode, out var playersGuessed);

            if(playersGuessed != null)
            {
                lock (playersGuessed)
                {
                    same = playersGuessed.OrderBy(x => x).SequenceEqual(playingPlayers.OrderBy(x => x));

                }
            }
            

            return same;
        }*/

        public void ClearPlayersGuessedCorrectly(string roomCode)
        {
            playersGuessedCorrectly.TryRemove(roomCode, out _);
        }
    }
}
