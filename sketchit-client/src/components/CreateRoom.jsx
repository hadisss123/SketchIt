import { useState } from 'react'


function CreateRoom({ onJoinSuccess, displayName }) {
    const [roomData, setRoomData] = useState(null);


    async function handleCreateRoom() {
        if (displayName == "") {
            alert("Please enter a display name before creating a Room!");
            return;
        }

        try {
            const res = await fetch("http://localhost:5230/api/rooms", {
                method: "POST",
                headers: {
                    Accept: "*/*",
                },
                body: JSON.stringify(displayName)
            });

            if (!res.ok) {
                alert("Something went wrong.");
                return;
            }

            const json = await res.json();

            setRoomData(json);

            alert("Room " + JSON.stringify(json.code) + " created");

            onJoinSuccess(json.code, displayName);
        }
        catch (err) {
            console.error(err)
        }
    }

    return (
        <>
            <button onClick={handleCreateRoom}>
                Create Room
            </button>
        </>
    );
};


export default CreateRoom;