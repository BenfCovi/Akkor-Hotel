document.addEventListener('DOMContentLoaded', async function () {
    const token = localStorage.getItem('token');
    if (!token) {
        console.log('No token found. User must log in.');
        window.location.href = 'index.html';
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
            if (userData.role === 0) {
                document.getElementById('businessdata').innerHTML = '';
                document.getElementById('businessdata').style.display = 'None';
                document.getElementById('formeditbusiness').innerHTML = '';
                document.getElementById('formeditbusiness').style.display = 'None';
            } else {
                document.getElementById('formcreatebusiness').innerHTML = '';
                document.getElementById('formcreatebusiness').style.display = 'None';
                setData(userData.hotels[0], token);
                await updateValueForm(userId, token);
            }
        } catch (error) {
            console.error('Error fetching user data:', error);
        }
    }

    async function setData(hotelId, token) {
        try {
            const reservationsResponse = await fetch(`http://localhost:7071/api/Reservation/GetAllByHotel/${hotelId}`, {
                method: 'GET',
                headers: { 'Authorization': `Bearer ${token}` }
            });
            const reservations = await reservationsResponse.json();
            document.getElementById('totalreservation').textContent = reservations.length;
            document.getElementById('totalroom').textContent = reservations.length;
            if (reservations.length > 0) {
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

    async function updateValueForm(userId, token) {
        try {
            const userDataResponse = await fetch(`http://localhost:7071/api/User/Get/${userId}`, {
                method: 'GET',
                headers: { 'Authorization': `Bearer ${token}` }
            });
            const userData = await userDataResponse.json();
            if(userData.hotels!=null){hotelId = userData.hotels[0];}else{alert('Error fetching user data, please log in again.');}
            const hotelDataResponse = await fetch(`http://localhost:7071/api/Hotel/Get/${hotelId}`, {
            method: 'GET',
            headers: { 'Authorization': `Bearer ${token}` }
        });
        const hotelData = await hotelDataResponse.json();
        document.getElementById('formeditbusiness').id_hotel = hotelData.id;
        document.getElementById('hotelName').value = hotelData.name;
        document.getElementById('hotelLocation').value = hotelData.location;
        document.getElementById('hotelDescription').value = hotelData.description;
        document.getElementById('numberOfRooms').value = hotelData.capacity;
        document.getElementById('hotelStars').value = hotelData.stars;
        document.getElementById('deleteimagebutton').id_image = hotelData.pictureList;
        } catch (error) {
            console.error('Error fetching user data:', error);
        }
    }


    const createHotelBtn = document.getElementById('createHotelBtn');


    createHotelBtn.addEventListener('click', async function () {
        const token = localStorage.getItem('token');
        if (!token) {
            alert('Please log in to create your hotel.');
            window.location.href = 'index.html';
            return;
        }

        const hotelData = await {
            name: document.getElementById('hotelName').value,
            location: document.getElementById('hotelLocation').value,
            description: document.getElementById('hotelDescription').value,
            capacity: document.getElementById('numberOfRooms').value,
            stars: document.getElementById('hotelStars').value,
            pictureList: await uploadImage(document.getElementById('hotelPicture').files),

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
                const userEmail = await getCurrentUserEmail(token);
                const userId = await getCurrentUserId(token);
                fetch(`http://localhost:7071/api/User/Update/${userId}`, {
                    method: 'PUT',
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': `Bearer ${token}`
                    },
                    body: JSON.stringify({
                        id: userId, email: userEmail, role: "1", hotels: [result.id]
                    })
                })
                    .then(response => {
                        if (!response.ok) {
                            throw new Error('Failed to update user status');
                        }
                        alert('User status updated successfully');
                        localStorage.removeItem('token');
                        window.location.href = 'index.html';
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

    async function uploadImage(files) {
        if (!files.length) {
            alert("Please select a file.");
            return [];
        }
        resulturl = [];
        console.log(files);
        const uploadPromises = Array.from(files).map(async file => {
            const formData = new FormData();
            formData.append("file", file);

            try {
                const response = await fetch('http://localhost:7071/api/CreateMedia', {
                    method: 'POST',
                    headers: { 'Authorization': `Bearer ${token}` },
                    body: formData,
                });

                if (!response.ok) {
                    throw new Error('Failed to upload image');
                }

                const result = await response.json();
                return result.mediaUrl;
            } catch (error) {
                console.error('Error uploading image:', error);
                return '';
            }
        })
        const resultUrls = await Promise.all(uploadPromises);
        console.log(resultUrls);
        return resultUrls.filter(url => url);
    };

    const updateButtons = document.getElementById('editbutton');

    updateButtons.addEventListener('click', async function () {
            const hotelData = {
                id: document.getElementById('formeditbusiness').id_hotel,
                name: document.getElementById('hotelName').value,
                location: document.getElementById('hotelLocation').value,
                description: document.getElementById('hotelDescription').value,
                capacity: document.getElementById('numberOfRooms').value,
                stars: document.getElementById('hotelStars').value,
                pictureList: document.getElementById('deleteimagebutton').id_image.value,
            };

            const token = localStorage.getItem('token');
            const response = await fetch(`http://localhost:7071/api/Hotel/Update/${hotelData.id}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(hotelData)
            });

            if (!response.ok) {
                console.error('Failed to update hotel');
                // Handle error
                return;
            }

            alert('Hotel updated successfully');
            // Handle successful update
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
            const email = await response.text();
            return email;
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

    await roleUser(userId, token);
});
