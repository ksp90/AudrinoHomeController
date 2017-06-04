using Android.App;
using Android.Widget;
using Android.OS;
using Android.Bluetooth;
using System.Linq;
using System;
using System.Text;
using System.Threading.Tasks;
using HomeController;
//using Android.Media;

namespace HomeController
{
    [Activity(Label = "HomeController", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {

        BluetoothConnection myConnection = new BluetoothConnection();
        Button button1, button2, button3, button4, button5, button6, button7, button8, button9;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button buttonConnect = FindViewById<Button>(Resource.Id.button1);
            Button buttonDisconnect = FindViewById<Button>(Resource.Id.button2);

            button1 = FindViewById<Button>(Resource.Id.button3);
            button2 = FindViewById<Button>(Resource.Id.button4);
            button3 = FindViewById<Button>(Resource.Id.button5);
            button4 = FindViewById<Button>(Resource.Id.button6);
            button5 = FindViewById<Button>(Resource.Id.button7);
            button6 = FindViewById<Button>(Resource.Id.button8);
            button7 = FindViewById<Button>(Resource.Id.button9);
            button8 = FindViewById<Button>(Resource.Id.button10);
            button9 = FindViewById<Button>(Resource.Id.button11);

            TextView connected = FindViewById<TextView>(Resource.Id.textView1);

            BluetoothSocket _socket = null;

            buttonConnect.Click += delegate
            {
                myConnection = new BluetoothConnection();
                myConnection.getAdapter();
                if (myConnection.thisAdapter != null)
                {
                    myConnection.thisAdapter.StartDiscovery();

                    try
                    {
                        myConnection.getDevice();
                        myConnection.thisDevice.SetPairingConfirmation(false);
                        myConnection.thisDevice.SetPairingConfirmation(true);
                        myConnection.thisDevice.CreateBond();
                    }
                    catch
                    {
                    }

                    myConnection.thisAdapter.CancelDiscovery();


                    _socket = myConnection.thisDevice.CreateRfcommSocketToServiceRecord(Java.Util.UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));

                    myConnection.thisSocket = _socket;
                    
                    try
                    {
                        myConnection.thisSocket.Connect();
                        connected.Text = "Connected!";
                        buttonDisconnect.Enabled = true;
                        buttonConnect.Enabled = false;
                    }
                    catch
                    {
                    }
                }
            };

            buttonDisconnect.Click += delegate
            {
                try
                {
                    buttonConnect.Enabled = true;
                    buttonDisconnect.Enabled = false;
                    myConnection.thisDevice.Dispose();
                    myConnection.thisSocket.OutputStream.WriteByte(187);
                    myConnection.thisSocket.OutputStream.Close();
                    myConnection.thisSocket.Close();
                    myConnection = new BluetoothConnection();
                    _socket = null;

                    connected.Text = "Disconnected!";
                }
                catch { }
            };

            button1.Click += OnItemClick;
            button2.Click += OnItemClick;
            button3.Click += OnItemClick;
            button4.Click += OnItemClick;
            button5.Click += OnItemClick;
            button6.Click += OnItemClick;
            button7.Click += OnItemClick;
            button8.Click += OnItemClick;

            button9.Click += AllOnOffClick;
        }

        private async void AllOnOffClick(object sender, EventArgs e)
        {
            var s = sender as Button;
            var text = s.Text;
            byte[] data = null;
            if (text.Contains("OFF"))
            {
                try
                {
                    for (int i = 1; i <= 8; i++)
                    {
                        data = Encoding.ASCII.GetBytes(string.Format("{0} ON", i));
                        if (myConnection.thisSocket.OutputStream.CanWrite)
                        {
                            myConnection.thisSocket.OutputStream.Flush();
                            myConnection.thisSocket.OutputStream.Write(data, 0, data.Length);
                            await Task.Delay(150);
                            data = null;
                        }
                    }
                }
                catch { return; }
                text = text.Replace("OFF", "ON");
                MakeALlButtonStatusChange("ON");
            }
            else
            {
                try
                {
                    for (int i = 1; i <= 8; i++)
                    {
                        data = Encoding.ASCII.GetBytes(string.Format("{0} OFF", i));
                        if (myConnection.thisSocket.OutputStream.CanWrite)
                        {
                            myConnection.thisSocket.OutputStream.Flush();
                            myConnection.thisSocket.OutputStream.Write(data, 0, data.Length);
                            await Task.Delay(150);
                            data = null;
                        }
                    }
                }
                catch { return; }
                text = text.Replace("ON", "OFF");
                MakeALlButtonStatusChange("OFF");
            }
            s.SetText(text, TextView.BufferType.Normal);
        }

        private void OnItemClick(object sender, EventArgs e)
        {
            var s = sender as Button;
            var text = s.Text;
            byte[] data = null;
            if (text.Contains("OFF"))
            {
                text = text.Replace("OFF", "ON");
                data = Encoding.ASCII.GetBytes(text);
            }
            else
            {
                text = text.Replace("ON", "OFF");
                data = Encoding.ASCII.GetBytes(text);
            }
            try
            {
                if (myConnection.thisSocket.OutputStream.CanWrite)
                {
                    myConnection.thisSocket.OutputStream.Flush();
                    myConnection.thisSocket.OutputStream.Write(data, 0, data.Length);
                    s.SetText(text, TextView.BufferType.Normal);
                }
            }
            catch
            {
            }
        }

        private void MakeALlButtonStatusChange(string onOffStatus)
        {
            string sourceText = onOffStatus == "ON" ? "OFF" : "ON";
            button1.SetText(button1.Text.Replace(sourceText, onOffStatus), TextView.BufferType.Normal);
            button2.SetText(button2.Text.Replace(sourceText, onOffStatus), TextView.BufferType.Normal);
            button3.SetText(button3.Text.Replace(sourceText, onOffStatus), TextView.BufferType.Normal);
            button4.SetText(button4.Text.Replace(sourceText, onOffStatus), TextView.BufferType.Normal);
            button5.SetText(button5.Text.Replace(sourceText, onOffStatus), TextView.BufferType.Normal);
            button6.SetText(button6.Text.Replace(sourceText, onOffStatus), TextView.BufferType.Normal);
            button7.SetText(button7.Text.Replace(sourceText, onOffStatus), TextView.BufferType.Normal);
            button8.SetText(button8.Text.Replace(sourceText, onOffStatus), TextView.BufferType.Normal);
        }
    }

    public class BluetoothConnection
    {

        public void getAdapter() { this.thisAdapter = BluetoothAdapter.DefaultAdapter; }
        public void getDevice()
        {
            if (this.thisAdapter != null)
                this.thisDevice = thisAdapter.BondedDevices.FirstOrDefault(bd => bd.Name == "HC-05");
            else
                thisDevice = null;
        }

        public BluetoothAdapter thisAdapter { get; set; }
        public BluetoothDevice thisDevice { get; set; }

        public BluetoothSocket thisSocket { get; set; }
    }
}