let img = new Image(); // Image is loaded globally if not already
let zoomLevel = 1; // Global zoom level

function initializeCanvas(imagePath, canvasId) {
    const canvas = document.getElementById(canvasId);
    const ctx = canvas.getContext('2d');

    canvas.addEventListener('wheel', function (event) {
        handleZoom(event, canvasId);
    });

    ctx.fillStyle = 'black';
    // Always update the image source
    img.onload = () => {
        setCanvasSizeAndDrawImage(canvas, ctx, zoomLevel);
    };
    if (img.src !== imagePath) {  // Check if the path is different
        img.src = imagePath;      // Update the source if different
    } else {
        img.onload(); // Call onload manually if the source hasn't changed
    }
}


function setCanvasSizeAndDrawImage(canvas, ctx, zoom) {
    canvas.width = img.width;
    canvas.height = img.height;
    updateZoomLevel(canvas.id, zoom);
}
function handleZoom(event, canvasId) {
    event.preventDefault();
    const zoomIntensity = 0.1;
    const scaleChange = Math.sign(event.deltaY) * zoomIntensity;
    zoomLevel -= scaleChange;
    zoomLevel = Math.min(Math.max(zoomLevel, 0.1), 10); // Clamps zoom level between 0.1 and 10

    updateZoomLevel(canvasId, zoomLevel);
}

function updateZoomLevel(canvasId, newZoomLevel) {
    const canvas = document.getElementById(canvasId);
    const ctx = canvas.getContext('2d');
    zoomLevel = newZoomLevel;

    let offsetX = (canvas.width / 2) - (img.width / 2 * zoomLevel);
    let offsetY = (canvas.height / 2) - (img.height / 2 * zoomLevel);

    ctx.setTransform(1, 0, 0, 1, 0, 0);
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = 'black';
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    ctx.setTransform(zoomLevel, 0, 0, zoomLevel, offsetX, offsetY);
    ctx.drawImage(img, 0, 0);
}
function updatePlayerLocation(canvasId, x, y, rotation) {
    const canvas = document.getElementById(canvasId);
    const ctx = canvas.getContext('2d');

    let playerColor = 'red';
    centerCanvasOnPlayer(canvas, ctx, x, y);
    drawMarker(ctx, x, y, playerColor, rotation);
}

function centerCanvasOnPlayer(canvas, ctx, x, y) {
    // Calculate the new offsets to center the player
    let offsetX = (canvas.width / 2) - (x * zoomLevel);
    let offsetY = (canvas.height / 2) - (y * zoomLevel);

    // Apply the transformation to center the canvas on the player
    ctx.setTransform(zoomLevel, 0, 0, zoomLevel, offsetX, offsetY);

    // Clear and redraw the background
    ctx.clearRect(-offsetX / zoomLevel, -offsetY / zoomLevel, canvas.width / zoomLevel, canvas.height / zoomLevel);
    ctx.fillStyle = 'black';
    ctx.fillRect(-offsetX / zoomLevel, -offsetY / zoomLevel, canvas.width / zoomLevel, canvas.height / zoomLevel);

    // Redraw the image
    ctx.drawImage(img, 0, 0);
}

function updatePlayerLocations(canvasId, playerDrawList) {
const canvas = document.getElementById(canvasId);
    const ctx = canvas.getContext('2d');

    // Draw all players
    for (let player of playerDrawList) {
        drawPlayer(ctx, player.x, player.y, player.r, 'blue');
    }
}
function drawPlayer(ctx, x, y, rotation, color) {
    ctx.save(); // Save the current canvas state
    ctx.beginPath();
    ctx.translate(x, y); // Move the origin to the marker position

    ctx.rotate(rotation); // Rotate the canvas if needed (in radians)

    // Draw a circle for the marker
    ctx.arc(0, 0, 10, 0, 2 * Math.PI);
    ctx.fillStyle = color;
    ctx.fill();

    // Draw lines to indicate direction
    ctx.moveTo(0, 0); // Start from the center of the circle
    ctx.lineTo(50, 0); // Draw a line extending from the center (adjust length as needed)
    ctx.strokeStyle = color; // Corrected color string
    ctx.lineWidth = 2; // Adjust line width as needed
    ctx.stroke();

    // Restore the previous canvas state
    ctx.restore();
}


function getImageSize(canvasId) {
    var canvas = document.getElementById(canvasId);
    return { width: canvas.width, height: canvas.height };
}

function drawMarker(ctx, x, y, color, rotation) {
    ctx.save(); // Save the current canvas state
    ctx.beginPath();
    ctx.translate(x, y); // Move the origin to the marker position

    ctx.rotate(rotation); // Rotate the canvas if needed (in radians)

    // Draw a circle for the marker
    ctx.arc(0, 0, 10, 0, 2 * Math.PI);
    ctx.fillStyle = color;
    ctx.fill();

    // Draw lines to indicate direction
    ctx.moveTo(0, 0); // Start from the center of the circle
    ctx.lineTo(50, 0); // Draw a line extending from the center (adjust length as needed)
    ctx.strokeStyle = color; // Corrected color string
    ctx.lineWidth = 2; // Adjust line width as needed
    ctx.stroke();

    // Restore the previous canvas state
    ctx.restore();
}
