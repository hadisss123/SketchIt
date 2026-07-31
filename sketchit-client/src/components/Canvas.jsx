import { useRef, useEffect } from 'react';

function Canvas(props) {
    const canvasRef = useRef(null);
    function getCanvasCoords(event, canvas) {
        const rect = canvas.getBoundingClientRect();
        return {
            x: event.clientX - rect.left,
            y: event.clientY - rect.top,
        };
    }

    const isDrawing = useRef(false);
    useEffect(() => {
        const canvas = canvasRef.current;
        const ctx = canvas.getContext("2d");
        ctx.lineWidth = 5;
        ctx.lineCap = "round";
        ctx.lineJoin = "round";
        ctx.fillStyle = "white";
        ctx.fillRect(0, 0, canvas.width, canvas.height);
        var oldX = 0;
        var oldY = 0;

        const handleReceiveEntireDrawing = (points) => {
            ctx.beginPath();
            for (let i = 0; i < points.length; i += 2) {
                const start = points[i];
                const end = points[i + 1];

                if (!end) break;

                ctx.moveTo(start.x, start.y);
                ctx.lineTo(end.x, end.y);
            }
            ctx.stroke();
        }

        const handleMouseDown = (event) => {
            isDrawing.current = true;
            const { x, y } = getCanvasCoords(event, canvas);
            ctx.beginPath();
            ctx.moveTo(x, y);
            oldX = x;
            oldY = y;
        }

        const handleMouseMove = (event) => {
            if (isDrawing.current) {
                const { x, y } = getCanvasCoords(event, canvas);
                ctx.lineTo(x, y);
                ctx.stroke();

                if (props.connectionRef.current) {
                    props.connectionRef.current.invoke("SendDrawSegment", props.roomCode, oldX, oldY, x, y);
                }

                oldX = x;
                oldY = y;
            }
        }

        const handleMouseUp = () => {
            isDrawing.current = false;
            ctx.beginPath();
        }

        const handleMouseLeave = () => {
            isDrawing.current = false;
            ctx.beginPath();
        }
        const handleReceiveDrawing = (x1, y1, x2, y2) => {
            ctx.beginPath();
            ctx.moveTo(x1, y1);
            ctx.lineTo(x2, y2);
            ctx.stroke();
        }

        canvas.addEventListener("mousedown", handleMouseDown);
        canvas.addEventListener("mousemove", handleMouseMove);
        canvas.addEventListener("mouseup", handleMouseUp);
        canvas.addEventListener("mouseleave", handleMouseLeave);
        if (props.connectionRef.current) {
            props.connectionRef.current.on("ReceiveDrawing", handleReceiveDrawing);
            props.connectionRef.current.on("ReceiveEntireDrawing", handleReceiveEntireDrawing);

            props.connectionRef.current.invoke("GetEntireDrawing", props.roomCode);
        }

        return () => {
            canvas.removeEventListener("mousedown", handleMouseDown);
            canvas.removeEventListener("mousemove", handleMouseMove);
            canvas.removeEventListener("mouseup", handleMouseUp);
            canvas.removeEventListener("mouseleave", handleMouseLeave);
            if (props.connectionRef.current) {
                props.connectionRef.current.off("ReceiveDrawing", handleReceiveDrawing);
                props.connectionRef.current.off("ReceiveEntireDrawing", handleReceiveEntireDrawing);
            }
        }
    }, [props.connectionRef, props.roomCode, props.isConnected])



    return <canvas
        ref={canvasRef}
        width={props.width}
        height={props.height}
    />
}
export default Canvas;