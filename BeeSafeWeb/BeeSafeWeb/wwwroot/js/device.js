let map;
let marker;
let fovLayer;
let initialPoint = null;
let selectedDeviceId = null;

function openModal(btn) {
    // Read device info from the button's data attributes
    selectedDeviceId = btn.getAttribute("data-id");
    const name = btn.getAttribute("data-name") || "";
    const latStr = btn.getAttribute("data-lat") || "51.168300";
    const lngStr = btn.getAttribute("data-lng") || "4.980980";
    const directionStr = btn.getAttribute("data-dir") || "";

    // Populate modal form fields
    document.getElementById('modal-id').value = selectedDeviceId;
    document.getElementById('modal-name').value = name;
    document.getElementById('modal-latitude').value = latStr;
    document.getElementById('modal-longitude').value = lngStr;
    document.getElementById('modal-direction').value = directionStr;

    // Hide any previous error messages
    document.getElementById('error-name').classList.add('hidden');
    document.getElementById('error-latitude').classList.add('hidden');
    document.getElementById('error-longitude').classList.add('hidden');
    document.getElementById('error-direction').classList.add('hidden');

    // Show modal
    document.getElementById('location-modal').classList.remove('hidden');

    // Parse coordinates (ensure dot as decimal separator)
    let latitude = parseFloat(latStr.replace(',', '.')) || 51.168300;
    let longitude = parseFloat(lngStr.replace(',', '.')) || 4.980980;

    // Initialize or update the map
    if (!map) {
        map = L.map('map').setView([latitude, longitude], 14);
        L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
        }).addTo(map);

        map.on('click', function(e) {
            if (!initialPoint) {
                // First click: set location & update latitude and longitude fields
                if (marker) {
                    map.removeLayer(marker);
                }
                marker = L.marker(e.latlng).addTo(map);
                initialPoint = e.latlng;
                document.getElementById('modal-latitude').value = e.latlng.lat.toFixed(6);
                document.getElementById('modal-longitude').value = e.latlng.lng.toFixed(6);
                map.on('mousemove', drawCameraFOV);
            } else {
                // Second click: set direction and update the direction field (keep the FOV visible)
                const direction = calculateDirection(initialPoint, e.latlng);
                document.getElementById('modal-direction').value = direction.toFixed(2);
                map.off('mousemove', drawCameraFOV);
                initialPoint = null;
            }
        });
    } else {
        map.setView([latitude, longitude], 14);
        if (marker) {
            map.removeLayer(marker);
        }
        marker = L.marker([latitude, longitude]).addTo(map);
    }
}

function drawCameraFOV(e) {
    let latlng;
    if (e.latlng) {
        latlng = e.latlng;
    } else if (e.touches && e.touches.length > 0) {
        const touch = e.touches[0];
        latlng = map.containerPointToLatLng([touch.clientX, touch.clientY]);
    }
    if (initialPoint && latlng) {
        if (fovLayer) {
            map.removeLayer(fovLayer);
        }
        const direction = calculateDirection(initialPoint, latlng);
        fovLayer = createFOVPolygon(initialPoint, direction);
        fovLayer.addTo(map);
    }
}

function createFOVPolygon(center, direction) {
    const fovAngle = 40;
    const fovDistance = 0.0005;
    const startAngle = direction - fovAngle / 2;
    const endAngle = direction + fovAngle / 2;
    const points = [center];
    for (let angle = startAngle; angle <= endAngle; angle += 5) {
        const rad = angle * (Math.PI / 180);
        const latOffset = Math.cos(rad) * fovDistance;
        const lngOffset = Math.sin(rad) * fovDistance;
        points.push([
            center.lat + latOffset,
            center.lng + lngOffset
        ]);
    }
    points.push(center);
    return L.polygon(points, {
        color: 'red',
        fillColor: 'rgba(255, 0, 0, 0.5)',
        fillOpacity: 0.3
    });
}

function calculateDirection(point1, point2) {
    const lat1 = point1.lat * (Math.PI / 180);
    const lon1 = point1.lng * (Math.PI / 180);
    const lat2 = point2.lat * (Math.PI / 180);
    const lon2 = point2.lng * (Math.PI / 180);
    const dLon = lon2 - lon1;
    const y = Math.sin(dLon) * Math.cos(lat2);
    const x = Math.cos(lat1) * Math.sin(lat2) - Math.sin(lat1) * Math.cos(lat2) * Math.cos(dLon);
    let bearing = Math.atan2(y, x) * (180 / Math.PI);
    bearing = (bearing + 360) % 360;
    return bearing;
}

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

function handleKeyPress(event) {
    if (event.key === "Enter") {
        searchCity();
    }
}

function closeModal() {
    document.getElementById('location-modal').classList.add('hidden');
    if (marker) {
        map.removeLayer(marker);
        marker = null;
    }
    // Optionally, remove the FOV layer if desired:
    // if (fovLayer) {
    //     map.removeLayer(fovLayer);
    //     fovLayer = null;
    // }
    initialPoint = null;
}

// Client-side form validation
function validateModalForm() {
    let valid = true;

    const nameField = document.getElementById("modal-name");
    const nameError = document.getElementById("error-name");
    if (nameField.value.trim() === "") {
        nameError.classList.remove("hidden");
        valid = false;
    } else {
        nameError.classList.add("hidden");
    }

    const latitudeField = document.getElementById("modal-latitude");
    const latError = document.getElementById("error-latitude");
    if (latitudeField.value.trim() === "") {
        latError.classList.remove("hidden");
        valid = false;
    } else {
        latError.classList.add("hidden");
    }

    const longitudeField = document.getElementById("modal-longitude");
    const lngError = document.getElementById("error-longitude");
    if (longitudeField.value.trim() === "") {
        lngError.classList.remove("hidden");
        valid = false;
    } else {
        lngError.classList.add("hidden");
    }

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
