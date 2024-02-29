document.addEventListener('DOMContentLoaded', function() {
    const bookHotelButton = document.getElementById('bookHotelButton');

    bookHotelButton.addEventListener('click', function() {
        const dateFrom = document.getElementById('dateFrom').value;
        const dateTo = document.getElementById('dateTo').value;
        const numberOfRooms = document.getElementById('numberOfRooms').value;

        // Vérification que tous les champs sont remplis
        if (!dateFrom || !dateTo || !numberOfRooms) {
            alert('Please fill in all fields.');
            return; // Arrête l'exécution si un champ est vide
        }

        const userId = 42; // Valeur fixe pour l'instant
        const hotelId = 42; // Valeur fixe pour l'instant

        if (new Date(dateTo) <= new Date(dateFrom)) {
            alert("The check-out date must be later than the check-in date.");
            return; // Stoppe l'exécution de la fonction si la condition est vraie
        }

        const reservation = { userId, hotelId, dateFrom, dateTo, numberOfRooms };

        saveReservation(reservation);
        // Envoi de la réservation à l'API
        // sendReservationToAPI(reservation);
    });

    function sendReservationToAPI(reservation) {
        fetch('URL_DE_VOTRE_API', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(reservation),
        })
        .then(response => response.json())
        .then(data => {
            console.log('Success:', data);
            alert('Reservation saved successfully!');
        })
        .catch((error) => {
            console.error('Error:', error);
            alert('An error occurred while saving the reservation.');
        });
    }

    function saveReservation(reservation) {
        const reservations = JSON.parse(localStorage.getItem('reservations')) || [];
        reservations.push(reservation);
        localStorage.setItem('reservations', JSON.stringify(reservations));
        alert('Reservation saved successfully!');
    }
});
