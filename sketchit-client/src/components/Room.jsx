import { useRef, useState, useEffect } from 'react'
import { HubConnectionBuilder } from '@microsoft/signalr';

function Room({ roomCode, displayName }) {
    const connectionRef = useRef(null);
    const [playerLogs, setPlayerLogs] = useState([]);
    const [allPlayers, setAllPlayers] = useState([]);

    useEffect(() => {
        let isActive = true;
        const connection = new HubConnectionBuilder()
            .withUrl("http://localhost:5230/gamehub")
            .withAutomaticReconnect()
            .build();

        connection.on("NewPlayer", (name) => {
            setPlayerLogs((prevPlayers) => [...prevPlayers, `${name} connected`]);
        });

        connection.on("PlayerLeft", (name) => {
            setPlayerLogs((prevPlayers) => [...prevPlayers, `${name} disconnected`]);
        });

        connection.on("AllPlayers", (playersList) => {
            setAllPlayers(playersList);
        });

        connection.start().then(() => {
            if (isActive) {
                connection.invoke("JoinRoom", roomCode, displayName);
            }
        });

        connectionRef.current = connection;

        return () => {
            isActive = false;
            connection.stop();
        };


    }, [roomCode, displayName]);

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
        </div>
    )
}

export default Room;