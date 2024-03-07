# Documentation de l'API

Cette documentation fournit des informations sur les différentes requêtes disponibles dans l'API de gestion d'hôtels.

## Authentification

### Authentifier un utilisateur

- Endpoint: `POST /AuthFunction`
- Description: Cette requête permet d'authentifier un utilisateur et de retourner un jeton d'authentification.
- Paramètres de la requête: Aucun
- Corps de la requête (JSON):
  ```json
  {
        "email": "string",
        "password": "string"
  }
  ```
- Réponse :
  ```json
    {
        "token": "string"
    }
  ```

## Utilisateur

### Créer un utilisateur

- Endpoint: `POST /User/Create`
- Description: Cette requête permet de créer un utilisateur.
- Paramètres de la requête: Aucun
- Corps de la requête (JSON) et réponse :
  ```json
  {
        "id": "string",
        "passwordHash": "string",
        "email": "string",
        "role": 0,
        "hotels": [
            "string"
        ]
  }
  ```
### Récupérer un utilisateur

- Endpoint: `GET ​/User/Get​/{id}`
- Description: Cette requête permet de récuperer un utilisateur.
- Paramètres de la requête: `{id}` de l'utilisateur
- Réponse :
  ```json
    {
        "id": "string",
        "passwordHash": "string",
        "email": "string",
        "role": 0,
        "hotels": [
            "string"
        ]
    }
  ```
### Récupérer un utilisateur en fonction de son Email

- Endpoint: `GET /User/Get/Email/{email}`
- Description: Cette requête permet de récuperer un utilisateur grace à son email.
- Paramètres de la requête: `{email}` de l'utilisateur
- Réponse :
  ```json
    {
        "id": "string",
        "passwordHash": "string",
        "email": "string",
        "role": 0,
        "hotels": [
            "string"
        ]
    }
  ```
### Récupérer tous les utilisateurs

- Endpoint: `GET ​/User/GetAll`
- Description: Cette requête permet de récuperer tous les utilisateurs.
- Paramètres de la requête: aucun
- Réponse :
  ```json
    {
        [
            {
                "id": "string",
                "passwordHash": "string",
                "email": "string",
                "role": 0,
                "hotels": [
                    "string"
                ]
            }
            ...
        ]
    }
  ```

### Mettre à jours un utilisateur

- Endpoint: `PUT /User/Update/{id}`
- Description: Cette requête permet de MAJ un utilisateur.
- Paramètres de la requête: `{id}` de l'utilisateur
- Corps de la requête (JSON) et réponse :
  ```json
  {
        "id": "string",
        "passwordHash": "string",
        "email": "string",
        "role": 0,
        "hotels": [
            "string"
        ]
  }
  ```

### Supprimer un utilisateur

- Endpoint: `DELETE /User/Delete/{id}`
- Description: Cette requête permet de supprimer un utilisateur.
- Paramètres de la requête: `{id}` de l'utilisateur

## Hotel

### Créer un hotel

- Endpoint: `POST /Hotel/Create`
- Description: Cette requête permet de créer un hotel.
- Paramètres de la requête: Aucun
- Corps de la requête (JSON) et réponse :
  ```json
  {
        "id": "string",
        "name": "string",
        "location": "string",
        "description": "string",
        "pictureList": [
            "string"
        ],
        "capacity": 0,
        "stars": 0
  }
  ```

### Récupérer un hotel

- Endpoint: `GET ​/Hotel​/Get​/{id}`
- Description: Cette requête permet de récuperer un hotel.
- Paramètres de la requête: `{id}` de l'hotel
- Réponse :
  ```json
    {
        "id": "string",
        "name": "string",
        "location": "string",
        "description": "string",
        "pictureList": [
            "string"
        ],
        "capacity": 0,
        "stars": 0    
    }
  ```

### Récupérer tous les hotels

- Endpoint: `GET ​/Hotel​/GetAll`
- Description: Cette requête permet de récuperer tous les hotels.
- Paramètres de la requête: aucun
- Réponse :
  ```json
    {
        [
            {
                "id": "string",
                "name": "string",
                "location": "string",
                "description": "string",
                "pictureList": [
                "string"
                ],
                "capacity": 0,
                "stars": 0
            },
            ...
        ]
    }
  ```

### Récupérer tous les hotels de luxe

- Endpoint: `GET ​/Hotel​/GetAllBest`
- Description: Cette requête permet de récuperer tous les hotels ayant 4 ou 5 étoiles.
- Paramètres de la requête: aucun
- Réponse :
  ```json
    {
        [
            {
                "id": "string",
                "name": "string",
                "location": "string",
                "description": "string",
                "pictureList": [
                "string"
                ],
                "capacity": 0,
                "stars": 0
            },
            ...
        ]
    }
  ```

### Récupérer les derniers hotels ajoutés

- Endpoint: `GET ​/Hotel/GetLast/{count}`
- Description: Cette requête permet de récuperer tous les derniers hotels ajoutés.
- Paramètres de la requête: `{count}` nombre des derniers hotels a récuperer
- Réponse :
  ```json
    {
        "id": "string",
        "name": "string",
        "location": "string",
        "description": "string",
        "pictureList": [
            "string"
        ],
        "capacity": 0,
        "stars": 0    
    }
  ```

### Mettre à jours un hotel

- Endpoint: `PUT /Hotel/Update/{id}`
- Description: Cette requête permet de MAJ un hotel.
- Paramètres de la requête: `{id}` de l'hotel
- Corps de la requête (JSON) et réponse :
  ```json
  {
        "id": "string",
        "name": "string",
        "location": "string",
        "description": "string",
        "pictureList": [
            "string"
        ],
        "capacity": 0,
        "stars": 0
  }
  ```

### Supprimer un hotel

- Endpoint: `DELETE /Hotel/Delete/{id}`
- Description: Cette requête permet de supprimer un hotel.
- Paramètres de la requête: `{id}` de l'hotel

### Récupérer le nombre totale de chambres réservées

- Endpoint: `GET /Hotel/GetMaxReservedRooms/{id}`
- Description: Cette requête permet de récuperer le nombre totale de chambres réservées d'un hotel.
- Paramètres de la requête: `{id}` de l'hotel
- Réponse :
  ```json
    {
        0    
    }
  ```

## Réservation

### Créer une réservation

- Endpoint: `POST /Reservation/Create`
- Description: Cette requête permet de créer une reservation.
- Paramètres de la requête: Aucun
- Corps de la requête (JSON) et réponse :
  ```json
  {
        "id": "string",
        "idUser": "string",
        "idHotel": "string",
        "numberOfRoom": 0,
        "startDate": "2024-03-07T21:15:31.120Z",
        "endDate": "2024-03-07T21:15:31.120Z"
  }
  ```

### Récupérer une réservation

- Endpoint: `GET ​/Reservation/Get​/{id}`
- Description: Cette requête permet de récuperer une réservation.
- Paramètres de la requête: `{id}` de la réservation
- Réponse :
  ```json
  {
        "id": "string",
        "idUser": "string",
        "idHotel": "string",
        "numberOfRoom": 0,
        "startDate": "2024-03-07T21:15:31.120Z",
        "endDate": "2024-03-07T21:15:31.120Z"
  }
  ```

### Récupérer toutes les réservations

- Endpoint: `GET ​/Reservation/GetAll`
- Description: Cette requête permet de récuperer toutes les réservations.
- Paramètres de la requête: aucun
- Réponse :
  ```json
    {
        [
            {
                "id": "string",
                "idUser": "string",
                "idHotel": "string",
                "numberOfRoom": 0,
                "startDate": "2024-03-07T21:17:45.983Z",
                "endDate": "2024-03-07T21:17:45.983Z"
            },
            ...
        ]
    }
  ```

### Récupérer toutes les réservations d'un utilisateur

- Endpoint: `GET /Reservation/GetAllByUser/{userId}`
- Description: Cette requête permet de récuperer tous les réservations qu'un utilisateur a pris.
- Paramètres de la requête: `{userId}` (int) Id de l'utilisateur
- Réponse :
  ```json
    {
        [
            {
                "id": "string",
                "name": "string",
                "location": "string",
                "description": "string",
                "pictureList": [
                "string"
                ],
                "capacity": 0,
                "stars": 0
            },
            ...
        ]
    }
  ```

### Récupérer toutes les réservations d'un hotel

- Endpoint: `GET /Reservation/GetAllByHotel/{hotelId}`
- Description: Cette requête permet de récuperer toutes les réservations prises dans un hotel.
- Paramètres de la requête: `{hotelId}` (int) Id de l'hotel
- Réponse :
  ```json
    {
        [
            {
                "id": "string",
                "name": "string",
                "location": "string",
                "description": "string",
                "pictureList": [
                "string"
                ],
                "capacity": 0,
                "stars": 0
            },
            ...
        ]
    }
  ```

### Mettre à jours une réservation

- Endpoint: `PUT /Reservation/Update/{id}`
- Description: Cette requête permet de MAJ une reservation.
- Paramètres de la requête: `{id}` de la reservation
- Corps de la requête (JSON) et réponse :
  ```json
  {
            "id": "string",
            "name": "string",
            "location": "string",
            "description": "string",
            "pictureList": [
                "string"
            ],
            "capacity": 0,
            "stars": 0
  }
  ```

### Supprimer une réservation

- Endpoint: `DELETE /Reservation/Delete/{id}`
- Description: Cette requête permet de supprimer une réservation.
- Paramètres de la requête: `{id}` de la réservation

## Commande pour lancer le projet et ses tests :

### Backend :
#### Prérequis :
Avoir les package "Azure Tools" installé.

#### Méthode de lancement :
Ce placer dans le dossier **Back/MohamedRemi-Test** et taper dans le terminal la commande `func start` afin de lancer les azures functions. Il est aussi possible de les déployer sur Azure dans un composant **Function App**.

### FrontEnd :
#### Prérequis :
Avoir l'extension "Live Server" installé. (visual studio code)

#### Méthode de lancement :
Ce placer dans le dossier **Front** et cliquer sur le bouton `Go Live` présent en bas à droite de l'écran après l'installation afin de lancer localement le site (front + back). Il est aussi possible de les déployer sur Azure dans un composant **Web App**.

### FrontEnd Testing :
#### Prérequis :
Avoir **Python** avec le package **Selenium** et **WebDriver** installé sur son instance.

#### Méthode de lancement :
Ce placer dans le dossier **Racine** et rentrer dans le terminal la commande `Get-ChildItem -Path .\selenium -Filter *.py | ForEach-Object {python $_.FullName}` afin de lancer les testes frontend. Un navigateur (Chrome) devrai s'ouvrir à plusieurs reprise et réaliser des actions préprogrammé afin de tester toutes les fonctionnalité du site en repartant toujours de zéro. Si un des teste échoue alors cela révèle immédiatement un défaut au niveau du frontend.

### TestBDD :
#### Prérequis :
- Sur **Visual Studio**, installer les packages **SpecFlow** et **Nunit**.

#### Méthode de lancement :
- Se placer dans le dossier **TestTDD** et appuyer sur le bouton `Lancer les tests` pour exécuter les tests BDD. Les résultats des tests doivent s'ouvrir dans une nouvelle fenêtre.

### TestTDD :
#### Prérequis :
- Sur **Visual Studio**, installer les packages **MS Test** et **Nunit**.

#### Méthode de lancement :
- Se placer dans le dossier **TestBDD** et appuyer sur le bouton `Lancer les tests` pour exécuter les tests BDD. Les résultats des tests doivent s'ouvrir dans une nouvelle fenêtre.

### Test Unitaire :
#### Prérequis :
- Sur **Visual Studio**, installer le package **Xunit**.

#### Méthode de lancement :
- Se placer dans le dossier **TestUnitaire** et appuyer sur le bouton `Lancer les tests` pour exécuter les tests unitaires. Les résultats des tests doivent s'ouvrir dans une nouvelle fenêtre.
