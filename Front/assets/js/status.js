document.addEventListener('DOMContentLoaded', function() {
    const updateStatusButton = document.getElementById('updateStatusButton');

    updateStatusButton.addEventListener('click', function() {
        const token = localStorage.getItem('token');
        if (!token) {
            alert('Please log in to update your status.');
            return;
        }

        // Remplacez 'http://localhost:7071/api/UpdateUser' par votre endpoint API correct
        fetch('http://localhost:7071/api/UpdateUser', {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({ role: "1" }) // Mettre à jour le rôle de l'utilisateur
        })
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data => {
            console.log('Success:', data);
            alert('Your status has been updated. Please log in again.');
            localStorage.removeItem('token');
            window.location.href = 'login.html'; // Redirigez vers la page de connexion pour une nouvelle authentification
        })
        .catch((error) => {
            console.error('Error:', error);
            alert('An error occurred while updating your status.');
        });
    });
});
