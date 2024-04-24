let map; // Global map object
let imageOverlay; // Image overlay object
var mapScale = 4.2;
var mapX = -169.5;
var mapY = -1040.2;
var bounds;

function initializeMap() {
    const imagePath = '/CustomsSat.jpg'; // Path to your image
    const imageWidth = 16384; // Width of your image in pixels
    const imageHeight = 16384; // Height of your image in pixels

    // Create map object
    map = L.map('tarkovmap', {
        minZoom: -2,
        maxZoom: 6, // Set max zoom to a reasonable level for your image
        center: [0, 0],
        zoom: 1,
        crs: L.CRS.Simple
    });

    // Define bounds based on the dimensions of the image
    const southWest = map.unproject([0, imageHeight], map.getMaxZoom());
    const northEast = map.unproject([imageWidth, 0], map.getMaxZoom());
    bounds = new L.LatLngBounds(southWest, northEast);

    // Set the image as an overlay
    imageOverlay = L.imageOverlay(imagePath, bounds).addTo(map);

    // Set map bounds and max bounds to prevent panning outside the image
    map.setMaxBounds(bounds);
    map.fitBounds(bounds);

}

function updateMap(x, y, scale) {
    mapX = x;
    mapY = y;
    mapScale = scale;

    if (window.playerMarker) {
        var newPlayerPosition = toLeafletCoords(mapX, mapY);  // Recalculate with the current player's in-game coords
        window.playerMarker.setLatLng(newPlayerPosition);
    }
}

function toLeafletCoords(inGameX, inGameY) {
    var mapWidth = bounds.getEast() - bounds.getWest();
    var mapHeight = bounds.getNorth() - bounds.getSouth();

    var scaledX = (inGameX + mapX) / mapScale;
    var scaledY = (inGameY + mapY) / mapScale;

    var x = bounds.getWest() + (mapWidth / 2) + scaledX;
    var y = bounds.getNorth() + (mapHeight / 2) + scaledY;

    return L.latLng(y, x);
}

function updateLocalPlayerMarker(x, y) {
    var latLng = toLeafletCoords(x, y);

    if (window.localPlayerMarker) {
        window.localPlayerMarker.setLatLng(latLng);
    } else {
        window.localPlayerMarker = L.circleMarker(latLng, {
            radius: 1.5,
            fillColor: 'green',
            color: 'green',
            weight: 2,
            opacity: 1,
            fillOpacity: 0.8
        }).addTo(map);
        window.localPlayerMarker.bindPopup("Local Player");
    }
}

// Maintain a map of markers by unique identifier
var markers = {};

function drawMarkerAt(inGameX, inGameY, name, base, dist) {
    var latLng = toLeafletCoords(inGameX, inGameY);
    var formattedDistance = dist.toFixed(2); // Formatting the distance to two decimal places

    if (markers[base]) {
        // Update the existing marker's position, color, and popup
        markers[base].setLatLng(latLng);
        markers[base].bindPopup(name + " - Distance: " + formattedDistance);
    } else {
        // Create a new circle marker with distance in popup if it does not exist
        var marker = L.circleMarker(latLng, {
            radius: 1.5,
            fillColor: 'yellow',
            color: 'yellow',
            weight: 2,
            opacity: 1,
            fillOpacity: 0.8
        }).addTo(map);
        marker.bindPopup(name + " - Distance: " + formattedDistance);
        markers[base] = marker;  // Store the marker in the map using 'base' as the key
    }
}
