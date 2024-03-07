document.addEventListener('DOMContentLoaded', function () {
    const token = localStorage.getItem('token');
    if (!token) {
        console.log('No token found. User must log in.');
        window.location.href = 'index.html';
        return;
    }
    const updateStatusButton = document.getElementById('updateStatusButton');
    updateStatusButton.addEventListener('click', async function () {
        const token = localStorage.getItem('token');
        if (!token) {
            alert('Please log in to update your status.');
            return;
        }

        // Assuming you can retrieve the current user's ID from the token or another source
        const userId = await getCurrentUserId(token);

        fetch(`http://localhost:7071/api/User/Update/${userId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({
                id: userId, role: "1"
            })
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to update user status');
                }
                alert('User status updated successfully');
                localStorage.removeItem('token');
                window.location.reload();
            })
            .catch(error => {
                console.error('Error updating status:', error);
                alert('Error updating user status');
            });
    });


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

    async function getCurrentUserId(token) {
        email = await getCurrentUserEmail(token);
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