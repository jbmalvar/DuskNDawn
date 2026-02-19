const express = require('express');
const { MongoClient } = require('mongodb');

const app = express();
const port = 3000;

app.use(express.json());

const mongoUrl = 'mongodb+srv://GS:<db_password>@march16.cizdio7.mongodb.net/?appName=March16';
const dbName = 'GameAnalytics';
const collectionName = 'LevelEvents';

let db;

MongoClient.connect(mongoUrl)
    .then(client => {
        console.log('Connected successfully to MongoDB');
        db = client.db(dbName);
    })
    .catch(err => console.error('MongoDB connection error:', err));

app.post('/analytics', async (req, res) => {
    try {
        const payload = req.body;

        // Extract IP address from the request
        const clientIp = req.headers['x-forwarded-for'] || req.socket.remoteAddress;
        payload.ip_address = clientIp;

        // Insert into MongoDB
        const collection = db.collection(collectionName);
        const result = await collection.insertOne(payload);

        console.log(`Saved analytics for session: ${payload.session_id}`);
        res.status(201).json({ message: 'Analytics saved successfully', id: result.insertedId });
    } catch (error) {
        console.error('Error saving analytics:', error);
        res.status(500).json({ error: 'Internal Server Error' });
    }
});

app.listen(port, () => {
    console.log(`Analytics API listening on port ${port}`);
});