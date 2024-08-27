using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Helper;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Security.KeyVault.Secrets;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Azure.Amqp.Framing;
using Google.Protobuf.Reflection;

namespace ProcessMessageFunc
{
	public class ProcessMessageFunc
	{
		private readonly ILogger _logger;
		static readonly HttpClient mHttpClient;
		private static readonly int mResubmitMaxCount = Convert.ToInt32(AzureConfigurationManager.GetSetting("resubmit_MaxCount"));
		private static readonly int mResubmitInterval = Convert.ToInt32(AzureConfigurationManager.GetSetting("resubmit_Interval"));
		private static readonly bool mResubmitExponential = Convert.ToBoolean(AzureConfigurationManager.GetSetting(
			"resubmit_ExponentialBackoff"));
		private static readonly bool mIsManagedIdentityEnabled = Convert.ToBoolean
			(AzureConfigurationManager.GetSetting("EnableMI") ?? "false");

		static readonly ServiceBusClient mServiceBusClient;
        static readonly ServiceBusSender mRescheduleQueueSender;


		// Constructor to initialize logger and other dependencies
		public ProcessMessageFunc(ILoggerFactory loggerFactory)
		{
			_logger = loggerFactory.CreateLogger<ProcessMessageFunc>();
		}

		static ProcessMessageFunc()
		{
			mServiceBusClient = mIsManagedIdentityEnabled ? new ServiceBusClient(
				AzureConfigurationManager.GetConnection("ServiceBusConnectionString:fullyQualifiedNamespace"), new DefaultAzureCredential())
			: new ServiceBusClient(AzureConfigurationManager.GetSetting("ServiceBusConnectionString"));

			mRescheduleQueueSender = mServiceBusClient.CreateSender(AzureConfigurationManager.GetSetting(""));
		}

		[Function("ProcessMessageFunc")]
		public async Task Run([ServiceBusTrigger("myqueue", Connection = "ServiceBusConnectionString")] 
		ServiceBusReceivedMessage myQueueItem, ServiceBusMessageActions messageActions,
			FunctionContext context)
		{
			ServiceBusMessage serviceBusMessage = myQueueItem.ConvertToServiceBusMessage();

			try
			{
				_logger.LogInformation($"Entering into ProcessMessage.Run method at {DateTime.UtcNow}");
				string rawbody = serviceBusMessage.ConvertMessageBodyToString();

				// Process message
				//await ProcessMessage(message);
			}
			catch (Exception ex)
			{
				int retryCount = myQueueItem.ApplicationProperties.ContainsKey("RetryCount") ?
				(int)myQueueItem.ApplicationProperties["RetryCount"] : 0;

				if (retryCount < mResubmitMaxCount)
				{
					
					_logger.LogError(ex, "An error occurred while processing the message.");
					await serviceBusMessage.Resubmit(mServiceBusClient.CreateSender("myqueue"), 60, "RetryCount", true);
				}
				else
				{
					_logger.LogError(ex, "An error occurred while processing the message. Max retry count reached.");
					await messageActions.DeadLetterMessageAsync(myQueueItem);

				}
			}
		}

		private static bool ValidateandParseJson(ServiceBusMessage message,string rawBody,
			ILogger logger,bool mIsManagedIdentityEnabled)
		{
			if (string.IsNullOrEmpty(rawBody))
			{
				logger.LogError($"The message file recieved is Empty {DateTime.UtcNow}");
				return false;
			}
			else if (true)
			{
				return false;
			}
			else
			{
				logger.LogError($"The message file recieved is Empty {DateTime.UtcNow}");
				return false;
			}
		}
	}
}
