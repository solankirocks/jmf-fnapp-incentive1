//using System;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Threading.Tasks;
//using Helper;
//using Microsoft.Azure.Functions.Worker;
//using Microsoft.Extensions.Logging;
//using Azure.Identity;
//using Azure.Messaging.ServiceBus;
//using Azure.Security.KeyVault.Secrets;
//using Newtonsoft.Json;
//using System.Text;

//namespace ProcessMessageFunc
//{
//	public class MessageFunc
//	{
//		private readonly ILogger _logger;
//		private readonly ServiceBusClient _serviceBusClient;
//		private readonly HttpClient _httpClient;
//		private const int MaxRetryAttempts = 3;

//		public MessageFunc(ILoggerFactory loggerFactory)
//		{
//			_logger = loggerFactory.CreateLogger<ProcessMessageFunc>();

//			// Initialize Key Vault client and fetch secrets
//			var keyVaultName = Environment.GetEnvironmentVariable("KeyVaultName");
//			var kvUri = $"https://{keyVaultName}.vault.azure.net";
//			var credential = new DefaultAzureCredential();
//			var client = new SecretClient(new Uri(kvUri), credential);

//			// Fetch secrets from Key Vault
//			var serviceBusConnectionString = client.GetSecret("ServiceBusConnectionString").Value.Value;
//			var clientSecret = client.GetSecret("D365ClientSecret").Value.Value;

//			// Initialize Service Bus client
//			_serviceBusClient = new ServiceBusClient(serviceBusConnectionString);

//			// Fetch environment variables for D365
//			var d365ClientId = Environment.GetEnvironmentVariable("D365ClientId");
//			var d365TenantId = Environment.GetEnvironmentVariable("D365TenantId");
//			var d365Url = Environment.GetEnvironmentVariable("D365Url");

//			// Initialize HttpClient for Dataverse OData endpoint
//			_httpClient = new HttpClient
//			{
//				BaseAddress = new Uri(d365Url)
//			};

//			// Set authorization header with access token
//			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAccessToken(d365ClientId, clientSecret, d365TenantId).Result);
//			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//		}

//		[Function("ProcessMessageFunc")]
//		public async Task Run([ServiceBusTrigger("myqueue", Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message,
//			FunctionContext context)
//		{
//			var log = context.GetLogger<ProcessMessageFunc>();
//			log.LogInformation($"C# ServiceBus queue trigger function processed message: {message.Body}");

//			ServiceBusMessage serviceBusMessage = message.ConvertToServiceBusMessage();

//			try
//			{
//				// Validate and parse data
//				if (ValidateandParseJson(serviceBusMessage, out var validData))
//				{
//					// Transform and Write data
//					bool result = await TransformData(validData, serviceBusMessage, log);

//					if (!result)
//					{
//						log.LogError("Failed to process message after retries.");
//					}

//				}

//			}
//			catch (Exception ex)
//			{
//				log.LogError(ex, "An error occurred while processing the message.");

//			}
//		}

//		private async Task<string> GetAccessToken(string clientId, string clientSecret, string tenantId)
//		{
//			try
//			{
//				// Acquire token from Azure AD
//				var authority = $"https://login.microsoftonline.com/{tenantId}";
//				var confidentialClient = Microsoft.Identity.Client.ConfidentialClientApplicationBuilder
//					.Create(clientId)
//					.WithClientSecret(clientSecret)
//					.WithAuthority(new Uri(authority))
//					.Build();

//				var result = await confidentialClient.AcquireTokenForClient(new[] { "https://dynamics-instance.crm.dynamics.com/.default" }).ExecuteAsync();
//				return result.AccessToken;
//			}
//			catch (Exception ex)
//			{
//				_logger.LogError($"Error acquiring token: {ex.Message}");
//				throw;
//			}
//		}

//		private static bool ValidateandParseJson(ServiceBusMessage message, out MessageDataModel data)
//		{
//			try
//			{
//				// Attempt to deserialize the message body
//				data = JsonConvert.DeserializeObject<MessageDataModel>(message.Body.ToString())!;
//				return true;
//			}
//			catch (JsonException ex)
//			{
//				// Log the exception if needed
//				data = null;
//				return false;
//			}
//		}

//		private async Task<bool> TransformData(MessageDataModel data, ServiceBusMessage message, ILogger log)
//		{
//			// Implement your transformation logic
//			string result = JsonConvert.SerializeObject(data);
//			// Write data to Dataverse
//			return await WriteToCEAsync(result, message, log);
//		}

//		private async Task<bool> WriteToCEAsync(string data, ServiceBusMessage message, ILogger log)
//		{
//			const int maxRetries = 5;
//			int retryCount = message.ApplicationProperties.ContainsKey("RetryCount") ?
//				(int)message.ApplicationProperties["RetryCount"] : 0;

//			while (retryCount < maxRetries)
//			{
//				try
//				{
//					// Send data to Dataverse
//					var content = new StringContent(data, Encoding.UTF8, "application/json");
//					var response = await _httpClient.PostAsync("/api/data/v9.1/your_entity_name", content);
//					var sender = _serviceBusClient.CreateSender("myqueue");

//					if (response.IsSuccessStatusCode)
//					{
//						log.LogInformation("Data successfully written to Dataverse.");
//						return true;
//					}
//					else if (IsTransientError(response.StatusCode))
//					{
//						// Handle transient errors with retry
//						retryCount++;
//						log.LogWarning("HTTP {StatusCode} detected. Retry attempt {RetryCount} of {MaxRetries}.",
//							response.StatusCode, retryCount, maxRetries);

//						// Resubmit the message with updated retry count and delay
//						await message.Resubmit(sender, 5, "RetryCount", true); // Initial delay is 5 seconds

//						return false; // Exit the loop and let the resubmitted message handle further retries
//					}
//					else
//					{
//						// Log non-transient errors
//						var responseContent = await response.Content.ReadAsStringAsync();
//						await message.Resubmit(sender, 5, "RetryCount", false); // Initial delay is 5 seconds
//						log.LogError("Failed to write data to Dataverse. Status Code: {StatusCode}, Reason: {ReasonPhrase}, " +
//							"Content: {ResponseContent}", response.StatusCode, response.ReasonPhrase, responseContent);
//						return false;
//					}
//				}
//				catch (HttpRequestException ex)
//				{
//					// Handle HttpRequestException with retry
//					retryCount++;
//					log.LogWarning("HttpRequestException detected. Retry attempt {RetryCount} of {MaxRetries}. Exception: {Exception}",
//								   retryCount, maxRetries, ex.Message);

//					// Resubmit the message with updated retry count and delay
//					var sender = _serviceBusClient.CreateSender("myqueue");
//					await message.Resubmit(sender, 5, "RetryCount", true); // Initial delay is 5 seconds

//					return false; // Exit the loop and let the resubmitted message handle further retries
//				}

//			}

//			// Max retry attempts reached
//			log.LogError("Max retry attempts reached. Failed to write data to Dataverse due to repeated errors.");
//			///write code to dead letter the message

//			return false;
//		}

//		private static bool IsTransientError(System.Net.HttpStatusCode statusCode)
//		{
//			// Define transient errors
//			return statusCode == (System.Net.HttpStatusCode)429 || // Too Many Requests
//				   statusCode == (System.Net.HttpStatusCode)500 || // Internal Server Error
//				   statusCode == (System.Net.HttpStatusCode)502 || // Bad Gateway
//				   statusCode == (System.Net.HttpStatusCode)503 || // Service Unavailable
//				   statusCode == (System.Net.HttpStatusCode)504;   // Gateway Timeout
//		}
//	}

//	public class MessageDataModel
//	{
//		public string Property1 { get; set; }
//		public string Property2 { get; set; }
//	}
//}
