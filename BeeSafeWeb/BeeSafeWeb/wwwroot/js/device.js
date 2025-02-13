let map;
let marker;
let fovLayer;
let selectedDeviceId = null;
let initialPoint = null;
const fovAngle = 40;
const fovDistance = 0.0005;

function openModal(deviceId) {
    console.log(deviceId);
    selectedDeviceId = deviceId;
    document.getElementById('location-modal').classList.remove('hidden');

    const latitude = parseFloat(document.getElementById(`latitude-${deviceId}`).value);
    const longitude = parseFloat(document.getElementById(`longitude-${deviceId}`).value);

    if (!map) {
        map = L.map('map').setView([51.168300, 4.980980], 14);

        L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
        }).addTo(map);

        map.on('click', function (e) {
            if (!initialPoint) {
                // First click: Set location
                if (marker) {
                    map.removeLayer(marker);
                }
                marker = L.marker(e.latlng).addTo(map);
                initialPoint = e.latlng;

                document.getElementById(`latitude-${selectedDeviceId}`).value = e.latlng.lat.toFixed(6);
                document.getElementById(`longitude-${selectedDeviceId}`).value = e.latlng.lng.toFixed(6);

                map.on('mousemove', drawCameraFOV);
            } else {
                // Second click: Confirm direction and FOV
                const direction = calculateDirection(initialPoint, e.latlng);
                document.getElementById(`direction-${selectedDeviceId}`).value = direction.toFixed(2);

                if (fovLayer) {
                    map.removeLayer(fovLayer);
                }

                map.off('mousemove', drawCameraFOV);
                initialPoint = null;

                Swal.fire({
                    position: "top-end",
                    icon: 'success',
                    title: 'Location and Direction Confirmed',
                    text: `Direction: ${direction.toFixed(2)} degrees`,
                    toast: true,
                    showConfirmButton: false,
                    timer: 2500,
                    background: '#28a745',
                });
            }
        });
    }

    if (!isNaN(latitude) && !isNaN(longitude) && latitude !== 0 && longitude !== 0) {
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
    const x = Math.cos(lat1) * Math.sin(lat2) -
        Math.sin(lat1) * Math.cos(lat2) * Math.cos(dLon);

    let bearing = Math.atan2(y, x) * (180 / Math.PI);
    bearing = (bearing + 360) % 360;  // Normalize the bearing

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
                Swal.fire(
                    {
                        position: "top-end",
                        icon: 'warning',
                        title: 'City Not Found',
                        text: 'Please enter a valid city in Belgium.',
                        toast: true,
                        timer: 2500,
                        showConfirmButton: false,
                        background: '#fd7e14',
                    }
                );
            }
        })
        .catch(error => Swal.fire(
            {
                position: "top-end",
                icon: 'error',
                title: 'Error',
                text: 'An error occurred while fetching city data. Please try again.',
                toast: true,
                timer: 2500,
                showConfirmButton: false,
                background: '#dc3545',
            }
        ));
}

function handleKeyPress(event) {
    if (event.key === "Enter") {
        searchCity();
    }
}

function confirmLocation() {
    const name = document.getElementById(`name-${selectedDeviceId}`).value;
    const lat = parseFloat(document.getElementById(`latitude-${selectedDeviceId}`).value);
    const lng = parseFloat(document.getElementById(`longitude-${selectedDeviceId}`).value);
    const direction = parseFloat(document.getElementById(`direction-${selectedDeviceId}`).value);

    if (!isNaN(lat) && !isNaN(lng) && !isNaN(direction)) {
        document.getElementById(`form-name-${selectedDeviceId}`).value = name;
        document.getElementById(`form-latitude-${selectedDeviceId}`).value = lat.toFixed(6);
        document.getElementById(`form-longitude-${selectedDeviceId}`).value = lng.toFixed(6);
        document.getElementById(`form-direction-${selectedDeviceId}`).value = direction.toFixed(2);

        document.getElementById(`approval-form-${selectedDeviceId}`).classList.remove('hidden');
        closeModal();
    } else {
        Swal.fire({
            position: "top-end",
            icon: 'warning',
            title: 'Incomplete Data',
            text: 'Please set both location and direction.',
            toast: true,
            timer: 2500,
            showConfirmButton: false,
            background: '#fd7e14',
        });
    }
}

function closeModal() {
    document.getElementById('location-modal').classList.add('hidden');
    if (marker) {
        map.removeLayer(marker);
        marker = null;
    }
    if (fovLayer) {
        map.removeLayer(fovLayer);
        fovLayer = null;
    }
    initialPoint = null;
}

function rejectDevice(event) {
    console.log("Is this bloody function even called?")
    event.preventDefault();
    Swal.fire({
        title: 'Are you sure?',
        text: 'Do you really want to decline this device?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Yes, decline it!'
    }).then(result => {
        if (result.isConfirmed) {
            event.target.submit();
        }
    });
}