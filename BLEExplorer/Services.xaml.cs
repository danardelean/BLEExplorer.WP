using BLEExplorer.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace BLEExplorer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Services : Page
    {
        private readonly NavigationHelper navigationHelper;

        ServiceUuidToName cUToName=new ServiceUuidToName();
        CharacteristicName cCName = new CharacteristicName();
        ByteToText cByteToText = new ByteToText();
        ByteToString cByteToString = new ByteToString();
        CharacteristicProperties cCharacteristicProperties = new CharacteristicProperties();
        public Services()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        /// 
        BluetoothLEDevice _device;
        DataTransferManager _dataTransferManager;
        IReadOnlyList<GattDeviceService> _services;
        StatusBarProgressIndicator _progressbar;
        string _idDevice;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            _progressbar = StatusBar.GetForCurrentView().ProgressIndicator;
          
            await _progressbar.ShowAsync();
            if ((e.Parameter != null) && (e.Parameter.GetType() == typeof(DeviceInformation)))
            {
                
                try
                {
                    _idDevice=((DeviceInformation)e.Parameter).Id;
                    _device = await BluetoothLEDevice.FromIdAsync(((DeviceInformation)e.Parameter).Id);
                    this.lblDeviceName.Text = ((DeviceInformation)e.Parameter).Name+" " + BLEHelper.AddressToString(_device.BluetoothAddress);
                    if (_device == null)
                        new MessageDialog("Could not connect to the selected device!", "Error").ShowAsync();

                    _services = _device.GattServices;
                    lstServices.ItemsSource = _services;
                }
                catch (Exception ex)
                {
                    new MessageDialog("Device enumeration error: " + ex.Message, "Error").ShowAsync();
                }
            }
            this.navigationHelper.OnNavigatedTo(e);
            await _progressbar.HideAsync();
          
            _dataTransferManager = DataTransferManager.GetForCurrentView();
            _dataTransferManager.DataRequested += OnDataRequested;

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
            _dataTransferManager.DataRequested -= OnDataRequested;
        }

        private void lstServices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstServices.SelectedIndex < 0)
                return;

            this.Frame.Navigate(typeof(Characteristics), _services[lstServices.SelectedIndex]);
        }

        //private async void AppBarButtonEmail_Click(object sender, RoutedEventArgs e)
        //{
        //    var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();
        //    emailMessage.Body = "";
        //    emailMessage.Subject = _device.Name;
             
        //    await _progressbar.ShowAsync();
        //    emailMessage.Body=await DumpDeviceInfo();
        //    await _progressbar.HideAsync();
        //    await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);
        //}

        private async Task<string> DumpDeviceInfo()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                sb.Append("Device: " + _device.Name+Environment.NewLine);
                sb.Append("Id: " + _idDevice + Environment.NewLine);
                sb.Append("Address: " + BLEHelper.AddressToString(_device.BluetoothAddress) + Environment.NewLine);
                sb.Append(Environment.NewLine);
                sb.Append("Services:" + Environment.NewLine);
              
                foreach (var s in _services)
                {
                    sb.Append(Environment.NewLine);
                    sb.Append("Service: " + (string)cUToName.Convert(s.Uuid, typeof(string), null, null) + Environment.NewLine);
                    sb.Append("UUID: " + s.Uuid + Environment.NewLine);
                    foreach(var c in s.GetAllCharacteristics())
                    {
                        sb.Append(Environment.NewLine);
                        sb.Append("Characteristic: " + ((string)cCName.Convert(c, typeof(string), null, null)).Replace("\0","") + Environment.NewLine);
                        sb.Append("UUID: " + c.Uuid + Environment.NewLine);
                        sb.Append((string)cCharacteristicProperties.Convert(c, typeof(string), null, null) + Environment.NewLine);

                        if (((int)c.CharacteristicProperties & (int)GattCharacteristicProperties.Read) != 0)
                        {
                            try
                            {
                                GattReadResult readResult = await c.ReadValueAsync();
                                if (readResult.Status == GattCommunicationStatus.Success)
                                {
                                    var value = new byte[readResult.Value.Length];
                                    DataReader.FromBuffer(readResult.Value).ReadBytes(value);
                                    sb.Append((string)cByteToString.Convert(value, typeof(string), null, null) + Environment.NewLine);
                                    sb.Append( ((string)cByteToText.Convert(value, typeof(string), null, null)).Replace("\0","") + Environment.NewLine);
                                }
                            }
                            catch(Exception ex) {
                                int kk = 0;
                            }
                        }
                    }
                    sb.Append(Environment.NewLine);
                }
            }
            catch
            {

            }
            return sb.ToString();
        }

        private void AppBarButtonShare_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        protected async void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs e)
        {
            var request = e.Request;
            var deferral = request.GetDeferral();

            await _progressbar.ShowAsync();
            e.Request.Data.Properties.Title = _device.Name;
            var txt = await DumpDeviceInfo();
            e.Request.Data.SetText(txt);
          
            deferral.Complete();

            await _progressbar.HideAsync();
        }
    }

        
}
