using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;


namespace APIReverseProxy
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            _ = app.UseRouting();

            _ = app.UseEndpoints(endpoints =>
            {
                _ = endpoints.MapPost("/v1", HandlePostRequest);
            });
        }

        private async Task HandlePostRequest(HttpContext context)
        {
            try
            {
                // Read the incoming POST request body
                using StreamReader reader = new(context.Request.Body);
                string requestBody = await reader.ReadToEndAsync();
                Console.Write(requestBody);

                // Validate the JSON-RPC request
                JObject newRequest = ValidateRpcRequest(requestBody);

                if (newRequest["error"] != null)
                {
                    Console.Write(newRequest);
                    await context.Response.WriteAsync(newRequest.ToString(Newtonsoft.Json.Formatting.None));
                    await context.Response.CompleteAsync();
                    return;
                }
                Console.Write("DA FUCK AM I DOING HERE! LET ME OUT!");
                // Make HTTP requests to multiple API endpoints
                await ProcessApiRequests(context, newRequest);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync($"Error: {ex.Message}");
            }
        }

        private static async Task ProcessApiRequests(HttpContext context, JObject newRequest)
        {
            List<Task<string>> apiTasks = new();
            string? InfuraApiKey = Environment.GetEnvironmentVariable("INFURA_API_KEY");
            string? AlchemyApiKey = Environment.GetEnvironmentVariable("ALCHEMY_API_KEY");

            Task<string> infuraRequest = MakeHttpClientPostRequestAsync("https://mainnet.infura.io/v3/" + InfuraApiKey, newRequest.ToString());
            Task<string> alchemyRequest = MakeHttpClientPostRequestAsync("https://eth-mainnet.g.alchemy.com/v2/" + AlchemyApiKey, newRequest.ToString());
            
            apiTasks.Add(infuraRequest);
            apiTasks.Add(alchemyRequest);

            while (apiTasks.Count > 0)
            {
                Task<string> completedTask = await Task.WhenAny(apiTasks);
                string result = await completedTask;

                if (completedTask.Status == TaskStatus.RanToCompletion)
                {
                    // Return the result of the first successful task
                    context.Response.ContentType = "application/json";
                    JObject JsonObject = JObject.Parse(result);
                    if (JsonObject["result"] != null)
                    {
                        await context.Response.WriteAsync(result);
                        await context.Response.CompleteAsync();
                    }
                }
                else
                {
                    // Remove the completed or failed task from the list
                    _ = apiTasks.Remove(completedTask);
                }
            }
        }

        private static async Task<string> MakeHttpClientPostRequestAsync(string url, string requestBody)
        {
            try
            {
                using var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(url),
                    Headers =
                    {
                        { "accept", "application/json" },
                    },
                    Content = new StringContent(requestBody)
                    {
                        Headers =
                        {
                            ContentType = new MediaTypeHeaderValue("application/json")
                        }
                    }
                };

                using var response = await client.SendAsync(request);
                _ = response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"{{ \"error\": \"{ex.Message}\" }}";
            }
        }

        private static JObject ValidateRpcRequest(string requestBody)
        {
            try
            {
                // Parse the JSON-RPC request
                var jsonRequest = JObject.Parse(requestBody);
                string? method = jsonRequest["method"]?.ToString();
                JToken parameters = jsonRequest["params"];
                string? id = jsonRequest["id"]?.ToString();
                string? jsonrpc = jsonRequest["jsonrpc"]?.ToString();

                if (method != "eth_gasPrice")
                {
                    return new JObject(new JProperty("error", $"Method {method} not supported!"));
                }

                var newRequest = new JObject(
                    new JProperty("jsonrpc", jsonrpc),
                    new JProperty("method", method),
                    new JProperty("params", parameters),
                    new JProperty("id", id)
                );

                return newRequest;
            }
            catch (Exception ex)
            {
                return new JObject(new JProperty("error", ex.Message));
            }
        }
    }
}
