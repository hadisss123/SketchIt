import { useRef, useState, useEffect } from 'react'
import { HubConnectionBuilder } from '@microsoft/signalr';
import Canvas from "./Canvas";

function Room({ roomCode, displayName }) {
    const connectionRef = useRef(null);
    const [playerLogs, setPlayerLogs] = useState([]);
    const [allPlayers, setAllPlayers] = useState([]);
    const [isConnected, setIsConnected] = useState(false);
    const [roomStatus, setRoomStatus] = useState("Waiting");

    async function handleStartGame() {
        setRoomStatus("Playing");
    }

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
                setIsConnected(true);
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
            {
                roomStatus="Waiting" ? (
                    <RoomWaiting roomCode={joinedRoom.roomCode}/>
                )
            }
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
            <ul>
                <button onClick={handleStartGame}>
                    Start Game
                </button>
            </ul>
        </div>
    )
}

export default Room;