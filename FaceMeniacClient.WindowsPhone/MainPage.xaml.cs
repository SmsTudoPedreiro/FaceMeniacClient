using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using Windows.Media.Capture;      //For MediaCapture  
using Windows.Media.MediaProperties;  //For Encoding Image in JPEG format  
using Windows.Storage;         //For storing Capture Image in App storage or in Picture Library  
using Windows.UI.Xaml.Media.Imaging;  //For BitmapImage. for showing image on screen we need BitmapImage format. 
using Windows.Storage.Pickers;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Activation;
using System.Net;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Cubisoft.Winrt.Ftp;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace FaceMeniacClient
{
    public sealed partial class MainPage : Page
    {
        CoreApplicationView view;
        Windows.Media.Capture.MediaCapture captureManager;
        private BitmapImage _image;

        public BitmapImage Image
        {
            get
            {
                return this._image;
            }
            set
            {
                this._image = value;
                this.imagePlaceholder.Source = this._image;
            }
        }

        public StorageFile File { get; set; }


        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
            this.view = CoreApplication.GetCurrentView();

            SetupCamera();
        }

        private async void SetupCamera()
        {
            captureManager = new MediaCapture();    //Define MediaCapture object  
            await captureManager.InitializeAsync();   //Initialize MediaCapture and   
            capturePreview.Source = captureManager;   //Start preiving on CaptureElement  
            await captureManager.StartPreviewAsync();  //Start camera capturing
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e) // tira foto
        {
            //Create JPEG image Encoding format for storing image in JPEG type  
            ImageEncodingProperties imgFormat = ImageEncodingProperties.CreateJpeg();
            
            // create storage file in local app storage  
            Random random = new Random();
            File = await ApplicationData.Current.LocalFolder.CreateFileAsync(random.Next(1, 9999) + ".jpg", CreationCollisionOption.ReplaceExisting);

            // take photo and store it on file location.  
            await captureManager.CapturePhotoToStorageFileAsync(imgFormat, File);

            StorageFolder folder = KnownFolders.SavedPictures;
            await File.MoveAsync(folder);

            // Get photo as a BitmapImage using storage file path.  
            Image = new BitmapImage(new Uri(File.Path));
            await captureManager.StopPreviewAsync();  //stop camera capturing  
        }

        private async void AppBarButton_Click_1(object sender, RoutedEventArgs e) // pega arquivo
        {
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            filePicker.ViewMode = PickerViewMode.Thumbnail;

            // Filter to include a sample subset of file types
            filePicker.FileTypeFilter.Clear();
            filePicker.FileTypeFilter.Add(".jpg");

            filePicker.PickSingleFileAndContinue();
            this.view.Activated += viewActivated;
            await captureManager.StopPreviewAsync();  //stop camera capturing  
        }

        private async void viewActivated(CoreApplicationView sender, IActivatedEventArgs args1) // quando arquivo é escolhido
        {
            FileOpenPickerContinuationEventArgs args = args1 as FileOpenPickerContinuationEventArgs;

            if (args != null)
            {
                if (args.Files.Count == 0) return;

                this.view.Activated -= viewActivated;
                File = args.Files[0];
                var stream = await File.OpenAsync(Windows.Storage.FileAccessMode.Read);
                var bitmapImage = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                await bitmapImage.SetSourceAsync(stream);

                var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
                Image = bitmapImage;
            }
        }

        private async void  AppBarButton_Click_2(object sender, RoutedEventArgs e) // envia imagem para ftp
        {
            FtpClient ftp = new FtpClient()
            {
                HostName = new Windows.Networking.HostName("tudopedreirorj.netai.net"),
                Credentials = new NetworkCredential("a1713127", "j123456"),
                ServiceName = "21"
            };

            await ftp.ConnectAsync();
            await ftp.SetDataTypeAsync(FtpDataType.Binary);
            using (var stream = await ftp.OpenWriteAsync("/public_html/" + File.Name))
            {
                byte[] bytes = null;

                bytes = await ReadFile(File);
                await Task.Delay(TimeSpan.FromSeconds(1));
                Debug.WriteLine("2------------" + bytes.Length);
                await stream.WriteAsync(bytes.AsBuffer());
                await Task.Delay(TimeSpan.FromSeconds(1));
                await stream.FlushAsync();
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            await ftp.DisconnectAsync();
        }

        // que metodo sensual, rapá!
        private async Task<byte[]> ReadFile(StorageFile file)
        {
            byte[] fileBytes = null;
            using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
            {
                fileBytes = new byte[stream.Size];
                using (DataReader reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }

            return fileBytes;
        }
    }
}
