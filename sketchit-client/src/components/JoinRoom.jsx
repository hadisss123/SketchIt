import { useRef, useState } from 'react'
import { HubConnectionBuilder } from '@microsoft/signalr';
import Room from './Room';

function JoinRoom({ onJoinSuccess }) {
    const [roomCode, setRoomCode] = useState("");
    const [displayName, setDisplayName] = useState("");

    async function handleJoinRoom() {

        try {
            const res = await fetch(`http://localhost:5230/api/Rooms?Code=${roomCode}`);
            if (res.status === 404) {
                alert("Room not found (404)");
                return;
            }

            if (!res.ok) {
                alert("Something went wrong.");
                return;
            }
            else {
                onJoinSuccess(roomCode, displayName);
            }
        }
        catch (err) {
            console.error(err);
        }
    }

    return (
        <>
            <input
                placeholder='Enter Room Code'
                value={roomCode}
                onChange={(event) => {
                    setRoomCode(event.target.value);
                }}
            />
            <input
                placeholder='Enter Display Name'
                value={displayName}
                onChange={(event) => {
                    setDisplayName(event.target.value);
                }}
            />

            <button onClick={handleJoinRoom}>
                Join Room
            </button>
        </>
    )
};



export default JoinRoom;