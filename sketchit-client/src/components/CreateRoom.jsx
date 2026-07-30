import { useState } from 'react'


function CreateRoom() {
    const [roomData, setRoomData] = useState(null);
    function handleCreateRoom() {
        fetch("http://localhost:5230/api/Rooms", {
            method: "POST",
            headers: {
                Accept: "*/*",
            }
        })
            .then((res) => res.json())
            .then((json) => {
                setRoomData(json);
                alert("Room " + JSON.stringify(json.code) + " created");
            }
            )
            .catch((err) => console.error(err));
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