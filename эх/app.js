var express = require('express');
const path = require('path');
const fetch=require('node-fetch');
var app = express();
//датчики
let sensors = []; 
// устрйоства
let devices = [];

app.use(express.json());
app.use(express.urlencoded({ extended: true }));
app.use(express.static(path.join(__dirname, 'public')));

// backend отправляет информацию о датчиках - текущие значения
app.post('/api/sensors', (req, res) =>{
    try
    {
        sensors = req.body.map(i => +i);
        //sensors.push(req.body.map(i => +i));
        res.sendStatus(200);
    }
    catch 
    {
        res.sendStatus(400)
    };
});

app.post('/device/single', (req, res)=>{
    let dev=devices.find(d => d.idDevice==req.body.idDevice);
    if (dev==null){
        res.sendStatus(400);
        return;
    }
    dev.statusData=req.body.statusData;
})
//отправляет значения датчиков на клиентскую часть (сайт)
app.get('/api/sensors', (req, res) =>{
    res.send(sensors);
});

//отправляет список устройств на клиентскую часть (сайт)
app.get('/devices', (req, res) =>{
    res.send(devices);
});

//запрос клиентской части на изменения состояния устройств
app.post('/devices', (req,res) => {
    let dev=devices.find(d => d.idDevice==req.body.idDevice);
    if (dev==null){
        res.sendStatus(400);
        return;
    }
    fetch('http://127.0.0.1:3000/device', {
        method: 'POST',
        body: JSON.stringify({id: req.body.idDevice, status: req.body.statusData})
    })
    .then(result => {
        dev.statusData=req.body.statusData;
        res.sendStatus(200);
    });

})

Device_Сkan();
//прослушивание
app.listen(80);
//Сервер подкличлся
console.log(`Running server at http://127.0.0.1`);

//запрашивание списка устройств из backend
function Device_Сkan()
{
    fetch('http://127.0.0.1:3000/device')
    .then(response => response.json())
    .then(result => devices=result);
}
