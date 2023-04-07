/* c# libraries */

using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.CosmosDB;

namespace Temp.function
{

/* a class for storing sensor data for further processing or analysis */
    public class TemperatureItem
    {
        [JsonProperty("id")]
        public string Id {get; set;}
        public double Temperature {get; set;}
        public double Humidity {get; set;}
    }

/* defines an Iot hub trigger function that is triggered by messages sent to an IoT Hub */

    public class myIoTHubTrigger
    {   
        private static HttpClient client = new HttpClient();

/*Defines "myIoTHubTrigger" which specifies the name of the function as it should appear in the azure portal. 
which receives data from IoT Hub and store them in Cosmos DB continer with name "temperatures" . 
" out " shows it is an output value which is the "TemperatureItem" value. 
"log.information" line  show the sensor data that arrives into the hub in the terminal. */            
        
        [FunctionName("myIoTHubTrigger")]
        public static void Run([IoTHubTrigger("messages/events", Connection = "AzureEventHubConnectionString")] EventData message,
        [CosmosDB(databaseName: "IoTData",
                                 collectionName: "Temperatures",
                                 ConnectionStringSetting = "cosmosDBConnectionString")] out TemperatureItem output,
                       ILogger log)
        {
            log.LogInformation($"C# IoT Hub trigger function processed a message: {Encoding.UTF8.GetString(message.Body.Array)}");

/*retrieves the body of the message object received by IoTHubTrigger function. "Encoding.UTF8.GetString" is used to convert this byte array to a
 string representation of the message payload, assuming that the payload is encoded in UTF-8 format.
obtains a message in json format into the variable jsonBody, deserializes the message into the variable data, 
and utilises the data variable, which is divided into two doubles, one for temperature and one for humidity.
The values obtained from the JSON message body are then put into an instance of the TemperatureItem class 
that is created at that point.*/


       var jsonBody = Encoding.UTF8.GetString(message.Body);
       dynamic data = JsonConvert.DeserializeObject(jsonBody);
       double temperature = data.temperature;
       double humidity = data.humidity;


/*output value = initializes by new instance of the "TemperatureItem" class. */

        output = new TemperatureItem
            {            
                Temperature = temperature,              
                 Humidity = humidity
            };


        }
	    [FunctionName("GetTemperature")]

        public static IActionResult GetTemperature(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "temperature/")] HttpRequest req,
           [CosmosDB(databaseName: "IoTData",
                   collectionName: "Temperatures",
                   ConnectionStringSetting = "cosmosDBConnectionString",
                        SqlQuery = "SELECT * FROM c")] IEnumerable<TemperatureItem> temperatureItem,
                   ILogger log)
       {
         return new OkObjectResult(temperatureItem);
	   }
    }
}
