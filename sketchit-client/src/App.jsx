import { useEffect, useState } from 'react'
import './App.css'
import CreateRoom from './components/CreateRoom'
import JoinRoom from './components/JoinRoom'
import AvailableRooms from './components/AvailableRooms'
import Room from './components/Room'

function App() {
  const [joinedRoom, setJoinedRoom] = useState(null);
  const [displayName, setDisplayName] = useState("");
  const [hostName, setHostName] = useState("");

  return (
    <>
      {
        joinedRoom ? (
          <Room roomCode={joinedRoom.roomCode} displayName={joinedRoom.displayName} hostName={hostName} setHostName={setHostName} />
        ) : (
          <>
            <input
              placeholder='Enter Display Name'
              value={displayName}
              onChange={(event) => {
                setDisplayName(event.target.value);
              }}
            />
            <JoinRoom
              onJoinSuccess={(roomCode, displayName, hostName) => {
                setJoinedRoom({ roomCode, displayName })
                setHostName(hostName);
              }}
              displayName={displayName}
            />
            <CreateRoom
              onJoinSuccess={(roomCode, displayName) => setJoinedRoom({ roomCode, displayName })}
              displayName={displayName}
            />
            <AvailableRooms />
          </>
        )
      }
    </>
  );

}

export default App
