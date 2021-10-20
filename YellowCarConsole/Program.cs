using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace CSHttpClientSample
{
    static class Program
    {
        static void Main(string[] args)
        {
            string imageFilePath = "";
            if (args.Length == 0)
            {
                Console.Write("Enter image file path: ");
                imageFilePath = Console.ReadLine();
            }
            else
            {
                imageFilePath = args[0];
            }

            CallApi(imageFilePath);

            Console.WriteLine("\n\n\nHit ENTER to exit...");
            Console.ReadLine();
        }

        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }

        static async void CallApi(string imageFilePath)
        {
            var client = new HttpClient();

            byte[] byteData = GetImageAsByteArray(imageFilePath);

            using (var content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json" and "multipart/form-data" and "application/octet-stream".
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                //string uri = "http://yellowcarapi.azurewebsites.net/api/image";
                string uri = "http://localhost:64279/api/image";
                HttpResponseMessage response = await client.PostAsync(uri, content);
                //HttpResponseMessage response = await client.GetAsync(uri);
                string desc = await response.Content.ReadAsStringAsync();
                Console.WriteLine(desc);
            }

        }

        //static async void MakeAnalysisRequest(string imageFilePath)
        //{
        //    var client = new HttpClient();

        //    // Request headers - replace this example key with your valid subscription key.
        //    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "50bd13222fc64462bb320bc550359b5b");

        //    // Request parameters. A third optional parameter is "details".
        //    string requestParameters = "visualFeatures=Categories,Tags,Description,Color&language=en";
        //    string uri = "https://eastus2.api.cognitive.microsoft.com/vision/v1.0/analyze?" + requestParameters;
        //    Console.WriteLine(uri);

        //    HttpResponseMessage response;

        //    // Request body. Try this sample with a locally stored JPEG image.
        //    byte[] byteData = GetImageAsByteArray(imageFilePath);

        //    using (var content = new ByteArrayContent(byteData))
        //    {
        //        // This example uses content type "application/octet-stream".
        //        // The other content types you can use are "application/json" and "multipart/form-data" and "application/octet-stream".
        //        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        //        response = await client.PostAsync(uri, content);
        //        string desc = await response.Content.ReadAsStringAsync();
        //        Console.WriteLine(desc);
        //    }
        //}
    }
}