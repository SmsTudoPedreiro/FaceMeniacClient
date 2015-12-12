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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace FaceMeniacClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private StorageFile _pictureTake; 

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            SetupCamera();
        }

        async protected void SetupCamera()
        {
            captureManager = new MediaCapture();    //Define MediaCapture object  
            await captureManager.InitializeAsync();   //Initialize MediaCapture and   
            capturePreview.Source = captureManager;   //Start preiving on CaptureElement  
            await captureManager.StartPreviewAsync();  //Start camera capturing   
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
        Windows.Media.Capture.MediaCapture captureManager;
        async private void Start_Capture_Preview_Click(object sender, RoutedEventArgs e)
        {
            captureManager = new MediaCapture();    //Define MediaCapture object  
            await captureManager.InitializeAsync();   //Initialize MediaCapture and   
            capturePreview.Source = captureManager;   //Start preiving on CaptureElement  
            await captureManager.StartPreviewAsync();  //Start camera capturing   
        }
        async private void Stop_Capture_Preview_Click(object sender, RoutedEventArgs e)
        {
            await captureManager.StopPreviewAsync();  //stop camera capturing  
        }
        async private void Capture_Photo_Click(object sender, RoutedEventArgs e)
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
                imagePreivew.Source = bmpImage;

                this._pictureTake = file;
            }
            catch (Exception) { }

            this.uxBtnSendPhoto.IsEnabled = true;
            this.uxBtnCapture.IsEnabled = false;
        }

        protected void CaptureOnClick(object sender, RoutedEventArgs e)
        {
            // TODO: continuar a implementação. Enviar a imagem.
            Debug.WriteLine("Picture attributes: " + this._pictureTake.Attributes);
        }
    }
}
