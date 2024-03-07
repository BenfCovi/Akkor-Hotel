document.addEventListener('DOMContentLoaded', function () {
    const bookHotelButton = document.getElementById('bookHotelButton');
    if (bookHotelButton) {
        bookHotelButton.addEventListener('click', async function () {
            const dateFrom = document.getElementById('dateFrom').value;
            const dateTo = document.getElementById('dateTo').value;
            const numberOfRooms = document.getElementById('numberOfRooms').value;

            if (!dateFrom || !dateTo || !numberOfRooms) {
                alert('Please fill in all fields.');
                return;
            }
            const token = localStorage.getItem('token');
            const userId = await getCurrentUserId(token);
            const hotelId = bookHotelButton.getAttribute('data-hotel-id');

            if (new Date(dateTo) <= new Date(dateFrom)) {
                alert("The check-out date must be later than the check-in date.");
                return;
            }

            const reservation = { userId, hotelId, dateFrom, dateTo, numberOfRooms };

            sendReservationToAPI(reservation);
        });
    };

    function sendReservationToAPI(inforeservation) {

        const token = localStorage.getItem('token');
        if (!token) {
            alert('Please log in to make a reservation.');
            return;
        }

        const reservation = {
            idUser: inforeservation.userId,
            idHotel: inforeservation.hotelId,
            startDate: inforeservation.dateFrom,
            endDate: inforeservation.dateTo,
            pictureList: inforeservation.pictureList,
            numberOfRoom: inforeservation.numberOfRooms
        };
        console.log('Reservation:', reservation);

        fetch('http://localhost:7071/api/Reservation/Create', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
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
            })
            .catch((error) => {
                console.error('Error:', error);
                alert('An error occurred while saving the reservation.');
            });
    }

    async function fetchReservations() {
        const noReservationCard = document.getElementById('noReservationCard');
        if (!noReservationCard) return;

        const token = localStorage.getItem('token');
        if (!token) {
            alert('Please log in to view your reservations.');
            window.location.href = 'index.html';
            return;
        }

        const userId = await getCurrentUserId(token);

        fetch(`http://localhost:7071/api/Reservation/GetAllByUser/${userId}`, {
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
                    noReservationCard.style.display = 'none';
                    const container = document.getElementById('ReservationCardContainer');
                    container.innerHTML = '';
                    reservations.sort((a, b) => new Date(a.startDate) - new Date(b.startDate));
                    reservations.forEach(async reservation => {
                        const cardHtml = await createReservationCard(reservation, token);
                        container.appendChild(cardHtml);
                    });
                    document.querySelectorAll('.cancelReservationButton').forEach(button => {
                        button.addEventListener('click', function() {
                            const reservationId = this.getAttribute('data-reservation-id');
                            cancelReservation(reservationId);
                        });
                    });
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('No reservation found for you.');
            });
    }

    fetchReservations();

    async function createReservationCard(reservation, token) {
        hotelinfo = await getCurrentHotelInfo(reservation.idHotel, token)
        const days = calculateDaysBetweenDates(reservation.startDate, reservation.endDate);
        const card = document.createElement('div');
        card.innerHTML = `
        <div class="d-flex flex-column justify-content-center align-items-center flex-wrap"
        style="background: var(--bs-focus-ring-color);border-radius: 12px;padding-top: 8px;padding-bottom: 8px;padding-right: 12px;padding-left: 12px;margin-top: 24px;margin-bottom: 24px;">
        <div class="d-flex justify-content-center flex-wrap justify-content-lg-center">
            <!-- Start: Image card -->
            <div>
                <h3>My reservation for :</h3><img class="rounded" src="${hotelinfo.pictureList[0]}" loading="auto"
                    style="max-height: 250px;max-width: 250px;">
            </div><!-- End: Image card -->
            <!-- Start: Info Card -->
            <div style="padding-right: 12px;padding-left: 12px;margin-top: 12px;margin-bottom: 12px;">
                <div class="d-flex align-items-center"
                    style="margin-left: 12px;margin-bottom: 12px;margin-right: 12px;margin-top: 12px;"><svg
                        xmlns="http://www.w3.org/2000/svg" viewBox="-64 0 512 512" width="1em" height="1em"
                        fill="currentColor" class="d-lg-flex justify-content-lg-center align-items-lg-center"
                        style="width: 32px;height: 32px;margin-right: 8px;margin-top: 12px;margin-bottom: 12px;">
                        <!--! Font Awesome Free 6.4.2 by @fontawesome - https://fontawesome.com License - https://fontawesome.com/license/free (Icons: CC BY 4.0, Fonts: SIL OFL 1.1, Code: MIT License) Copyright 2023 Fonticons, Inc. -->
                        <path
                            d="M64 48c-8.8 0-16 7.2-16 16V448c0 8.8 7.2 16 16 16h80V400c0-26.5 21.5-48 48-48s48 21.5 48 48v64h80c8.8 0 16-7.2 16-16V64c0-8.8-7.2-16-16-16H64zM0 64C0 28.7 28.7 0 64 0H320c35.3 0 64 28.7 64 64V448c0 35.3-28.7 64-64 64H64c-35.3 0-64-28.7-64-64V64zm88 40c0-8.8 7.2-16 16-16h48c8.8 0 16 7.2 16 16v48c0 8.8-7.2 16-16 16H104c-8.8 0-16-7.2-16-16V104zM232 88h48c8.8 0 16 7.2 16 16v48c0 8.8-7.2 16-16 16H232c-8.8 0-16-7.2-16-16V104c0-8.8 7.2-16 16-16zM88 232c0-8.8 7.2-16 16-16h48c8.8 0 16 7.2 16 16v48c0 8.8-7.2 16-16 16H104c-8.8 0-16-7.2-16-16V232zm144-16h48c8.8 0 16 7.2 16 16v48c0 8.8-7.2 16-16 16H232c-8.8 0-16-7.2-16-16V232c0-8.8 7.2-16 16-16z">
                        </path>
                    </svg>
                    <h6 style="margin-top: 12px;margin-bottom: 12px;">In&nbsp;</h6><!-- Start: Hotel name -->
                    <h6 class="fw-bold" style="margin-top: 12px;margin-bottom: 12px;">${hotelinfo.name}</h6>
                    <!-- End: Hotel name -->
                </div>
                <div class="d-flex align-items-center"
                    style="margin-left: 12px;margin-bottom: 12px;margin-right: 12px;margin-top: 12px;"><svg
                        xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512" width="1em" height="1em"
                        fill="currentColor" class="d-lg-flex justify-content-lg-center align-items-lg-center"
                        style="width: 32px;height: 32px;margin-right: 8px;">
                        <!--! Font Awesome Free 6.4.2 by @fontawesome - https://fontawesome.com License - https://fontawesome.com/license/free (Icons: CC BY 4.0, Fonts: SIL OFL 1.1, Code: MIT License) Copyright 2023 Fonticons, Inc. -->
                        <path
                            d="M464 256A208 208 0 1 0 48 256a208 208 0 1 0 416 0zM0 256a256 256 0 1 1 512 0A256 256 0 1 1 0 256zm306.7 69.1L162.4 380.6c-19.4 7.5-38.5-11.6-31-31l55.5-144.3c3.3-8.5 9.9-15.1 18.4-18.4l144.3-55.5c19.4-7.5 38.5 11.6 31 31L325.1 306.7c-3.2 8.5-9.9 15.1-18.4 18.4zM288 256a32 32 0 1 0 -64 0 32 32 0 1 0 64 0z">
                        </path>
                    </svg>
                    <h6>In&nbsp;</h6><!-- Start: Location -->
                    <h6 class="fw-bold">${hotelinfo.location}</h6><!-- End: Location -->
                </div>
                <div class="d-flex align-items-center"
                    style="margin-left: 12px;margin-bottom: 12px;margin-right: 12px;margin-top: 12px;"><svg
                        xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512" width="1em" height="1em"
                        fill="currentColor" class="d-lg-flex justify-content-lg-center align-items-lg-center"
                        style="width: 32px;height: 32px;margin-right: 8px;">
                        <!--! Font Awesome Free 6.4.2 by @fontawesome - https://fontawesome.com License - https://fontawesome.com/license/free (Icons: CC BY 4.0, Fonts: SIL OFL 1.1, Code: MIT License) Copyright 2023 Fonticons, Inc. -->
                        <path
                            d="M464 256A208 208 0 1 1 48 256a208 208 0 1 1 416 0zM0 256a256 256 0 1 0 512 0A256 256 0 1 0 0 256zM232 120V256c0 8 4 15.5 10.7 20l96 64c11 7.4 25.9 4.4 33.3-6.7s4.4-25.9-6.7-33.3L280 243.2V120c0-13.3-10.7-24-24-24s-24 10.7-24 24z">
                        </path>
                    </svg><!-- Start: How many time -->
                    <h6 class="fw-bold">${days}</h6><!-- End: How many time -->
                    <h6>&nbsp;day</h6>
                </div>
                <div class="d-flex align-items-center"
                    style="margin-left: 12px;margin-bottom: 12px;margin-right: 12px;margin-top: 12px;"><svg
                        xmlns="http://www.w3.org/2000/svg" viewBox="-32 0 512 512" width="1em" height="1em"
                        fill="currentColor" class="d-lg-flex justify-content-lg-center align-items-lg-center"
                        style="width: 32px;height: 32px;margin-right: 8px;">
                        <!--! Font Awesome Free 6.4.2 by @fontawesome - https://fontawesome.com License - https://fontawesome.com/license/free (Icons: CC BY 4.0, Fonts: SIL OFL 1.1, Code: MIT License) Copyright 2023 Fonticons, Inc. -->
                        <path
                            d="M128 0c13.3 0 24 10.7 24 24V64H296V24c0-13.3 10.7-24 24-24s24 10.7 24 24V64h40c35.3 0 64 28.7 64 64v16 48V448c0 35.3-28.7 64-64 64H64c-35.3 0-64-28.7-64-64V192 144 128C0 92.7 28.7 64 64 64h40V24c0-13.3 10.7-24 24-24zM400 192H48V448c0 8.8 7.2 16 16 16H384c8.8 0 16-7.2 16-16V192zM329 297L217 409c-9.4 9.4-24.6 9.4-33.9 0l-64-64c-9.4-9.4-9.4-24.6 0-33.9s24.6-9.4 33.9 0l47 47 95-95c9.4-9.4 24.6-9.4 33.9 0s9.4 24.6 0 33.9z">
                        </path>
                    </svg>
                    <h6>From&nbsp;</h6><!-- Start: Begin date -->
                    <h6 class="fw-bold">${formatDate(reservation.startDate)}</h6><!-- End: Begin date -->
                </div>
                <div class="d-flex align-items-center"
                    style="margin-left: 12px;margin-bottom: 12px;margin-right: 12px;margin-top: 12px;"><svg
                        xmlns="http://www.w3.org/2000/svg" viewBox="-32 0 512 512" width="1em" height="1em"
                        fill="currentColor" class="d-lg-flex justify-content-lg-center align-items-lg-center"
                        style="width: 32px;height: 32px;margin-right: 8px;">
                        <!--! Font Awesome Free 6.4.2 by @fontawesome - https://fontawesome.com License - https://fontawesome.com/license/free (Icons: CC BY 4.0, Fonts: SIL OFL 1.1, Code: MIT License) Copyright 2023 Fonticons, Inc. -->
                        <path
                            d="M304 128a80 80 0 1 0 -160 0 80 80 0 1 0 160 0zM96 128a128 128 0 1 1 256 0A128 128 0 1 1 96 128zM49.3 464H398.7c-8.9-63.3-63.3-112-129-112H178.3c-65.7 0-120.1 48.7-129 112zM0 482.3C0 383.8 79.8 304 178.3 304h91.4C368.2 304 448 383.8 448 482.3c0 16.4-13.3 29.7-29.7 29.7H29.7C13.3 512 0 498.7 0 482.3z">
                        </path>
                    </svg><!-- Start: Number room -->
                    <h6 class="fw-bold">${reservation.numberOfRoom}</h6><!-- End: Number room -->
                    <h6>&nbsp;room(s)</h6>
                </div>
            </div><!-- End: Info Card -->
        </div>
        <form class="d-flex flex-column justify-content-center">
            <h6>You can cancel your reservation but this action is definitively.</h6><button
                class="btn btn-danger" type="button" id="cancelReservationButton" data-reservation-id="${reservation.id}"
                style="padding-right: 24px;padding-left: 24px;">Cancel</button>
        </form>
    </div>
        `;
        return card;
    }

    async function cancelReservation(reservationId, token) {
        if (!token) {
            alert('Please log in to update your profile.');
            window.location.href = 'index.html';
            return;
        }
        try {
            const response = await fetch(`http://localhost:7071/api/Hotel/Delete/${reservationId}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) {
                throw new Error('Failed to delete account');
            }

            alert('Your reservation has been deleted.');
            fetchReservations();
        } catch (error) {
            alert('Error deleting account: ' + error.message);
        }
    }

    async function getCurrentHotelInfo(idHotel, token) {
        try {
            const response = await fetch(`http://localhost:7071/api/Hotel/Get/${idHotel}`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });
            if (!response.ok) {
                throw new Error('Failed to fetch user profile');
            }
            const hotelinfo = await response.json();
            console.log(hotelinfo);
            return hotelinfo;
        } catch (error) {
            console.error('Error fetching user profile:', error);
        }
    }

    function calculateDaysBetweenDates(startDate, endDate) {
        const start = new Date(startDate);
        const end = new Date(endDate);
        return Math.round((end - start) / (1000 * 60 * 60 * 24));
    }

    function formatDate(dateString) {
        const options = { year: 'numeric', month: 'long', day: 'numeric' };
        return new Date(dateString).toLocaleDateString(undefined, options);
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
