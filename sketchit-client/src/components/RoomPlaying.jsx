import { useRef, useState, useEffect } from 'react'
import { HubConnectionBuilder } from '@microsoft/signalr';
import Canvas from "./Canvas";

function RoomPlaying({ roomCode, displayName, connectionRef, playerLogs, allPlayers, roomStatus, setRoomStatus, isConnected, playerToDraw, currentRound }) {


    return (
        <>
            <div>
                    <Canvas
                        connectionRef={connectionRef}
                        roomCode={roomCode}
                        isConnected={isConnected}
                        playerToDraw={playerToDraw}
                        displayName={displayName}
                        width={1000}
                        height={850}
                    />
                
            </div>
            <button
                onClick={() =>
                    connectionRef.current.invoke("NewPlayerToDraw", roomCode)
                }
            >
                Force New Player
            </button>

            {currentRound}
        </>
    )
}

export default RoomPlaying;