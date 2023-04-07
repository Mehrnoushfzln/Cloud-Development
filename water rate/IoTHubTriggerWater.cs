
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


namespace Water.Function
{
  /* a class for storing sensor data for further processing or analysis */
   public class WaterItem
  {
   	  [JsonProperty("id")]
  	  public string Id {get; set;}
  	  public double Water {get; set;}

   }

    public class IoTHubTriggerWater
    {
      
        private static HttpClient client = new HttpClient();

/*Defines "IoTHubTriggerWater" which specifies the name of the function as it should appear in the azure portal. 
which receives data from IoT Hub and store them in Cosmos DB continer with name "waters" . 
" out " shows it is an output value which is the "WaterItem" value. 
"log.information" line  show the sensor data that arrives into the hub in the terminal. */ 

        
        [FunctionName("IoTHubTriggerWater")]
        public static void Run([IoTHubTrigger("messages/events", Connection = "AzureEventHubConnectionString")] EventData message,
        [CosmosDB(databaseName: "IoTData",
                                 collectionName: "Waters",
                                 ConnectionStringSetting = "cosmosDBConnectionString")] out WaterItem output,
                       ILogger log)
        {
            log.LogInformation($"C# IoT Hub trigger function processed a message: {Encoding.UTF8.GetString(message.Body.Array)}");


/*obtains a message in json format into the variable jsonBody, deserializes the message into the variable data, 
and utilises the data variable, which is divided into two doubles, one for temperature and one for humidity.
The values obtained from the JSON message body are then put into an instance of the TemperatureItem class 
that is created at that point.
*/

     var jsonBody = Encoding.UTF8.GetString(message.Body);
     dynamic data = JsonConvert.DeserializeObject(jsonBody);
     double water = data.water;


     output = new WaterItem
     {
       Water = water
     };
        }

/* output value = initializes by new instance of the "TemperatureItem" class. */

/*GetWater Function uses an HTTP trigger to handle GET requests to the /water route. 
It also uses a Cosmos DB output binding to retrieve data from the "IoTData" database and "Waters" collection, based on a SQL query.*/

	[FunctionName("GetWater")]

      public static IActionResult GetWater(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "water/")] HttpRequest req,
        [CosmosDB(databaseName: "IoTData",
                  collectionName: "Waters",
                  ConnectionStringSetting = "cosmosDBConnectionString",
                      SqlQuery = "SELECT * FROM c")] IEnumerable<WaterItem> waterItem,
                  ILogger log)
      {
        return new OkObjectResult(waterItem);
      }


    }
}