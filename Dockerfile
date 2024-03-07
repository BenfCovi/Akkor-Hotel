# Définir l'image de base. Node 14 ici comme exemple.
FROM node:14

# Définir le répertoire de travail dans le conteneur
WORKDIR /

# Copier les fichiers de dépendances dans le répertoire de travail
COPY package*.json ./

# Installer les dépendances
RUN npm install

# Copier le reste des fichiers du projet dans le conteneur
COPY . .

# Exposer le port sur lequel votre app va tourner
EXPOSE 3000

# Commande pour démarrer l'application
CMD ["npm", "start"]
