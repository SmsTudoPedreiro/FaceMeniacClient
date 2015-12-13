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
using System.IO;
using System.Threading.Tasks;

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
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(random.Next(1, 9999) + ".jpg", CreationCollisionOption.ReplaceExisting);

            // take photo and store it on file location.  
            await captureManager.CapturePhotoToStorageFileAsync(imgFormat, file);

            StorageFolder folder = KnownFolders.SavedPictures;
            await file.MoveAsync(folder);

            // Get photo as a BitmapImage using storage file path.  
            Image = new BitmapImage(new Uri(file.Path));
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
                StorageFile storageFile = args.Files[0];
                var stream = await storageFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
                var bitmapImage = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                await bitmapImage.SetSourceAsync(stream);

                var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
                Image = bitmapImage;
            }
        }

        private async void  AppBarButton_Click_2(object sender, RoutedEventArgs e) // envia imagem para ftp
        {
            WebRequest request = WebRequest.Create("ftp:tudopedreirorj.netai.net:21");
            request.Credentials = new NetworkCredential("a1713127", "j123456");
            // request.BeginGetResponse(new AsyncCallback(ReadCallback), request);

            Task<byte[]> processStreamFile = new Task<byte[]>( (this._image) => { return ReadFile(); });
            processStreamFile.Start();
            Task.WaitAll();
            byte[] streamFile = processStreamFile.Result; //await Task.Run(() => ReadFile());

            Stream requestStream = await request.GetRequestStreamAsync();
            requestStream.Write(streamFile, 0, streamFile.Length);
        }

        private byte[] ReadFile(BitmapImage image)
        {
            byte[] bufferStreamFile = null;

            return bufferStreamFile;

        }
    }
}
