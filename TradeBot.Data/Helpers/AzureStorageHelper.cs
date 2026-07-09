using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Core.Pipeline;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using TradeBot.Base.Models;
using TradeBot.Base.Configuration;
using TradeBot.Base;

namespace TradeBot.Data.Helpers;

/// <summary>
/// Helper class for Azure Storage operations
/// </summary>
public class AzureStorageHelper : IAzureStorageHelper
{
    private readonly BlobContainerClient _containerClient;
    private readonly QueueClient _tradeDealsQueueClient;
    private readonly QueueClient _notificationQueueClient;
    private readonly QueueClient _regionTransferNotificationsQueueClient;

    public AzureStorageHelper()
    {
        var azureConnectionString = EnvironmentConfiguration.GetAzureStorageConnectionString();
        var blobServiceClient = new BlobServiceClient(azureConnectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(Constants.AzureStorageConfiguration.BlobContainerName);
        
        var queueServiceClient = new QueueServiceClient(azureConnectionString);
        _tradeDealsQueueClient = queueServiceClient.GetQueueClient(Constants.AzureStorageConfiguration.TradeDealsQueueName);
        _notificationQueueClient = queueServiceClient.GetQueueClient(Constants.AzureStorageConfiguration.NotificationsQueueName);
        _regionTransferNotificationsQueueClient = queueServiceClient.GetQueueClient(Constants.AzureStorageConfiguration.RegionTransferNotificationsQueueName);
    }

    /// <summary>
    /// Uploads content to blob storage
    /// </summary>
    public async Task<string> UploadBlobAsync(string blobName, Stream content, bool overwrite = true)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(content, overwrite: overwrite);
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to upload blob '{blobName}'", ex);
        }
    }

    /// <summary>
    /// Downloads blob content
    /// </summary>
    public async Task<Stream> DownloadBlobAsync(string blobName)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            var download = await blobClient.DownloadAsync();
            return download.Value.Content;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to download blob '{blobName}'", ex);
        }
    }

    /// <summary>
    /// Lists all blobs in the container
    /// </summary>
    public async Task<List<string>> ListBlobsAsync()
    {
        var blobs = new List<string>();
        try
        {
            await foreach (var item in _containerClient.GetBlobsAsync())
            {
                blobs.Add(item.Name);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to list blobs", ex);
        }
        return blobs;
    }

    /// <summary>
    /// Deletes a blob
    /// </summary>
    public async Task DeleteBlobAsync(string blobName)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.DeleteAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete blob '{blobName}'", ex);
        }
    }

    /// <summary>
    /// Pushes an encoded message to the queue
    /// </summary>
    public async Task PushToTradeDealsQueueEncodedAsync(EquipmentQueueMessageModel equipmentMessage)
    {
        try
        {
            if (_tradeDealsQueueClient == null)
            {
                throw new InvalidOperationException("Queue client not initialized. Initialize with queue name in constructor.");
            }
            string jsonMessage = JsonSerializer.Serialize(equipmentMessage);
            string base64Encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonMessage));
            await _tradeDealsQueueClient.SendMessageAsync(base64Encoded);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to push a message in the trade-deals queue", ex);   
        }
    }

    /// <summary>
    /// Pushes a message to the queue
    /// </summary>
    public async Task PushToNotificationsQueueEncodedAsync(string message)
    {
        try
        {
            if (_notificationQueueClient == null)
            {
                throw new InvalidOperationException($"{nameof(_notificationQueueClient)} Queue client not initialized. Initialize with queue name in constructor.");
            }
            Console.WriteLine($"Pushing message to notifications queue: {message}");
            string base64Encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(message));
            await _notificationQueueClient.SendMessageAsync(base64Encoded);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to push a message in the notifications queue", ex);   
        }
    }

    /// <summary>
    /// Pushes a message to the queue
    /// </summary>
    public async Task PushToRegionTransferNotificationsQueueEncodedAsync(string message)
    {
        try
        {
            if (_regionTransferNotificationsQueueClient == null)
            {
                throw new InvalidOperationException($"{nameof(_regionTransferNotificationsQueueClient)} Queue client not initialized. Initialize with queue name in constructor.");
            }
            string base64Encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(message));
            await _regionTransferNotificationsQueueClient.SendMessageAsync(base64Encoded);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to push a message in the region transfer notifications queue", ex);   
        }
    }

    /// <summary>
    /// Reads (receives) a message from the queue
    /// </summary>
    public async Task<ItemResponseModel?> ReadFromTradeDealsQueueAsync()
    {
        try
        {
            if (_tradeDealsQueueClient == null)
            {
                throw new InvalidOperationException("Queue client not initialized. Initialize with queue name in constructor.");
            }
            var message = await _tradeDealsQueueClient.ReceiveMessageAsync();
            if(message == null || message.Value == null)
            {
                throw new InvalidOperationException("No messages available in the queue");
            }
            
            return JsonSerializer.Deserialize<ItemResponseModel>(message?.Value.Body);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to read message from queue", ex);
        }
    }

    /// <summary>
    /// Reads multiple messages from the queue
    /// </summary>
    public async Task<IEnumerable<QueueMessage>?> ReadFromQueueAsync(int maxMessages)
    {
        try
        {
            if (_tradeDealsQueueClient == null)
            {
                throw new InvalidOperationException("Queue client not initialized. Initialize with queue name in constructor.");
            }
            var messages = await _tradeDealsQueueClient.ReceiveMessagesAsync(maxMessages);
            if(messages == null || messages.Value == null)
            {
                throw new InvalidOperationException("No messages available in the queue");
            }
            return messages?.Value;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to read messages from queue", ex);
        }
    }

    /// <summary>
    /// Deletes a message from the queue
    /// </summary>
    public async Task DeleteFromQueueAsync(string messageId, string popReceipt)
    {
        try
        {
            if (_tradeDealsQueueClient == null)
            {
                throw new InvalidOperationException("Queue client not initialized. Initialize with queue name in constructor.");
            }
            await _tradeDealsQueueClient.DeleteMessageAsync(messageId, popReceipt);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to delete message from queue", ex);
        }
    }
}
