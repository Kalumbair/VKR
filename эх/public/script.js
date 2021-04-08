
$(document).ready(()=>{
    $.ajax("/api/sensors").then(result_sensor=> {
        $("#temperature").text(result_sensor[0].toFixed(1));
        $("#humidity").text(result_sensor[1].toFixed(0));
        $("#gas").text(result_sensor[2].toFixed(2));

        $("#temperature").css("background-color",color_sensors(40,0,result_sensor[0]));
        $("#humidity").css("background-color",color_sensors_vl(100,0,result_sensor[1]));
        $("#gas").css("background-color",color_sensors(2,-2,result_sensor[2]));

        if (result_sensor[0]<17)
        {
            $("#errors").append("<div>Температура в помещении понизилась</div>");
        }
        else if (result_sensor[0]>25)
        {
            $("#errors").append("<div>Температура в помещении выше нормы</div>");
        }
        
        if (result_sensor[1]<40)
        {
            $("#errors").append("<div>Влажность в помещении ниже нормы</div>");
        }
        else if (result_sensor[1]>60)
        {
            $("#errors").append("<div>Влажность в помещении выше нормы</div>");
        }

        if (result_sensor[2]>0.3)
            $("#errors").append("<div>Загазованность в помещении выше нормы</div>");
                
    });

    
    $.ajax("/devices").then(result_devices=> {
        let str="";
        for ( let device of result_devices){
            str+=`<div data-id="${device.idDevice}"
            data-value="${device.statusData[0] ? 1 : 0}"
            onclick="toggle_device(this)"
            style="background-color: ${device.statusData[0]? 'yellow':'gray'}">
            Устройство ${device.nameDevice}</div>`;
        }
        document.querySelector('#Device').innerHTML=str;
        
    });
});                                           

function color_sensors(max,min,value)
{
    let sr=(max+min)/2;
    if(value>max)
        value=max;
    else if( value<min)
        value=min;
let red=0;
let green=255;
let blue=0;

let value_color=Math.round(Math.abs(value-sr)*(255/(max-sr)));
green-=value_color*5;
if(green<0)
green=0;
if(value>sr)
red=value_color;
else
blue=value_color;
    
let r = (red.toString(16).length == 1 ? '0' : '')+red.toString(16);
let g = (green.toString(16).length == 1 ? '0' : '')+green.toString(16);
let b = (blue.toString(16).length == 1 ? '0' : '')+blue.toString(16);
return r + g + b;
}
function color_sensors_vl(max,min,value)
{
    let sr=(max+min)/2;
    if(value>max)
        value=max;
    else if( value<min)
        value=min;
let blue=128;

let value_color=Math.round(Math.abs(value-sr)*(127/(max-sr)));
if(value>sr)
blue-=value_color;
else
blue+=value_color;
    
let b = (blue.toString(16).length == 1 ? '0' : '')+blue.toString(16);
return '0000' + b;
}

function toggle_device(e){
$.post('/devices',{ idDevice: e.dataset.id, statusData: [!(+e.dataset.value)] }, 
() => {
    e.dataset.value=+e.dataset.value ? 0 : 1;
    $(e).css('background-color', +e.dataset.value? 'yellow':'gray');
})
}