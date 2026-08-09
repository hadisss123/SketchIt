import { useRef, useState, useEffect } from 'react'

function RoomWaiting({ roomCode, displayName, connectionRef, playerLogs, allPlayers, roomStatus, setRoomStatus, isHost }) {
    async function handleStartGame() {
        if (isHost) {
            connectionRef.current.invoke("StartGame", roomCode, displayName);
        }
    }

    return (
        <div>
            <p>Connected to room {roomCode} as {displayName}</p>
            <ul>
                <>
                    {playerLogs.map((log, index) => (
                        <li key={index}>{log}</li>
                    ))}
                </>
            </ul>
            <ul>
                <p>All Players</p>
                {allPlayers.map((name, index) => (
                    <li key={index}>{name}</li>
                ))}
            </ul>
            {isHost && (
                <ul>
                    <button onClick={handleStartGame}>
                        Start Game
                    </button>
                </ul>
            )}

        </div>
    )
}

export default RoomWaiting;