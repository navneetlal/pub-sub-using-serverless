using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Publisher
{

  public class Function
  {

    private static readonly HttpClient client = new HttpClient();

    private static async Task<string> GetCallingIP()
    {
      client.DefaultRequestHeaders.Accept.Clear();
      client.DefaultRequestHeaders.Add("User-Agent", "AWS Lambda .Net Client");

      var msg = await client.GetStringAsync("http://checkip.amazonaws.com/").ConfigureAwait(continueOnCapturedContext: false);

      return msg.Replace("\n", "");
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
    {
      var location = await GetCallingIP();
      var body = new Dictionary<string, string>
            {
                { "message", "hello world" },
                { "location", location }
            };

      await PublishMessageAsync(JsonConvert.SerializeObject(body));

      return new APIGatewayProxyResponse
      {
        Body = JsonConvert.SerializeObject(body),
        StatusCode = 200,
        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
      };
    }

    public async Task PublishMessageAsync(String msg)
    {
      var client = new AmazonSimpleNotificationServiceClient();
      var topicArn = Environment.GetEnvironmentVariable("TOPIC_ARN");

      var request = new PublishRequest
      {
        Message = msg,
        TopicArn = topicArn
      };

      try
      {
        var response = await client.PublishAsync(request);

        Console.WriteLine("Message sent to topic:");
        Console.WriteLine(JsonConvert.SerializeObject(response));
      }
      catch (Exception ex)
      {
        Console.WriteLine("Caught exception publishing request:");
        Console.WriteLine(ex.Message);
      }
    }

  }
}
