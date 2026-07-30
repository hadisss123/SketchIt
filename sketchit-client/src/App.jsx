import { useEffect, useState } from 'react'
import './App.css'
import CreateRoom from './components/CreateRoom'
import JoinRoom from './components/JoinRoom'
import AvailableRooms from './components/AvailableRooms'
import Room from './components/Room'

function App() {
  const [joinedRoom, setJoinedRoom] = useState(null);

  return (
    <>
      {
        joinedRoom ? (
          <Room roomCode={joinedRoom.roomCode} displayName={joinedRoom.displayName} />
        ) : (
          <>
            <JoinRoom onJoinSuccess={(roomCode, displayName) => setJoinedRoom({ roomCode, displayName })} />
            <CreateRoom />
            <AvailableRooms />
          </>
        )
      }
    </>
  );

}

export default App
