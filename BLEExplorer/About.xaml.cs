using BLEExplorer.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
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
    public sealed partial class About : Page
    {
        private readonly NavigationHelper navigationHelper;
        public About()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }
        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
            var ver = Windows.ApplicationModel.Package.Current.Id.Version;
            var bt=await RetrieveLinkerTimestamp(typeof(App).GetTypeInfo().Assembly);
            lblVer.Text = "v." + ver.Major + "." + ver.Minor;
            if (bt!=null)
                lblVer.Text+= " "+((DateTimeOffset)bt).DateTime.ToString("MM/dd/yy");
        }

        public static async Task<DateTimeOffset?> RetrieveLinkerTimestamp(Assembly assembly)
        {
            var pkg = Windows.ApplicationModel.Package.Current;
            if (null == pkg)
            {
                return null;
            }

            var assemblyFile = await pkg.InstalledLocation.GetFileAsync(assembly.ManifestModule.Name);
            if (null == assemblyFile)
            {
                return null;
            }

            using (var stream = await assemblyFile.OpenSequentialReadAsync())
            {
                using (var reader = new DataReader(stream))
                {
                    const int PeHeaderOffset = 60;
                    const int LinkerTimestampOffset = 8;

                    //read first 2048 bytes from the assembly file.
                    byte[] b = new byte[2048];
                    await reader.LoadAsync((uint)b.Length);
                    reader.ReadBytes(b);
                    reader.DetachStream();

                    //get the pe header offset
                    int i = System.BitConverter.ToInt32(b, PeHeaderOffset);

                    //read the linker timestamp from the PE header
                    int secondsSince1970 = System.BitConverter.ToInt32(b, i + LinkerTimestampOffset);

                    var dt = new DateTimeOffset(1970, 1, 1, 0, 0, 0, DateTimeOffset.Now.Offset) + DateTimeOffset.Now.Offset;
                    return dt.AddSeconds(secondsSince1970);
                }
            }
        }

        private async void ButtonContact_Click(object sender, RoutedEventArgs e)
        {
            var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();
            emailMessage.Body = "";
            emailMessage.Subject = "BLEExplorer Feedback";
            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }

       

        private void ButtonRate_Click(object sender, RoutedEventArgs e)
        {
            Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
        }
    }


}
