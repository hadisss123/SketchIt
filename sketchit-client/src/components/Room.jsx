import { useRef, useState, useEffect } from 'react'
import { HubConnectionBuilder } from '@microsoft/signalr';
import Canvas from "./Canvas";
import RoomWaiting from "./RoomWaiting"
import RoomPlaying from "./RoomPlaying"
//import RoomFinished from "./RoomFinished"

function Room({ roomCode, displayName, hostName, setHostName }) {
    const connectionRef = useRef(null);
    const [playerLogs, setPlayerLogs] = useState([]);
    const [allPlayers, setAllPlayers] = useState([]);
    const [isConnected, setIsConnected] = useState(false);
    const [roomStatus, setRoomStatus] = useState("Waiting");
    const [playerToDraw, setPlayerToDraw] = useState("");
    const [currentRound, setCurrentRound] = useState(null);

    const isHost = displayName === hostName;


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
        connection.on("GameStarted", (playersLeft) => {
            setRoomStatus("Playing");
            console.log(playersLeft);
        });

        connection.on("NewPlayerToDraw", (playerToDraw) => {
            setPlayerToDraw(playerToDraw);
            console.log(playerToDraw);

        });

        connection.on("NewRound", (currentRound) => {
            setCurrentRound(currentRound);
            console.log(currentRound);
        });

        connection.on("HostChanged", (host) =>
            setHostName(host)
        );


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


    }, [roomCode, displayName, setHostName]);

    return (
        <>
            {
                roomStatus == "Waiting" ? (
                    <RoomWaiting
                        roomCode={roomCode}
                        displayName={displayName}
                        connectionRef={connectionRef}
                        playerLogs={playerLogs}
                        allPlayers={allPlayers}
                        roomStatus={roomStatus}
                        setRoomStatus={setRoomStatus}
                        isHost={isHost}
                    />
                ) : roomStatus == "Playing" ? (
                    <RoomPlaying
                        roomCode={roomCode}
                        displayName={displayName}
                        connectionRef={connectionRef}
                        playerLogs={playerLogs}
                        allPlayers={allPlayers}
                        roomStatus={roomStatus}
                        setRoomStatus={setRoomStatus}
                        isConnected={isConnected}
                        isHost={isHost}
                        playerToDraw={playerToDraw}
                        currentRound={currentRound}
                    />
                ) /*: roomStatus == "Finished" ? (
                    <RoomFinished roomCode={roomCode} displayName={displayName} />
                )*/ : "Loading"
            }

        </>
    );
}

export default Room;