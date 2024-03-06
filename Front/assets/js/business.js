document.addEventListener('DOMContentLoaded', async function () {
    const token = localStorage.getItem('token');
    if (!token) {
        console.log('No token found. User must log in.');
        return;
    }
    const userId = await getCurrentUserId(token);

    async function roleUser(id, token) {
        try {
            const userDataResponse = await fetch(`http://localhost:7071/api/User/Get/${id}`, {
                method: 'GET',
                headers: { 'Authorization': `Bearer ${token}` }
            });
            const userData = await userDataResponse.json();

            if (userData.role === "0") {
                document.getElementById('businessdata').style.display = 'none';
            } else {
                document.getElementById('businessdata').style.display = 'block';
                setData(userData.hotelId, token);
            }
        } catch (error) {
            console.error('Error fetching user data:', error);
        }
    }

    async function setData(hotelId, token) {
        try {
            const reservationsResponse = await fetch(`http://localhost:7071/api/Reservation/GetAllByHotel/${hotelId}`, {
                method: 'GET',
                headers: {'Authorization': `Bearer ${token}`}
            });
            const reservations = await reservationsResponse.json();
            document.getElementById('totalreservation').textContent = reservations.length;
            document.getElementById('totalroom').textContent = reservations.length;
            if(reservations.length > 0) {
                populateReservationSummary(reservations);
            }
        } catch (error) {
            console.error('Error populating business data:', error);
        }
    }

    async function populateReservationSummary(reservations) {
        if (reservations.length > 0) {
            const reservationSummary = reservations.reduce((acc, reservation) => {
                if (acc[reservation.email]) {
                    acc[reservation.email].totalReservations += 1;
                    acc[reservation.email].totalRooms += reservation.numberOfRoom;
                } else {
                    acc[reservation.email] = {
                        totalReservations: 1,
                        totalRooms: reservation.numberOfRoom,
                    };
                }
                return acc;
            }, {});
    
            Object.keys(reservationSummary).forEach(email => {
                const summary = reservationSummary[email];
                const reservationDiv = document.createElement('div');
                reservationDiv.classList.add('d-flex', 'flex-row', 'flex-wrap');
                reservationDiv.innerHTML = `
                    <h5 class="fw-bold">${email}</h5>
                    <h5>&nbsp;has taken&nbsp;</h5>
                    <h5 class="fw-bold">${summary.totalReservations}</h5>
                    <h5>&nbsp;reservation(s) for a total of&nbsp;</h5>
                    <h5 class="fw-bold">${summary.totalRooms}</h5>
                    <h5>&nbsp;room(s).</h5>
                `;
                document.getElementById('reservationlist').appendChild(reservationDiv);
            });
        }
    }
    

    await roleUser(userId, token);


    const createHotelBtn = document.getElementById('createHotelBtn');

    createHotelBtn.addEventListener('click', async function () {
        const token = localStorage.getItem('token');
        if (!token) {
            alert('Please log in to create your hotel.');
            return;
        }

        const hotelData = {
            name: document.getElementById('hotelName').value,
            location: document.getElementById('hotelLocation').value,
            description: document.getElementById('hotelDescription').value,
            numberOfRooms: document.getElementById('numberOfRooms').value,
            stars: document.getElementById('hotelStars').value,
            images: ["https://example.com/image1.jpg", "https://example.com/image2.jpg"],
        };

        try {
            const response = await fetch('http://localhost:7071/api/Hotel/Create', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(hotelData),
            });

            if (response.ok) {
                const result = await response.json();
                console.log('Hotel created id:', result.id);
                const userId = await getCurrentUserId(token);
                fetch(`http://localhost:7071/api/User/Update/${userId}`, {
                    method: 'PUT',
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': `Bearer ${token}`
                    },
                    body: JSON.stringify({
                        id: userId, role: "1", hotels: [result.id]
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
                alert('Hotel created successfully');
            } else {
                console.error('Failed to create hotel');
            }
        } catch (error) {
            console.error('Error:', error);
        }
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
