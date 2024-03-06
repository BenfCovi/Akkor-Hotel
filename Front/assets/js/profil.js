document.addEventListener('DOMContentLoaded', function () {
    const emailInput = document.getElementById('emailInput');
    const changeEmailButton = document.getElementById('changeEmailButton');
    const changePasswordButton = document.getElementById('changePasswordButton');
    const deleteAccountButton = document.getElementById('deleteAccountButton');

    fetchUserProfile();

    changeEmailButton.addEventListener('click', function () {
        const email = emailInput.value;
        if (!email.match(/^\w+([.-]?\w+)*@\w+([.-]?\w+)*(\.\w{2,3})+$/)) {
            alert("Please enter a valid email address.");
            return;
        }
        updateUser({ email });
    });

    changePasswordButton.addEventListener('click', function () {
        const password = document.getElementById('passwordInput').value;
        if (!password) {
            alert("Password cannot be empty.");
            return;
        }
        updateUser({ password });
    });


    deleteAccountButton.addEventListener('click', function () {
        deleteUser();
    });

    async function updateUser(updateData) {
        const token = localStorage.getItem('token');
        if (!token) {
            alert('Please log in to update your profile.');
            window.location.href = 'index.html';
            return;
        }
        const userEmail = await getCurrentUserEmail(token);
        const userId = await getCurrentUserId(userEmail, token);
        updateData.id = userId;
        try {
            const response = await fetch(`http://localhost:7071/api/User/Update/${userId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(updateData)
            });

            if (!response.ok) {
                throw new Error('Failed to update profile');
            }

            alert('Your profile has been updated. Please log in again.');
            localStorage.removeItem('token');
            window.location.href = 'index.html';
        } catch (error) {
            alert('Error updating profile: ' + error.message);
        }
    }

    async function deleteUser() {
        const token = localStorage.getItem('token');
        if (!token) {
            alert('Please log in to update your profile.');
            window.location.href = 'index.html';
            return;
        }
        const userEmail = await getCurrentUserEmail(token);
        const userId = await getCurrentUserId(userEmail, token);
        try {
            const response = await fetch(`http://localhost:7071/api/User/Delete/${userId}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) {
                throw new Error('Failed to delete account');
            }

            alert('Your account has been deleted.');
            localStorage.removeItem('token');
            window.location.href = 'index.html'; // Redirect to the home page
        } catch (error) {
            alert('Error deleting account: ' + error.message);
        }
    }

    async function fetchUserProfile() {
        const token = localStorage.getItem('token');
        if (!token) {
            console.log('No token found, redirecting to login.');
            window.location.href = 'index.html';
            return;
        }
        userEmail = await getCurrentUserEmail(token);
        emailInput.value = userEmail; // Assuming the API returns an object with an email field
    }

    async function getCurrentUserEmail(token) {
        try {
            const response = await fetch(`http://localhost:7071/api/User/GetEmailFromToken/${token}`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });
            if (!response.ok) {
                throw new Error('Failed to fetch user profile');
            }
            const email = await response.text(); // Correctly awaits the text value
            return email;
            // Use the email value as needed here
        } catch (error) {
            console.error('Error fetching user profile:', error);
        }
    }

    async function getCurrentUserId(email, token) {
        console.log('Fetching user ID for email:', email);
        console.log('Token:', token);
        try {
            const response = await fetch(`http://localhost:7071/api/User/Get/Email/${encodeURIComponent(email)}`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) {
                throw new Error('Failed to fetch user profile');
            }

            const data = await response.json();
            return data.id;

        } catch (error) {
            console.error('Error fetching user profile:', error);
        }
    }
});
