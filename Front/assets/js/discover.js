document.addEventListener('DOMContentLoaded', function () {
    async function loadHotels() {
        try {
            const response = await fetch('http://localhost:7071/api/Hotel/GetLast/4');
            if (!response.ok) throw new Error('Failed to fetch hotels');

            const hotels = await response.json();
            hotels.forEach((hotel, index) => {
                createDiscoverSection(hotel);
                createModal(hotel);
            });
        } catch (error) {
            console.error('Error loading hotels:', error);
        }
    }

    function createDiscoverSection(hotel) {
        const container = document.getElementById('discovercontainer');
        if (hotel.pictureList == null) {
            hotel.pictureList = []; 
            hotel.pictureList[0] = 'assets/img/header-bg.webp';
        }
        const sectionHTML = `
        <div class="d-flex" style="min-width: auto; max-width: 275px; margin: 12px;">
            <div class="d-flex justify-content-center align-items-end" data-bss-hover-animate="pulse">
                <img class="flex-fill" src="${hotel.pictureList[0] || 'assets/img/header-bg.webp'}" style="width: 100%; height: 100%; filter: brightness(75%);" loading="auto">
                <button class="btn btn-primary" type="button" data-bs-target="#modal-hotel-detail${hotel.id}" data-bs-toggle="modal" style="border-radius: 0px; border-style: none; position: absolute; background: rgba(11,24,43,0); width: 275px; height: 275px; display: block;"></button>
                <p class="text-uppercase fs-2 fw-semibold" style="color: var(--bs-body-bg); text-shadow: 2px 1px 7px var(--bs-emphasis-color); position: absolute;">${hotel.location}</p>
            </div>
        </div>
    `;
        container.innerHTML += sectionHTML;
    }

    function createModal(hotel) {
        const modalHTML = `
        <div class="modal fade" role="dialog" tabindex="-1" id="modal-hotel-detail${hotel.id}">
        <div class="modal-dialog" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <!-- Start: City name -->
                <h4 id="cityname" class="modal-title">${hotel.location}</h4><!-- End: City name --><button class="btn-close" type="button"
                    aria-label="Close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <!-- Start: Hotel picture -->
                <div class="carousel slide" data-bs-ride="false" id="carousel-${hotel.id}">
                    <div id="carrouselimg${hotel.id}" class="carousel-inner">  
                    </div>
                    <div>
                        <!-- Start: Previous --><a class="carousel-control-prev" href="#carousel-${hotel.id}" role="button"
                            data-bs-slide="prev"><span class="carousel-control-prev-icon"></span><span
                                class="visually-hidden">Previous</span></a><!-- End: Previous -->
                        <!-- Start: Next --><a class="carousel-control-next" href="#carousel-${hotel.id}" role="button"
                            data-bs-slide="next"><span class="carousel-control-next-icon"></span><span
                                class="visually-hidden">Next</span></a><!-- End: Next -->
                    </div>
                    <div class="carousel-indicators" id="carousel-indicators${hotel.id}">
                        </div>
                </div><!-- End: Hotel picture -->
                <!-- Start: Hotel name -->
                <p id="hotelname" class="fw-bold" style="margin: 0px;margin-bottom: 8px;font-size: 20px;">${hotel.name}</p>
                <!-- End: Hotel name -->
                <div class="d-flex align-items-center align-items-lg-center"><svg xmlns="http://www.w3.org/2000/svg"
                        viewBox="-64 0 512 512" width="1em" height="1em" fill="currentColor"
                        style="width: 22px;height: 22px;font-size: 22px;">
                        <!--! Font Awesome Free 6.4.2 by @fontawesome - https://fontawesome.com License - https://fontawesome.com/license/free (Icons: CC BY 4.0, Fonts: SIL OFL 1.1, Code: MIT License) Copyright 2023 Fonticons, Inc. -->
                        <path
                            d="M215.7 499.2C267 435 384 279.4 384 192C384 86 298 0 192 0S0 86 0 192c0 87.4 117 243 168.3 307.2c12.3 15.3 35.1 15.3 47.4 0zM192 128a64 64 0 1 1 0 128 64 64 0 1 1 0-128z">
                        </path>
                    </svg><!-- Start: Hotel Location -->
                    <p i="location" class="d-lg-flex" style="margin: 0px;">${hotel.location}</p><!-- End: Hotel Location -->
                </div>
                <hr style="margin-bottom: 0px;">
            </div>
            <div style="margin-left: 12px;">
                <p class="fw-semibold">Description</p><!-- Start: Hotel description -->
                <p  id="description">${hotel.description}</p><!-- End: Hotel description -->
            </div>
            <div class="modal-footer d-lg-flex justify-content-lg-center">
                <form class="d-flex justify-content-center flex-wrap justify-content-lg-center align-items-lg-center" id="bookingForm${hotel.id}">
                    <!-- Start: Input Date From -->
                    <div style="margin-right: 12px; margin-left: 12px; margin-top: 6px; margin-bottom: 12px;">
                        <small class="form-text">When? Until?</small>
                        <div class="d-flex">
                            <input class="form-control" type="date" id="dateFrom${hotel.id}"
                                   style="margin-right: 12px; height: 32px; padding-left: 6px; text-align: center; width: 130px;"
                                   name="date_from" required>
                            <hr class="vr" style="margin: 6px 0px;">
                            <input class="form-control" type="date" id="dateTo${hotel.id}"
                                   style="margin-left: 12px; padding-left: 6px; text-align: center; height: 32px; width: 130px;"
                                   name="date_to" required>
                        </div>
                    </div>
                    <!-- End: Input Date From -->
                
                    <!-- Start: Input Room -->
                    <div class="d-flex d-lg-flex flex-column justify-content-lg-start" style="margin-right: 8px;">
                        <small class="form-text">Room ?</small>
                        <input class="form-control form-control-sm" type="number" id="numberOfRooms${hotel.id}"
                               style="width: 64px; height: 32px;" name="number_room" value="1" min="1" required>
                    </div>
                    <!-- End: Input Room -->
                
                    <!-- Start: Button Validate -->
                    <button class="btn btn-primary btn-lg" type="button" id="bookHotelButton${hotel.id}"
                            style="padding-left: 24px; padding-right: 24px;" id_hotel=${hotel.id}>Book it</button>
                    <!-- End: Button Validate -->
                </form>                    
            </div>
        </div>
    
        </div>
    `;
        document.body.insertAdjacentHTML('beforeend', modalHTML);
        const carrouselimg = document.getElementById(`carrouselimg${hotel.id}`);
        const carrouselindicators = document.getElementById(`carousel-indicators${hotel.id}`);
        hotel.pictureList.forEach((picture, index) => {
            const active = index === 0 ? 'active' : '';
            carrouselimg.innerHTML += `
            <div class="carousel-item ${active}">
                <img src="${picture}" class="d-block w-100" alt="...">
            </div>
        `;
            carrouselindicators.innerHTML += `
            <button type="button" data-bs-target="#carousel-${hotel.id}" data-bs-slide-to="${index}" class="${active}"></button>
        `;
        });
        const bookHotelButton = document.getElementById(`bookHotelButton${hotel.id}`);
        if (bookHotelButton && hotel) {

            bookHotelButton.addEventListener('click', async function () {
                const dateFrom = document.getElementById(`dateFrom${hotel.id}`).value;
                const dateTo = document.getElementById(`dateTo${hotel.id}`).value;
                const numberOfRooms = document.getElementById(`numberOfRooms${hotel.id}`).value;

                if (!dateFrom || !dateTo || !numberOfRooms) {
                    alert('Please fill in all fields.');
                    return;
                }
                const token = localStorage.getItem('token');
                const userId = await getCurrentUserId(token);
                const hotelId = bookHotelButton.getAttribute('id_hotel');

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

    loadHotels();
});
