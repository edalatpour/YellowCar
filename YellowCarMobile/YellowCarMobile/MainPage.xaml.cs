using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Newtonsoft.Json;
using YellowCar.Models;

namespace YellowCarMobile
{

    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            TakePicture();
        }

        private void TakePictureButton_Clicked(object sender, EventArgs e)
        {
            TakePicture();
        }

        private async void TakePicture()
        {
            string message = "";

            CapturedImage.Source = null;
            TakePictureButton.IsEnabled = false;
            TakePictureButton.Text = "Analyzing...";
            //OutputLabel.IsVisible = false;
            //OutputLabel.Text = "";

            try
            {

                byte[] imageBytes = await GetImage();

                if (imageBytes.Length > 0)
                {
                    ImageSource source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                    CapturedImage.Source = source;
 
                    YellowCarModel model = await CallApi(imageBytes);
                    if (model.IsYellowHummer)
                        message = "Yellow Hummer - I win!";
                    else if (model.IsYellowCar)
                        message = "Yellow Car - I win!";
                    else
                        message = "Nope!";
                }
            }
            catch(Exception ex)
            {
                message = "Something went wrong.";
            }
            finally
            {
                //if (message.Length > 0)
                //    await DisplayAlert("Yellow Car!", message, "Got it!");
                TakePictureButton.Text = message;
                TakePictureButton.IsEnabled = true;
            }


        }

        private async Task<byte[]> GetImage()
        {
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                return null;
            }

            StoreCameraMediaOptions mediaOptions = new StoreCameraMediaOptions();

            mediaOptions.PhotoSize = PhotoSize.Small;
            mediaOptions.CompressionQuality = 75;
            mediaOptions.DefaultCamera = CameraDevice.Rear;
            mediaOptions.AllowCropping = false;

            byte[] imageBytes = new byte[] { };
            Stream fileStream = null;

            //var file = await CrossMedia.Current.TakePhotoAsync(mediaOptions);
            var file = await CrossMedia.Current.PickPhotoAsync();

            if (file != null)
            {
                fileStream = file.GetStream();
                BinaryReader binaryReader = new BinaryReader(fileStream);
                imageBytes = binaryReader.ReadBytes((int)fileStream.Length);
                binaryReader.Dispose();
            }

            file.Dispose();

            return imageBytes;

        }

        private async Task<YellowCarModel> CallApi(byte[] byteData)
        {
            var client = new HttpClient();

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // Test locally
                // string uri = "http://localhost:64279/api/image";

                // Hit the Back-end API directly in Azure
                // string uri = "http://yellowcarservice.azurewebsites.net/api/image";

                // Call API through API Management
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "cb7e61342d9342dfaa88f5d9f426435c");
                string uri = "https://cloudappdemoapimanagement.azure-api.net/yellowcar/Image";

                HttpResponseMessage message = await client.PostAsync(uri, content);

                string jsonText = await message.Content.ReadAsStringAsync();

                JsonSerializer serializer = new JsonSerializer();
                //YellowCarResponse response = serializer.Deserialize<YellowCarResponse>(jsonText);
                YellowCarModel model = (YellowCarModel)serializer.Deserialize(new StringReader(jsonText), typeof(YellowCarModel));

                return model;

            }

        }

    }

}
