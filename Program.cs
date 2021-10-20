using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CustomVisionImageUploader
{

    class MainClass
    {
        
        public static void Main(string[] args)
        {
            MakeRequest().Wait();
        }

        static async Task MakeRequest()
        {
			try
			{
                string tag = "taxi";
				string resultsText = await BingImageSearch(tag);
				// Console.WriteLine(resultsText);

				JsonSerializer serializer = new JsonSerializer();
				BingImageSearchResponse response = (BingImageSearchResponse)serializer.Deserialize(new StringReader(resultsText), typeof(BingImageSearchResponse));

                foreach(Image image in response.value)
                {
                    string imageUrl = image.contentUrl;
                    await UploadTrainingImageFromUrl(imageUrl, tag);
                    //Console.WriteLine(imageUrl);
                }

            }
			catch (Exception ex)
			{
                Console.WriteLine(ex.Message);
			}
		}

        private static async Task<string> BingImageSearch(string query)
        {

			var client = new HttpClient();
            //var queryString = HttpUtility.ParseQueryString(string.Empty);

			// Request headers - replace this example key with your valid subscription key.
			client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "0982fbba5bcc499fb508edab7984f2f4");

			string uri = "https://api.cognitive.microsoft.com/bing/v5.0/images/search?count=100&q=" + query;

            HttpResponseMessage message = await client.GetAsync(uri);

            string jsonText = await message.Content.ReadAsStringAsync();

            return jsonText;
		
        }

        private static async Task UploadTrainingImageFromUrl(string imageUrl, string tag)
        {

            CreateImagesFromUrlsRequest req = new CreateImagesFromUrlsRequest();
            req.TagIds = new string[] { tag };
            req.Urls = new string[] { imageUrl };

            string requestJson = JsonConvert.SerializeObject(req);
            byte[] byteData = Encoding.UTF8.GetBytes(requestJson);

			var client = new HttpClient();
			
			// Request headers - replace this example key with your valid subscription key.
			client.DefaultRequestHeaders.Add("Training-Key", "e613bf2a6f6c487b9570fe58832c55c4");

			string uri = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Training/projects/cbc490e3-3e7e-4343-8c12-664fd197c699/images/url";

			using (HttpContent content = new ByteArrayContent(byteData))
            {
				HttpResponseMessage message = await client.PostAsync(uri, content);
                if(message.IsSuccessStatusCode)
                {
                    Console.WriteLine("Uploaded: " + imageUrl);
                }
			}

		}

    }

}
