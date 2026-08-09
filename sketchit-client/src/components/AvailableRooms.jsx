import { useEffect, useState } from "react";

function AvailableRooms() {
    const [rooms, setRooms] = useState([]);

    useEffect(() => {
        async function loadRooms() {
            const res = await fetch("http://localhost:5230/api/rooms")
            const json = await res.json();
            setRooms(json);
        }

        loadRooms();
    })

    return (
        <>
            {rooms.map(room => (
                <li key={room}>{room}</li>
            ))}
        </>
    )
}



export default AvailableRooms;