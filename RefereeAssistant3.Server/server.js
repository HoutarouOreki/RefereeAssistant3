'use strict';
const https = require('https');
const fs = require('fs');
const express = require('express');
const bodyParser = require('body-parser');
const mongoose = require('mongoose');
const app = express();

var appFolder = process.env.APPDATA + '/RefereeAssistant3Server/';
var configPath = appFolder + 'config.json';
var config;
if (fs.existsSync(configPath)) {
    var configText = fs.readFileSync();
    config = JSON.parse(configText);
} else {
    if (!fs.existsSync(appFolder)) {
        fs.mkdirSync(appFolder);
    }
    config = {
        "port": 1337,
        "databaseConnectionString": null
    }
    fs.writeFileSync(configPath, JSON.stringify(config))
}
const port = config.port || 1337;

mongoose.Promise = global.Promise;
mongoose.connect(config.databaseConnectionString, { useCreateIndex: true, useNewUrlParser: true });

app.use(bodyParser.json())
app.use(bodyParser.urlencoded({ extended: false }))

https.createServer(app).listen(port);

console.log('Server started on port ' + port);
