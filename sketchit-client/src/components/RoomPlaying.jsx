import { useRef, useState, useEffect } from 'react'
import { HubConnectionBuilder } from '@microsoft/signalr';
import Canvas from "./Canvas";
import Chat from "./Chat"

function RoomPlaying({ roomCode, displayName, connectionRef, playerLogs, allPlayers, roomStatus, setRoomStatus, isConnected, playerToDraw, currentRound, wordsToChoose, startDrawing, chosenWord, guessedPlayers, chat }) {
    const playerDrawing = displayName === playerToDraw;

    const sendChosenWord = async (word) => {
        try {
            await connectionRef.current.invoke("ChosenWord", roomCode, word);
        }
        catch (err) {
            console.error("Failed to select word:", err);
        }
    }

    return (
        <>

            <div>
                {!startDrawing && playerDrawing && wordsToChoose && (
                    <div>
                        <h3>Choose a word:</h3>

                        {wordsToChoose.map((word) => (
                            <button
                                key={word}
                                onClick={() => sendChosenWord(word)}
                            >
                                {word}
                            </button>
                        ))}
                    </div>
                )}
                {playerDrawing ?
                    (
                        <div>
                            You are drawing
                        </div>
                    ) : (
                        <div>
                            {playerToDraw} is Drawing
                        </div>
                    )

                }
                {startDrawing && (
                    <div>
                        {chosenWord}
                    </div>
                )}


                {(
                    <div style={{ display: "grid", gridTemplateColumns: "1fr auto 1fr", alignItems: "start" }}>
                        <div /> { }
                        <Canvas
                            connectionRef={connectionRef}
                            roomCode={roomCode}
                            isConnected={isConnected}
                            playerToDraw={playerToDraw}
                            displayName={displayName}
                            width={300}
                            height={450}
                            startDrawing={startDrawing}
                        />
                        <div style={{ justifySelf: "end" }}>
                            <Chat
                                connectionRef={connectionRef}
                                roomCode={roomCode}
                                playerDrawing={playerDrawing}
                                chat={chat}
                                startDrawing={startDrawing}
                            />
                        </div>
                    </div>
                )}

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