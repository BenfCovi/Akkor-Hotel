document.addEventListener('DOMContentLoaded', function() {
    const changeEmailButton = document.getElementById('changeEmailButton');
    const changePasswordButton = document.getElementById('changePasswordButton');
    const deleteAccountButton = document.getElementById('deleteAccountButton');

    tryToken();

    changeEmailButton.addEventListener('click', function() {
        const email = document.getElementById('emailInput').value;
        updateUser(email, null);
    });

    changePasswordButton.addEventListener('click', function() {
        const password = document.getElementById('passwordInput').value;
        updateUser(null, password);
    });

    deleteAccountButton.addEventListener('click', function() {
        deleteUser();
    });

    async function updateUser(email, password) {
        const token = localStorage.getItem('token');
        if (!token) {
            alert('Please log in to update your profile.');
            window.location.href = 'login.html';
            return;
        }

        // Ajoutez la logique pour envoyer la requête de mise à jour à l'API
        // Utilisez l'email et/ou le mot de passe fournis si non nulls

        // Après mise à jour, déconnectez et reconnectez l'utilisateur pour rafraîchir le token
        alert('Your profile has been updated. Please log in again.');
        localStorage.removeItem('token');
        window.location.href = 'login.html'; // Redirigez vers la page de connexion
    }

    async function deleteUser() {
        const token = localStorage.getItem('token');
        if (!token) {
            alert('Please log in to delete your account.');
            window.location.href = 'login.html';
            return;
        }

        // Ajoutez la logique pour envoyer la requête de suppression à l'API

        alert('Your account has been deleted.');
        localStorage.removeItem('token');
        window.location.href = 'index.html'; // Redirigez vers la page d'accueil
    }

    async function tryToken() {
        const token = localStorage.getItem('token');
        if (!token) {
            alert('Please log in to update your profile.');
            window.location.href = 'login.html';
            return;
        }
    }
});
