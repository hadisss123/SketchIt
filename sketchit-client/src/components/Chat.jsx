import { useState, useRef, useEffect } from 'react';

function Chat({ connectionRef, roomCode, playerDrawing, chat, startDrawing }) {
    const [guessText, setGuessText] = useState("");
    const messagesEndRef = useRef(null);

    function handleSendGuess() {
        if (guessText.trim() === "") return;
        connectionRef.current.invoke("GuessWord", roomCode, guessText.trim());
        setGuessText("");
    }

    function handleKeyDown(event) {
        if (event.key === "Enter") {
            handleSendGuess();
        }
    }

    useEffect(() => {
        messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
    }, [chat]);

    return (
        <div style={{ display: "flex", flexDirection: "column", height: "400px", width: "300px", border: "1px solid black" }}>
            <div style={{ flex: 1, overflowY: "auto", padding: "8px" }}>
                {chat.map((msg, i) => (
                    <div key={i} style={{ color: msg.type === "UserMessage" ? msg.correctGuess ? "green" : "white" : "red" }}>
                        {msg.type === "UserMessage" ? msg.correctGuess ? `${msg.player} ${msg.guess}` : `${msg.player}: ${msg.guess}` : `${msg.message}`}
                    </div>
                ))}
                <div ref={messagesEndRef} />
            </div>
            {startDrawing && !playerDrawing && <div style={{ display: "flex", borderTop: "1px solid black" }}>
                <input
                    value={guessText}
                    onChange={(event) => setGuessText(event.target.value)}
                    onKeyDown={handleKeyDown}
                    placeholder="Type your guess..."
                    style={{ flex: 1 }}
                />
                <button onClick={handleSendGuess}>Send</button>
            </div>}
        </div>
    );
}

export default Chat;