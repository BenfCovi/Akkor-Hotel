async function updateAdminInterface() {
    const token = localStorage.getItem('token');
    if (!token) {
      console.log('No token found, redirecting to login');
      window.location.href = 'index.html';
      return;
    }
  
    try {
      const userEmail = await getCurrentUserEmail(token);
  
      const isAdmin = await getCurrentUser(userEmail, token).role;
      if (isAdmin!=2) {
        console.log('You dont have right to be here, redirecting to login');
      window.location.href = 'index.html';
      }
    } catch (error) {
      console.error('Error updating UI:', error);
    }
  }


  async function getCurrentUser(email, token) {
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
        return data;

    } catch (error) {
        console.error('Error fetching user profile:', error);
    }
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
  
  document.addEventListener('DOMContentLoaded', updateAdminInterface);