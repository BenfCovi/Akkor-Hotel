const express = require('express');
const app = express();
const bodyParser = require('body-parser');
const fs = require('fs');

app.use(bodyParser.urlencoded({ extended: true }));

app.post('/search-form', (req, res) => {
    const destination = req.body.destination;
    const dateFrom = req.body['date from'];
    const dateTo = req.body['date to'];
    const numberRoom = req.body['number room'];

    // Créez un objet avec les données du formulaire
    const formData = {
        destination,
        dateFrom,
        dateTo,
        numberRoom
    };

    // Écrivez les données du formulaire dans un fichier JSON
    fs.writeFile('formData.json', JSON.stringify(formData, null, 2), (err) => {
        if (err) throw err;
        console.log('Data written to file');
    });

    res.send('Form submitted successfully!');
});

app.listen(3000, () => console.log('Server started on port 3000'));