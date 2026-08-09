import { useRef, useState } from 'react'
import Room from './Room';

function JoinRoom({ onJoinSuccess, displayName }) {
    const [roomCode, setRoomCode] = useState("");

    async function handleJoinRoom() {
        if (displayName == "") {
            alert("Please enter a display name before joining the Room!");
            return;
        }
        if (roomCode == "") {
            alert("Please enter a room code to join a Room!");
            return;
        }

        try {
            const res = await fetch(`http://localhost:5230/api/rooms/${roomCode.toUpperCase()}`);
            if (res.status === 404) {
                alert("Room not found (404)");
                return;
            }

            if (!res.ok) {
                alert("Something went wrong.");
                return;
            }
            else {
                const json = await res.json();
                console.log(json);
                if (json.status == 1) {
                    alert("Game has already started!")
                    return;
                }
                onJoinSuccess(roomCode.toUpperCase(), displayName, json.host);
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
                    setRoomCode(event.target.value.toUpperCase());
                }}
            />

            <button onClick={handleJoinRoom}>
                Join Room
            </button>
        </>
    )
};



export default JoinRoom;