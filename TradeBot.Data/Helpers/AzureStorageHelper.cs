using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace TradeBot.Data.Helpers;

/// <summary>
/// Helper class for Azure Storage operations
/// </summary>
public class AzureStorageHelper
{
    private readonly BlobContainerClient _containerClient;
    private readonly QueueClient _queueClient;

    public AzureStorageHelper(string connectionString, string containerName, string queueName)
    {
        var blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        
        var queueServiceClient = new QueueServiceClient(connectionString);
        _queueClient = queueServiceClient.GetQueueClient(queueName);
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
    /// Pushes a message to the queue
    /// </summary>
    public async Task PushToQueueAsync(string message)
    {
        try
        {
            if (_queueClient == null)
            {
                throw new InvalidOperationException("Queue client not initialized. Initialize with queue name in constructor.");
            }
            await _queueClient.SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to push message to queue", ex);
        }
    }

    /// <summary>
    /// Reads (receives) a message from the queue
    /// </summary>
    public async Task<QueueMessage> ReadFromQueueAsync()
    {
        try
        {
            if (_queueClient == null)
            {
                throw new InvalidOperationException("Queue client not initialized. Initialize with queue name in constructor.");
            }
            var message = await _queueClient.ReceiveMessageAsync();
            if(message == null || message.Value == null)
            {
                throw new InvalidOperationException("No messages available in the queue");
            }
            return message?.Value;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to read message from queue", ex);
        }
    }

    /// <summary>
    /// Reads multiple messages from the queue
    /// </summary>
    public async Task<IEnumerable<QueueMessage>> ReadFromQueueAsync(int maxMessages)
    {
        try
        {
            if (_queueClient == null)
            {
                throw new InvalidOperationException("Queue client not initialized. Initialize with queue name in constructor.");
            }
            var messages = await _queueClient.ReceiveMessagesAsync(maxMessages);
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
            if (_queueClient == null)
            {
                throw new InvalidOperationException("Queue client not initialized. Initialize with queue name in constructor.");
            }
            await _queueClient.DeleteMessageAsync(messageId, popReceipt);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to delete message from queue", ex);
        }
    }
}
