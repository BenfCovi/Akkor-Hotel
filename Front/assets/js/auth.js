document.addEventListener('DOMContentLoaded', function () {
    const loginForm = document.getElementById('loginForm');
    const signinForm = document.getElementById('singinForm');

    updateAuthButtons();

    function updateAuthButtons() {
        const userActionsDiv = document.getElementById('userActions');
        const userToken = localStorage.getItem('token');

        if (userToken) {
            // Utilisateur connecté
            userActionsDiv.innerHTML = `
                <a class="btn btn-primary" role="button"
                    style="background: var(--bs-body-bg);color: var(--bs-emphasis-color);border-radius: 0px;border-style: none;border-right-style: none;border-right-color: var(--bs-gray);border-left-width: 0px;border-left-style: none;"
                    href="javascript:void(0);" id="logoutButton">Disconnect</a>
                <hr class="vr">
                <a class="btn btn-primary" role="button"
                    style="color: var(--bs-navbar-brand-color);background: var(--bs-body-bg);border-radius: 0px;border-style: none;border-right-style: none;border-left: 0px none var(--bs-gray-600);"
                    href="profil.html">Profil<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512"
                                width="1em" height="1em" fill="currentColor"
                                style="width: 20px;height: 20px;margin-left: 10px;">
                    <path d="M399 384.2C376.9 345.8 335.4 320 288 320H224c-47.4 0-88.9 25.8-111 64.2c35.2 39.2 86.2 63.8 143 63.8s107.8-24.7 143-63.8zM0 256a256 256 0 1 1 512 0A256 256 0 1 1 0 256zm256 16a72 72 0 1 0 0-144 72 72 0 1 0 0 144z"></path>
                </svg></a>
            `;
            attachLogoutEvent();
        } else {
            // Utilisateur non connecté
            userActionsDiv.innerHTML = `
                <button class="btn btn-primary" type="button"
                    style="background: var(--bs-body-bg);color: var(--bs-emphasis-color);border-radius: 0px;border-style: none;border-right-style: none;border-right-color: var(--bs-gray);border-left-width: 0px;border-left-style: none;"
                    data-bs-target="#modal-signin" data-bs-toggle="modal">Sign Up</button>
                <hr class="vr">
                <button class="btn btn-primary" type="button"
                    style="color: var(--bs-navbar-brand-color);background: var(--bs-body-bg);border-radius: 0px;border-style: none;border-right-style: none;border-left: 0px none var(--bs-gray-600);"
                    data-bs-target="#modal-login" data-bs-toggle="modal">Login<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512" width="1em" height="1em" fill="currentColor" style="width: 20px;height: 20px;margin-left: 10px;">
                    <path d="M399 384.2C376.9 345.8 335.4 320 288 320H224c-47.4 0-88.9 25.8-111 64.2c35.2 39.2 86.2 63.8 143 63.8s107.8-24.7 143-63.8zM0 256a256 256 0 1 1 512 0A256 256 0 1 1 0 256zm256 16a72 72 0 1 0 0-144 72 72 0 1 0 0 144z"></path>
                </svg></button>
            `;
            attachLoginSinginEvent();
        }
    }

    function attachLogoutEvent() {
        const logoutButton = document.getElementById('logoutButton');
        if (logoutButton) {
            logoutButton.addEventListener('click', function () {
                logoutUser();
            });
        }
    }

    async function attachLoginSinginEvent() {
        $(document).ready(async function() {
        const loginButton = await document.getElementById('loginButton');
        console.log(loginButton);

            loginButton.addEventListener('click', async function (event) {
                event.preventDefault();
                const email = document.getElementById('loginEmail').value;
                const password = document.getElementById('loginPassword').value;
                loginUser(email, password);
            });


        const singinButton = await document.getElementById('singinButton');
        console.log(singinButton);

            singinButton.addEventListener('click', async function (event) {
                event.preventDefault();
                const email = document.getElementById('singinEmail').value;
                const password = document.getElementById('signinPassword').value;
                const confirmPassword = document.getElementById('signinConfirmPassword').value;
                if (password === confirmPassword) {
                    registerUser(email, password);
                } else {
                    alert("Passwords do not match.");
                }
            });
        });

    }

    async function registerUser(email, password) {
        try {
            const createUserResponse = await fetch('http://localhost:7071/api/User/Create', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ email: email, passwordHash: password }),
            });
            if (!createUserResponse.ok) {
                const errorDetails = await createUserResponse.text();
                throw new Error(`Registration failed: ${errorDetails}`);
            }
            // Authentifier l'utilisateur directement après l'inscription
            await loginUser(email, password);
            alert('Registration successful! You are now logged in.');
            window.location.reload();
        } catch (error) {
            console.error('Registration error:', error);
            alert(`Registration error: ${error.message}`);
        }
    }

    async function loginUser(email, password) {
        try {
            const response = await fetch('http://localhost:7071/api/AuthFunction', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ email: email, password: password }),
            });
            if (!response.ok) {
                const errorDetails = await response.text();
                throw new Error(`Login failed: ${errorDetails}`);
            }
            const data = await response.json();
            localStorage.setItem('token', data.token);
            alert('Login successful!');
            window.location.reload();
        } catch (error) {
            console.error('Login error:', error);
            alert(`Login error: ${error.message}`);
        }
    }


    function logoutUser() {
        localStorage.removeItem('token');
        // Mise à jour de l'UI ou redirection
        window.location.reload();
    }


});
