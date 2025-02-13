let map;
let marker;
let fovLayer;
let initialPoint = null;
let selectedDeviceId = null;

// Custom Icon for the Device Marker
var cameraIcon = L.icon({
    iconUrl: '/assets/photo-camera.svg', 
    iconSize: [30, 30],                 
    iconAnchor: [20, 40],               
    popupAnchor: [0, -35]                
});


// Adjusts the marker's center to account for the icon's offset so that the FOV polygon 
// is drawn relative to the visual center of the icon.
function getAdjustedCenter(latlng) {
    const containerPoint = map.latLngToContainerPoint(latlng);
    const adjustedPoint = L.point(containerPoint.x - 5, containerPoint.y - 25);
    return map.containerPointToLatLng(adjustedPoint);
}


// Called when the user clicks an "Edit" or "Add Device" button.
// This function populates the modal with device data, sets up the map view,
// creates/updates the marker with the custom icon, and draws the FOV polygon.
function openModal(btn) {
    // Retrieve device information from the button's data attributes.
    selectedDeviceId = btn.getAttribute("data-id");
    const name = btn.getAttribute("data-name") || "";
    const latStr = btn.getAttribute("data-lat") || "51.168300";
    const lngStr = btn.getAttribute("data-lng") || "4.980980";
    const directionStr = btn.getAttribute("data-dir") || "";

    // Populate modal form fields.
    document.getElementById('modal-id').value = selectedDeviceId;
    document.getElementById('modal-name').value = name;

    // Parse latitude, longitude, and direction strings (replace commas with dots) into numbers.
    let latitude = parseFloat(latStr.replace(',', '.')) || 51.168300;
    let longitude = parseFloat(lngStr.replace(',', '.')) || 4.980980;
    let direction = parseFloat(directionStr.replace(',', '.'));

    // Set the number input fields with the parsed numeric values.
    document.getElementById('modal-latitude').value = latitude;
    document.getElementById('modal-longitude').value = longitude;
    // For direction, if parsing fails (NaN), leave the input empty.
    document.getElementById('modal-direction').value = isNaN(direction) ? "" : direction;

    // Hide any previous error messages.
    document.getElementById('error-name').classList.add('hidden');
    document.getElementById('error-latitude').classList.add('hidden');
    document.getElementById('error-longitude').classList.add('hidden');
    document.getElementById('error-direction').classList.add('hidden');

    // Display the modal.
    document.getElementById('location-modal').classList.remove('hidden');

    // Wait a short time to ensure the modal is visible before updating the map.
    setTimeout(() => {
        if (map) {
            // If the map already exists, recalculate its size and update the view.
            map.invalidateSize();
            map.setView([latitude, longitude], 14);
            // Remove any existing marker.
            if (marker) {
                map.removeLayer(marker);
            }
            // Create a new marker with the custom camera icon.
            marker = L.marker([latitude, longitude], { icon: cameraIcon }).addTo(map);
        } else {
            // If the map isn't initialized, create it and set its view.
            map = L.map('map').setView([latitude, longitude], 14);
            L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
                maxZoom: 19,
                attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
            }).addTo(map);
            // Set up a click event on the map for setting location/direction.
            map.on('click', function(e) {
                if (!initialPoint) {
                    // First click: set the new location.
                    if (marker) {
                        map.removeLayer(marker);
                    }
                    marker = L.marker(e.latlng, { icon: cameraIcon }).addTo(map);
                    initialPoint = e.latlng;
                    document.getElementById('modal-latitude').value = e.latlng.lat.toFixed(6);
                    document.getElementById('modal-longitude').value = e.latlng.lng.toFixed(6);
                    // Start drawing the FOV polygon dynamically.
                    map.on('mousemove', drawCameraFOV);
                } else {
                    // Second click: determine the direction based on the movement.
                    const newDirection = calculateDirection(initialPoint, e.latlng);
                    document.getElementById('modal-direction').value = newDirection.toFixed(2);
                    // Stop drawing the FOV polygon.
                    map.off('mousemove', drawCameraFOV);
                    initialPoint = null;
                }
            });
            // Create the initial marker.
            marker = L.marker([latitude, longitude], { icon: cameraIcon }).addTo(map);
        }
        // If a valid direction is provided, create and add the FOV polygon.
        if (!isNaN(direction)) {
            if (fovLayer) {
                map.removeLayer(fovLayer);
            }
            // Adjust the marker's center so the FOV polygon aligns with the icon's visual center.
            const adjustedCenter = getAdjustedCenter(marker.getLatLng());
            fovLayer = createFOVPolygon(adjustedCenter, direction);
            fovLayer.addTo(map);
        }
    }, 100);
}

// Called on mousemove (after the first click) to update the FOV polygon dynamically.
function drawCameraFOV(e) {
    let latlng;
    if (e.latlng) {
        latlng = e.latlng;
    } else if (e.touches && e.touches.length > 0) {
        const touch = e.touches[0];
        latlng = map.containerPointToLatLng([touch.clientX, touch.clientY]);
    }
    if (initialPoint && latlng) {
        // Remove the existing FOV polygon if present.
        if (fovLayer) {
            map.removeLayer(fovLayer);
        }
        // Calculate the new direction from the initial click point to the current point.
        const newDirection = calculateDirection(initialPoint, latlng);
        // Use the adjusted center of the initial point for the FOV polygon.
        const adjustedCenter = getAdjustedCenter(initialPoint);
        fovLayer = createFOVPolygon(adjustedCenter, newDirection);
        fovLayer.addTo(map);
    }
}

// Generates a polygon representing the device's Field-Of-View based on a center point and a direction.
function createFOVPolygon(center, direction) {
    const fovAngle = 40;         
    const fovDistance = 0.0005;    
    const startAngle = direction - fovAngle / 2;
    const endAngle = direction + fovAngle / 2;
    const points = [center];
    // Loop from the start to the end angle in increments (here, 5° increments).
    for (let angle = startAngle; angle <= endAngle; angle += 5) {
        const rad = angle * (Math.PI / 180);
        // Calculate offsets based on cosine and sine.
        const latOffset = Math.cos(rad) * fovDistance;
        const lngOffset = Math.sin(rad) * fovDistance;
        points.push([center.lat + latOffset, center.lng + lngOffset]);
    }
    // Close the polygon by adding the center again.
    points.push(center);
    return L.polygon(points, {
        color: 'red',
        fillColor: 'rgba(255, 0, 0, 0.5)',
        fillOpacity: 0.3
    });
}


// Computes the bearing (in degrees) from point1 to point2 using trigonometry.
function calculateDirection(point1, point2) {
    const lat1 = point1.lat * (Math.PI / 180);
    const lon1 = point1.lng * (Math.PI / 180);
    const lat2 = point2.lat * (Math.PI / 180);
    const lon2 = point2.lng * (Math.PI / 180);
    const dLon = lon2 - lon1;
    const y = Math.sin(dLon) * Math.cos(lat2);
    const x = Math.cos(lat1) * Math.sin(lat2) - Math.sin(lat1) * Math.cos(lat2) * Math.cos(dLon);
    let bearing = Math.atan2(y, x) * (180 / Math.PI);
    // Normalize the bearing to a value between 0 and 360.
    return (bearing + 360) % 360;
}


// Uses the Nominatim API to search for a city by name and re-centers the map if found.
function searchCity() {
    const cityName = document.getElementById('city-search').value;
    if (!cityName) {
        Swal.fire({
            position: "top-end",
            icon: 'warning',
            title: 'Oops...',
            toast: true,
            text: 'Please enter a city name!',
            showConfirmButton: false,
            timer: 2500,
            background: '#FFA500'
        });
        return;
    }
    fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${cityName}&countrycodes=BE`)
        .then(response => response.json())
        .then(data => {
            if (data.length > 0) {
                const lat = parseFloat(data[0].lat);
                const lon = parseFloat(data[0].lon);
                map.setView([lat, lon], 14);
            } else {
                Swal.fire({
                    position: "top-end",
                    icon: 'warning',
                    title: 'City Not Found',
                    text: 'Please enter a valid city in Belgium.',
                    toast: true,
                    timer: 2500,
                    showConfirmButton: false,
                    background: '#fd7e14'
                });
            }
        })
        .catch(error => {
            Swal.fire({
                position: "top-end",
                icon: 'error',
                title: 'Error',
                text: 'An error occurred while fetching city data. Please try again.',
                toast: true,
                timer: 2500,
                showConfirmButton: false,
                background: '#dc3545'
            });
        });
}


// If the user presses the Enter key while in the city search input, trigger a search.
function handleKeyPress(event) {
    if (event.key === "Enter") {
        searchCity();
    }
}

// Closes the modal and cleans up temporary state like the marker and initialPoint.
// 
function closeModal() {
    document.getElementById('location-modal').classList.add('hidden');
    if (marker) {
        map.removeLayer(marker);
        marker = null;
    }
    initialPoint = null;
}

// Validates that all required input fields in the modal have values.
// Displays error messages if any fields are empty.
// Returns true if the form is valid, otherwise false.
function validateModalForm() {
    let valid = true;

    // Validate Name.
    const nameField = document.getElementById("modal-name");
    const nameError = document.getElementById("error-name");
    if (nameField.value.trim() === "") {
        nameError.classList.remove("hidden");
        valid = false;
    } else {
        nameError.classList.add("hidden");
    }

    // Validate Latitude.
    const latitudeField = document.getElementById("modal-latitude");
    const latError = document.getElementById("error-latitude");
    if (latitudeField.value.trim() === "") {
        latError.classList.remove("hidden");
        valid = false;
    } else {
        latError.classList.add("hidden");
    }

    // Validate Longitude.
    const longitudeField = document.getElementById("modal-longitude");
    const lngError = document.getElementById("error-longitude");
    if (longitudeField.value.trim() === "") {
        lngError.classList.remove("hidden");
        valid = false;
    } else {
        lngError.classList.add("hidden");
    }

    // Validate Direction.
    const directionField = document.getElementById("modal-direction");
    const directionError = document.getElementById("error-direction");
    if (directionField.value.trim() === "") {
        directionError.classList.remove("hidden");
        valid = false;
    } else {
        directionError.classList.add("hidden");
    }

    return valid;
}
