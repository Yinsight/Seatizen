using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Seatizen.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Seatizen
{
    public static class HttpTriggerSeatizen
    {
        public static void Main()
        {
            Console.Write("Enter image file path: ");
            var imageFilePath = Console.ReadLine();

            MakePredictionRequest(imageFilePath).Wait();

            Console.WriteLine("\n\nHit ENTER to exit...");
            Console.ReadLine();
        }

        [FunctionName("HttpTriggerSeatizen")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var responseMessage = await MakePredictionRequest("/Images/M8 PAC01.png");

            return new OkObjectResult(responseMessage);
        }

        public static async Task<int> MakePredictionRequest(string imageFilePath)
        {
            var client = new HttpClient();

            //todo Store in user secrets.
            client.DefaultRequestHeaders.Add("Prediction-Key", "3c11f60367a846d5b05dbdd92e2900c1");

            //todo Store in AppSettings.json
            const string url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v3.0/Prediction/cfe3f19d-c988-483a-9dc5-63275acacd49/detect/iterations/Iteration2/image";

            byte[] byteData;

            try
            {
                byteData = GetImageAsByteArray(imageFilePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            using var content = new ByteArrayContent(byteData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            var response = await client.PostAsync(url, content);

            var json = await response.Content.ReadAsStringAsync();
            var results = JsonSerializer.Deserialize<PredictionObject>(json);

            return results.Predictions.Count(p => p.Probability >= 0.8M);
        }

        private static byte[] GetImageAsByteArray(string imageFilePath)
        {
            var fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            var binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }
    }

    internal class InputData
    {
        [JsonProperty("data")]
        internal object[,] Data;
    }
    public static class PredictPeopleCountFromImage
    {
        [FunctionName("PredictPeopleCountFromImage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
        HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request for PredictPeopleCountFromImage");

            if (req.Method.ToLower() == "post")
            {
                log.LogInformation($"POST method was used to invoke the function");
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogInformation("Request Body: " + requestBody);
                var tripDetail = JsonSerializer.Deserialize<TripDetail>(requestBody);

                var payload = new InputData
                {
                    Data = new object[,]
                    {
                        {
                            tripDetail.BranchID,
                            tripDetail.TrainID,
                            tripDetail.StationID,
                            tripDetail.CarID,
                            tripDetail.Month,
                            tripDetail.Day,
                            tripDetail.Hour,
                            0
                        }
                    }
                };

                //todo Store in AppSettings.json
                const string scoringUri = "http://d40cc896-5303-48f9-8d59-3335550a7be8.eastus.azurecontainer.io/score";

                var client = new HttpClient();
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, new Uri(scoringUri))
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(payload))
                    };
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = client.SendAsync(request).Result;
                    var responseBody = response.Content.ReadAsStringAsync().Result;
                    log.LogInformation("Response Body: " + responseBody);
                    responseBody = responseBody.Replace("\\\"", "\'");
                    log.LogInformation("New Response Body: " + responseBody);
                    responseBody = responseBody.Replace("\"", "");
                    log.LogInformation("New Response Body: " + responseBody);
                    dynamic r = JsonConvert.DeserializeObject(responseBody);
                    log.LogInformation("ML Result: " + tripDetail.TrainID);

                    return new OkObjectResult(tripDetail);
                }
                catch (Exception e)
                {
                    await Console.Out.WriteLineAsync(e.Message);
                }

                return new BadRequestObjectResult("Error While Running");
            }
            else
            {
                log.LogInformation("Method: " + req.Method + "was used to invoke the function");
                return new BadRequestObjectResult("Only POST can be used");
            }

        }
    }
}
