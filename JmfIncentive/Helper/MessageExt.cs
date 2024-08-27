using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace Helper
{
	public static class MessageExt
	{
		public static ServiceBusMessage ConvertToServiceBusMessage(this ServiceBusReceivedMessage receivedMessage)
		{
			var serviceBusMessage = new ServiceBusMessage(receivedMessage.Body)
			{
				ContentType = receivedMessage.ContentType,
				CorrelationId = receivedMessage.CorrelationId,
				MessageId = receivedMessage.MessageId,
				Subject = receivedMessage.Subject,
				To = receivedMessage.To,
				ReplyTo = receivedMessage.ReplyTo,
				ReplyToSessionId = receivedMessage.ReplyToSessionId,
				SessionId = receivedMessage.SessionId,
				TimeToLive = receivedMessage.TimeToLive,
				PartitionKey = receivedMessage.PartitionKey,
				TransactionPartitionKey = receivedMessage.TransactionPartitionKey,
				ScheduledEnqueueTime = receivedMessage.ScheduledEnqueueTime
			};

			foreach (var property in receivedMessage.ApplicationProperties)
			{
				serviceBusMessage.ApplicationProperties.Add(property.Key, property.Value);
			}

			return serviceBusMessage;
		}
		public static async Task Resubmit(this ServiceBusMessage originalMessage, ServiceBusSender sender, int delayInSeconds,
			string retryCounterPropertyKey, bool exponentialBackoff)
		{
			ServiceBusMessage clonedMessage = CloneAndIncrementRetry(originalMessage, delayInSeconds, retryCounterPropertyKey);
			await sender.SendMessageAsync(clonedMessage);
		}

		private static ServiceBusMessage CloneAndIncrementRetry(ServiceBusMessage originalMessage, int delayInSeconds,
			string retryCounterPropertyKey, bool exponentialBackoff = false)
		{
			var clonedMessage = new ServiceBusMessage(originalMessage.Body)
			{
				ContentType = originalMessage.ContentType,
				CorrelationId = originalMessage.CorrelationId,
				MessageId = Guid.NewGuid().ToString(),
				Subject = originalMessage.Subject,
				To = originalMessage.To
			};

			int currRetryCount = 1;

			if (originalMessage.ApplicationProperties.ContainsKey(retryCounterPropertyKey))
			{
				int.TryParse(originalMessage.ApplicationProperties[retryCounterPropertyKey].ToString(), out currRetryCount);
				currRetryCount += 1;
			}

			clonedMessage.ApplicationProperties[retryCounterPropertyKey] = currRetryCount;

			int totalDelay = delayInSeconds;

			if (exponentialBackoff)
			{
				totalDelay = delayInSeconds * (int)Math.Pow(2, currRetryCount - 1);
			}

			clonedMessage.ScheduledEnqueueTime = DateTime.UtcNow.AddSeconds(totalDelay);

			return clonedMessage;
		}

		public static string ConvertMessageBodyToString(this ServiceBusMessage receivedMessage)
		{
			if (receivedMessage != null && receivedMessage.Body != null)
			{
				return Encoding.UTF8.GetString(receivedMessage.Body);
			}

			return string.Empty; // Return an empty string if receivedMessage or its Body is null
		}
	}
}
