using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        const string hostname = "127.0.0.1";
        const int port = 3000;

        HttpListener http;

        public Form1()
        {
            InitializeComponent();

            http = new HttpListener();
            http.Prefixes.Add($"http://{hostname}:{port}/");
            http.Start();
            HTTPList();

            List<Device> devices = new List<Device>();
            devices.Add(new Device("Light1", "Light Kitchen", Device.DeviceType.Light));
            devices[devices.Count - 1].Changed+= d => Invoke(new Action( ()=> checkBox1.Checked =(bool)d.statusData[0]));

            Device.devices = devices.ToDictionary<Device, string>( device => device.idDevice);

            checkBox1.CheckedChanged += (s, ea) => 
            { 
                Device.devices["Light1"].statusData[0] = checkBox1.Checked;
                RequestAsync("device/single", new { idDevice = "Light1", Device.devices["Light1"].statusData });
            };
        }

        async Task HTTPList()
        {
            while (true)
            {
                var context = await http.GetContextAsync();
                switch (context.Request.RawUrl)
                {
                    case "/device":
                        if (context.Request.HttpMethod=="GET")
                        {
                            context.Response.StatusCode = 200;
                            context.Response.ContentType = "application/json";
                            context.Response.AddHeader("Charset", "UTF-8");
                            StreamWriter write= new StreamWriter(context.Response.OutputStream);
                            await write.WriteAsync(JsonConvert.SerializeObject(Device.devices.Values.ToArray()));
                            write.Close();
                            context.Response.Close(); 
                        }
                        else if (context.Request.HttpMethod=="POST")
                        {
                            try 
                            {	        
                                StreamReader reader= new StreamReader(context.Request.InputStream);
                                var device = JsonConvert.DeserializeObject<IdStatusDevice>( await reader.ReadToEndAsync());
                                reader.Close();
                                if (string.IsNullOrWhiteSpace(device.id) || device.status==null || device.status.Length==0)
                                {
                                    context.Response.StatusCode=400;
                                    context.Response.Close();
                                    break;
                                }
                                if (!Device.devices.TryGetValue(device.id, out var currentDevice))
                                {
                                    context.Response.StatusCode=400;
                                    context.Response.Close();
                                    break;
                                }
                                currentDevice[0]=bool.Parse(device.status[0].ToString());
                                if(currentDevice.statusData.Length>1 && device.status.Length>1)
                                    currentDevice[1]=device.status[1];
                                context.Response.StatusCode=200;
                                context.Response.Close();
                            }
                            catch (InvalidCastException)
                            {
                                context.Response.StatusCode=400;
                                context.Response.Close();
                                break;
                            }    
                            catch (FormatException)
                            {
                                context.Response.StatusCode=400;
                                context.Response.Close();
                                break;
                            }   
                        }
                        break;
                }
            }
        }
        private static async Task RequestAsync(string path, object o)
        {

            HttpWebRequest request = WebRequest.CreateHttp("http://127.0.0.1/" + path);
            request.Method = "POST";
            request.ContentType = ("application/json");
            StreamWriter write = new StreamWriter(request.GetRequestStream());
            write.Write(JsonConvert.SerializeObject(o));
            write.Close();
            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
            if ((int)response.StatusCode >= 300)
                MessageBox.Show("Ошибка");
            response.Close();
        }

        private void Sensors_changes(object sender, EventArgs e)
        {
            string[] sensors = new string[]
            {
                numericUpDown1.Value.ToString("0.00", CultureInfo.InvariantCulture),
                numericUpDown2.Value.ToString("0.00", CultureInfo.InvariantCulture),
                numericUpDown3.Value.ToString("0.00", CultureInfo.InvariantCulture)
            };


            RequestAsync("api/sensors", sensors);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            numericUpDown1.Value = 25.00M;
            numericUpDown2.Value = 35.10M;
            numericUpDown3.Value = 0.20M;
            
        }
    }

}
class Device
{
    public static Dictionary<string, Device> devices = new Dictionary<string, Device>();
    
    public string idDevice;

    public string nameDevice;

    public DeviceType deviceType;

    public object[] statusData;


    public Device(string id, string name, DeviceType deviceType)
    {
        idDevice = id;
        nameDevice = name;
        this.deviceType = deviceType;

        switch (deviceType)
        {
            case DeviceType.Light:
                statusData = new object[1];
                statusData[0]=false;
                break;
            case DeviceType.Television:
                statusData = new object[2];
                statusData[0] = false;
                statusData[1] = 0;
                break;
        }
    }
    public event Action<Device> Changed;
    public object this [int i]
    {
        get
        {
            return statusData[i];
        }
        set 
        {
            statusData[i] = value;
            Changed?.Invoke(this);
        }
    }
    public enum DeviceType 
    {
        Light = 0,
        Teapot = 0,
        Television = 1
    }
}

class IdStatusDevice
{
    public string id;
    public object[] status;
}