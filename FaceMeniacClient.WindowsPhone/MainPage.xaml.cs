using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Media.Capture;      //For MediaCapture  
using Windows.Media.MediaProperties;  //For Encoding Image in JPEG format  
using Windows.Storage;         //For storing Capture Image in App storage or in Picture Library  
using Windows.UI.Xaml.Media.Imaging;  //For BitmapImage. for showing image on screen we need BitmapImage format. 
using System.Diagnostics;
using Windows.Storage.Pickers;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Activation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace FaceMeniacClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private StorageFile _pictureTake;
        CoreApplicationView view;
        Windows.Media.Capture.MediaCapture captureManager;

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
            this.capturePreview.Stretch = Stretch.Uniform;
            this.capturePreview.UseLayoutRounding = true;
            this.imagePlaceholder.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        //Declare MediaCapture object globally  

        async private void Stop_Capture_Preview_Click(object sender, RoutedEventArgs e)
        {
            await captureManager.StopPreviewAsync();  //stop camera capturing  
        }

        protected void CaptureOnClick(object sender, RoutedEventArgs e)
        {
            // TODO: continuar a implementação. Enviar a imagem.
            Debug.WriteLine("Picture attributes: " + this._pictureTake.Attributes);
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
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

            try
            {
                // Get photo as a BitmapImage using storage file path.  
                BitmapImage bmpImage = new BitmapImage(new Uri(file.Path));

                // show captured image on Image UIElement.  
                //this.imagePreivew.Source = bmpImage;

                this.imagePlaceholder.Source = bmpImage;
                this.capturePreview.Visibility = Visibility.Collapsed;
                this.imagePlaceholder.Visibility = Visibility.Visible;
                this._pictureTake = file;
                
            }
            catch (Exception) { }

            //await captureManager.StopPreviewAsync();  //stop camera capturing  
        }

        private async void AppBarButton_Click_1(object sender, RoutedEventArgs e)
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

        private async void viewActivated(CoreApplicationView sender, IActivatedEventArgs args1)
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
                //imagePreivew.Source = bitmapImage;
            }
        }

        private void AppBarButton_Click_2(object sender, RoutedEventArgs e)
        {

        }
    }
}
