document.addEventListener('DOMContentLoaded', function() {
    const bookHotelButton = document.getElementById('bookHotelButton');

    bookHotelButton.addEventListener('click', function() {
        const dateFrom = document.getElementById('dateFrom').value;
        const dateTo = document.getElementById('dateTo').value;
        const numberOfRooms = document.getElementById('numberOfRooms').value;
        const hotelId = document.getElementById('bookHotelButton').dataset.hotelId;

        // Vérification que tous les champs sont remplis
        if (!dateFrom || !dateTo || !numberOfRooms) {
            alert('Please fill in all fields.');
            return; // Arrête l'exécution si un champ est vide
        }

        const userId = 42; // Valeur fixe pour l'instant

        if (new Date(dateTo) <= new Date(dateFrom)) {
            alert("The check-out date must be later than the check-in date.");
            return; // Stoppe l'exécution de la fonction si la condition est vraie
        }

        const reservation = { userId, hotelId, dateFrom, dateTo, numberOfRooms };

        sendReservationToAPI(reservation);
    });

    function sendReservationToAPI() {
        // Récupération des informations de réservation ici...
        
        // Vérification de la présence du token
        const token = localStorage.getItem('token');
        if (!token) {
            alert('Please log in to make a reservation.');
            return;
        }
    
        // Construire l'objet de réservation en fonction de la structure attendue par l'API
        const reservation = {
            // ...
            // Assurez-vous de remplir les champs nécessaires selon la structure de l'API
        };
    
        fetch('http://localhost:7071/api/CreateReservation', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}` // Ajoute l'en-tête Authorization avec le token
            },
            body: JSON.stringify(reservation),
        })
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data => {
            console.log('Success:', data);
            alert('Reservation saved successfully!');
            // Vous pouvez recharger la page ou mettre à jour l'interface utilisateur ici si nécessaire
        })
        .catch((error) => {
            console.error('Error:', error);
            alert('An error occurred while saving the reservation.');
        });
    }

    function fetchReservations() {
        const noReservationCard = document.getElementById('noReservationCard');
        if (!noReservationCard) return; // S'arrête si noReservationCard n'existe pas

        const token = localStorage.getItem('token');
        if (!token) {
            alert('Please log in to view your reservations.');
            window.location.href = 'login.html';
            return;
        }

        fetch('http://localhost:7071/api/GetReservation', {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`
            },
        })
        .then(response => {
            if (!response.ok) {
                throw new Error('Error fetching reservations');
            }
            return response.json();
        })
        .then(reservations => {
            if (reservations.length > 0) {
                noReservationCard.style.display = 'none'; // Cache la card de "pas de réservation"
                reservations.forEach(reservation => {
                    // Appelle une fonction pour créer et afficher chaque card de réservation
                    // avec les données de `reservation`
                });
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('An error occurred while fetching reservations.');
        });
    }

    // Appel de la fonction pour chercher les réservations
    fetchReservations();
    
    function createReservationCard(reservation) {
        // Here you will create the DOM elements for the reservation card
        // and populate them with the data from the reservation object.
        // Append the new cards to the container in the HTML.
    }
    
    
});
