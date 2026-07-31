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

        const handleMouseDown = (event) => {
            isDrawing.current = true;
            const { x, y } = getCanvasCoords(event, canvas);
            ctx.beginPath();
            ctx.moveTo(x, y);
        }

        const handleMouseMove = (event) => {
            if (isDrawing.current) {
                const { x, y } = getCanvasCoords(event, canvas);
                ctx.lineTo(x, y);
                ctx.stroke();
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

        canvas.addEventListener("mousedown", handleMouseDown);
        canvas.addEventListener("mousemove", handleMouseMove);
        canvas.addEventListener("mouseup", handleMouseUp);
        canvas.addEventListener("mouseleave", handleMouseLeave);

        return () => {
            canvas.removeEventListener("mousedown", handleMouseDown);
            canvas.removeEventListener("mousemove", handleMouseMove);
            canvas.removeEventListener("mouseup", handleMouseUp);
            canvas.removeEventListener("mouseleave", handleMouseLeave);
        }
    }, [])



    return <canvas
        ref={canvasRef}
        width={props.width}
        height={props.height}
    />
}
export default Canvas;