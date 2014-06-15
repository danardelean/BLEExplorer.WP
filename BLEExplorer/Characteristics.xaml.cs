using BLEExplorer.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
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
    public sealed partial class Characteristics : Page
    {
        GattDeviceService _service;
        ServiceUuidToName _conv = new ServiceUuidToName();
        private readonly NavigationHelper navigationHelper;
        public Characteristics()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
        }
        List<CharacteristicWithValue> _characteristics;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            StatusBarProgressIndicator progressbar = StatusBar.GetForCurrentView().ProgressIndicator;
            await progressbar.ShowAsync();
            if ((e.Parameter != null) && (e.Parameter.GetType() == typeof(GattDeviceService)))
            {
                _service = ((GattDeviceService)e.Parameter);
                this.lblDeviceName.Text = _service.Device.Name;
                this.lblServiceName.Text = (string)_conv.Convert(_service.Uuid, typeof(string), null, null);
                this.lblServiceAddress.Text = _service.Uuid.ToString();

                _characteristics = new List<CharacteristicWithValue>();
                foreach (var c in _service.GetAllCharacteristics())
                {
                    var val = new CharacteristicWithValue();
                    val.GattCharacteristic = c;


                    if (((int)c.CharacteristicProperties & (int)GattCharacteristicProperties.Read) != 0)
                    {
                        try
                        {
                            GattReadResult readResult = await c.ReadValueAsync();
                            if (readResult.Status == GattCommunicationStatus.Success)
                            {
                                val.Value = new byte[readResult.Value.Length];
                                DataReader.FromBuffer(readResult.Value).ReadBytes(val.Value);
                            }
                        }
                        catch { }
                    }
                    _characteristics.Add(val);
                }

                lstCharacteristics.ItemsSource = _characteristics;
            }

            this.navigationHelper.OnNavigatedTo(e);
            await progressbar.HideAsync();
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }


        //GattReadResult readResult = await bodySensorLocationCharacteristics[0].ReadValueAsync();
        //            if (readResult.Status == GattCommunicationStatus.Success)
        //            {
        //                byte[] bodySensorLocationData = new byte[readResult.Value.Length];

        //                DataReader.FromBuffer(readResult.Value).ReadBytes(bodySensorLocationData);
    }

    public class CharacteristicWithValue
    {
        public GattCharacteristic GattCharacteristic { get; set; }
        public byte[] Value { get; set; }
    }
}
