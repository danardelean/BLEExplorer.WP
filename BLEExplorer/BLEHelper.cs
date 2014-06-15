using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace BLEExplorer
{
    public class BLEHelper
    {
        public static Dictionary<string, string> StandardServices = new Dictionary<string, string>() { 
            {"00001800-0000-1000-8000-00805f9b34fb","Generic Access"},
            {"00001801-0000-1000-8000-00805f9b34fb","Generic Attribute"},
            {"00001802-0000-1000-8000-00805f9b34fb","Immediate Alert"},
            {"00001803-0000-1000-8000-00805f9b34fb","Link Loss"},
            {"00001804-0000-1000-8000-00805f9b34fb","Tx Power"},
            {"00001805-0000-1000-8000-00805f9b34fb","Current Time Service"},
            {"00001806-0000-1000-8000-00805f9b34fb","Reference Time Update Service"},
            {"00001807-0000-1000-8000-00805f9b34fb","Next DST Change Service"},
            {"00001808-0000-1000-8000-00805f9b34fb","Glucose"},
            {"00001809-0000-1000-8000-00805f9b34fb","Health Thermometer"},
            {"00001810-0000-1000-8000-00805f9b34fb","Blood Pressure"},
            {"00001811-0000-1000-8000-00805f9b34fb","Alert Notification Service"},
            {"00001812-0000-1000-8000-00805f9b34fb","Human Interface Device"},
            {"00001813-0000-1000-8000-00805f9b34fb","Scan Parameters"},
            {"00001814-0000-1000-8000-00805f9b34fb","Running Speed and Cadence"},
            {"00001816-0000-1000-8000-00805f9b34fb","Cycling Speed and Cadence"},
            {"00001818-0000-1000-8000-00805f9b34fb","Cycling Power"},
            {"00001819-0000-1000-8000-00805f9b34fb","Location and Navigation"},
            {"0000180a-0000-1000-8000-00805f9b34fb","Device Information"},
            {"0000180d-0000-1000-8000-00805f9b34fb","Heart Rate"},
            {"0000180e-0000-1000-8000-00805f9b34fb","Phone Alert Status Service"},
            {"0000180f-0000-1000-8000-00805f9b34fb","Battery Service"},
            //{"f000ffc0-0451-4000-b000-000000000000","Simple Key Service"} ,
            //SENSOR TAG TI
            {"f000aa00-0451-4000-b000-000000000000","Thermometer"},
            {"f000aa10-0451-4000-b000-000000000000","Accelerometer"},
            {"f000aa20-0451-4000-b000-000000000000","Humidity"},
            {"f000aa30-0451-4000-b000-000000000000","Magnetometer"},
            {"f000aa40-0451-4000-b000-000000000000","Barometer"},
            {"f000aa50-0451-4000-b000-000000000000","Gyroscope"},
            {"f000aa60-0451-4000-b000-000000000000","Test Service"},
            {"f000ffc0-0451-4000-b000-000000000000","OAD Service"},
            {"f000ccc0-0451-4000-b000-000000000000","Connection control service"},
            {"0000ffe0-0000-1000-8000-00805f9b34fb","Simple Key Service"}
           
            
        };



        static public string AddressToString(ulong address)
        {
            var _bytes = BitConverter.GetBytes(address);

            StringBuilder result = new StringBuilder(18);
            string separator = ":";

            if (_bytes[7] != 0 && _bytes[6] != 0)
            {
                result.Append(_bytes[7].ToString("X2") + separator);
                result.Append(_bytes[6].ToString("X2") + separator);
            }

            result.Append(_bytes[5].ToString("X2") + separator);
            result.Append(_bytes[4].ToString("X2") + separator);
            result.Append(_bytes[3].ToString("X2") + separator);
            result.Append(_bytes[2].ToString("X2") + separator);
            result.Append(_bytes[1].ToString("X2") + separator);
            result.Append(_bytes[0].ToString("X2"));

            return "[" + result.ToString() + "]";
        }
    }
    public class ServiceUuidToName : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null && (value.GetType() == typeof(Guid)) && (BLEHelper.StandardServices.ContainsKey(((Guid)value).ToString())))
                return BLEHelper.StandardServices[((Guid)value).ToString()];

            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }



    public class CharacteristicName : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null && (value.GetType() == typeof(GattCharacteristic)))
            {
                var c = (GattCharacteristic)value;
                if (c.UserDescription != "")
                    return c.UserDescription;

                foreach (var prop in typeof(GattCharacteristicUuids).GetRuntimeProperties())
                {
                    object v = prop.GetValue(null);
                    if (v != null && v.GetType() == typeof(Guid) && ((Guid)v) == c.Uuid)
                        return prop.Name;
                }

            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }


    public class CharacteristicProperties : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null && (value.GetType() == typeof(GattCharacteristic)))
            {
                var c = (GattCharacteristic)value;
                return "Type: [" + c.CharacteristicProperties.ToString() + "] Protection: [" + c.ProtectionLevel.ToString() + "]";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class ByteToText : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null && (value.GetType() == typeof(byte[])) && ((byte[])value).Length > 0)
                return "String: " + System.Text.Encoding.UTF8.GetString((byte[])value, 0, ((byte[])value).Length);
            return "Not available";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class ByteToString : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string separator = ":";
            if (value != null && (value.GetType() == typeof(byte[])) && ((byte[])value).Length > 0)
            {
                StringBuilder sb=new StringBuilder();
                for(int i=((byte[])value).Length-1;i>=0;i--)
                    sb.Append(((byte[])value)[i].ToString("X2") + ((i>0)?separator:""));
                 return "Bytes: "+sb.ToString();
            }
            return "Not available";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
