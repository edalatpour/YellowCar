using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Script.Serialization;

using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types

using YellowCar.Models;

namespace YellowCar.Controllers
{
    public class ImageController : ApiController
    {

        [HttpGet]
        public async Task<List<YellowCarModel>> Get()
        {
            YellowCarModel m1 = new YellowCarModel();
            m1.ImageFileName = "xxxxx";
            m1.When = "Yesterday";
            m1.Where = "Ambler, PA";
            m1.IsYellowCar = true;
            m1.IsYellowHummer = false;

            YellowCarModel m2 = new YellowCarModel();
            m2.ImageFileName = "yyyyy";
            m2.When = "Today";
            m2.Where = "Durham, NC";
            m2.IsYellowCar = true;
            m2.IsYellowHummer = true;

            //YellowCarModel[] models = new YellowCarModel[] { m1, m2 };
            List<YellowCarModel> models = new List<YellowCarModel>();
            QueryDatabase(models);
            return models;

        }

        [HttpPost]
        public async Task<YellowCarModel> Post()
        {

            YellowCarModel model = new YellowCarModel();
            model.Who = "Anonymous User";
            model.When = DateTime.Now.ToString();
            model.Where = "Location not provided";

            byte[] imageBytes = new byte[] { };

            try
            {
                imageBytes = await Request.Content.ReadAsByteArrayAsync();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return model;
            }

            try
            {
                // Store the image in Blob storage
                string fileName = await StoreImage(imageBytes);
                model.ImageFileName = fileName;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return model;
            }

            CustomVisionResponse cvResponse;
                
            try
            {
                // Dtermine whether it contains good stuff
                //string description = await AnalyzeImage(imageBytes);
                cvResponse = await ClassifyImage(imageBytes);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return model;
            }

            try
            {
                ScoreImage(cvResponse, ref model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return model;
            }

            try
            {
                UpdateDatabase(ref model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return model;
            }

            return model;

        }

        private void QueryDatabase(List<YellowCarModel> models)
        {
            string databaseConnectionString = CloudConfigurationManager.GetSetting("DatabaseConnectionString");
            IDbConnection connection = new SqlConnection(databaseConnectionString);
            connection.Open();
            using (connection)
            {
                IDbCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                string commandText = string.Format("SELECT UserId, WhenTaken, WhereTaken, FileName, IsYellowCar, IsYellowHummer FROM Images");
                System.Diagnostics.Debug.WriteLine(commandText);
                command.CommandText = commandText;
                IDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    YellowCarModel model = new YellowCarModel();
                    model.Who = reader.GetString(0);
                    model.When = reader.GetString(1);
                    model.Where = reader.GetString(2);
                    model.ImageFileName = reader.GetString(3);
                    model.IsYellowCar = reader.GetBoolean(4);
                    model.IsYellowHummer = reader.GetBoolean(5);
                    models.Add(model);
                }
                reader.Close();
            }
            return;
        }

        private void UpdateDatabase(ref YellowCarModel model)
        {
            string databaseConnectionString = CloudConfigurationManager.GetSetting("DatabaseConnectionString");
            IDbConnection connection = new SqlConnection(databaseConnectionString);
            connection.Open();
            using (connection)
            {
                IDbCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                string commandText = string.Format("INSERT INTO Images(UserId, WhenTaken, WhereTaken, FileName, IsYellowCar, IsYellowHummer) VALUES ('{0}', '{1}', '{2}', '{3}', {4}, {5})",
                                                   model.Who,
                                                   model.When,
                                                   model.Where,
                                                   model.ImageFileName,
                                                   model.IsYellowCar ? 1 : 0,
                                                   model.IsYellowHummer ? 1 : 0);
                System.Diagnostics.Debug.WriteLine(commandText);
                command.CommandText = commandText;
                int rows = command.ExecuteNonQuery();
            }
            return;
        }

        private void ScoreImage(CustomVisionResponse cvResponse, ref YellowCarModel model)
        {
            bool isYellowCar = false;
            bool isHummer = false;
            bool isTaxi = false;

            foreach (Prediction p in cvResponse.Predictions)
            {
                if (p.Tag == "yellow car" && p.Probability >= 0.9f)
                    isYellowCar = true;
                if (p.Tag == "hummer" && p.Probability >= 0.9f)
                    isHummer = true;
                if (p.Tag == "taxi" && p.Probability >= 0.9f)
                    isTaxi = true;
            }

            if (isYellowCar && !isTaxi)
            {
                model.IsYellowCar = true;
                if (isHummer)
                {
                    model.IsYellowHummer = true;
                }
            }
        }
            
        private async Task<string> StoreImage(byte[] imageBytes)
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("yellowcar");

            // Retrieve reference to a blob named "myblob".
            string blobName = Guid.NewGuid().ToString();
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

            // Create or overwrite the "myblob" blob with contents from a local file.
            using (Stream imageStream = new MemoryStream(imageBytes))
            {
                blockBlob.UploadFromStream(imageStream);
            }

            return blobName;

        }

        private async Task<string> AnalyzeImage(byte[] byteData)
        {
            var client = new HttpClient();

            // Request headers - replace this example key with your valid subscription key.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "50bd13222fc64462bb320bc550359b5b");

            // Request parameters. A third optional parameter is "details".
            // string requestParameters = "visualFeatures=Categories,Tags,Description,Color&language=en";
            string requestParameters = "visualFeatures=Description&language=en";
            string uri = "https://eastus2.api.cognitive.microsoft.com/vision/v1.0/describe?" + requestParameters;
            // Console.WriteLine(uri);

            HttpResponseMessage response;
            string description = "";

             using (var content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json" and "multipart/form-data" and "application/octet-stream".
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(uri, content);
                description = await response.Content.ReadAsStringAsync();
                
                // Console.WriteLine(desc);
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            DescribeResponse describeResponse = serializer.Deserialize<DescribeResponse>(description);

            string caption = describeResponse.description.captions[0].text;

            return caption;

        }

        static async Task<CustomVisionResponse> ClassifyImage(byte[] byteData)
        {
            var client = new HttpClient();

            // Request headers - replace this example key with your valid subscription key.
            client.DefaultRequestHeaders.Add("Prediction-Key", "bc3b3e45dd014b5d88a6afb7638cfb8f");

            string uri = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/cbc490e3-3e7e-4343-8c12-664fd197c699/image";

            HttpResponseMessage message;

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                message = await client.PostAsync(uri, content);
                string jsonText = await message.Content.ReadAsStringAsync();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                CustomVisionResponse response = serializer.Deserialize<CustomVisionResponse>(jsonText);

                return response;
            }

        }

    }

}