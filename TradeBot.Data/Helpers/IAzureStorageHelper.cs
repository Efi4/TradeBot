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


namespace TradeBot.Data.Helpers;

public interface IAzureStorageHelper
{
    public Task<string> UploadBlobAsync(string blobName, Stream content, bool overwrite = true);
    public Task<Stream> DownloadBlobAsync(string blobName);
    public Task<List<string>> ListBlobsAsync();
    public Task DeleteBlobAsync(string blobName);
    public Task PushToTradeDealsQueueEncodedAsync(EquipmentQueueMessageModel equipment);
    public Task PushToNotificationsQueueEncodedAsync(string message);
    public Task PushToRegionTransferNotificationsQueueEncodedAsync(string message);
    public Task<ItemResponseModel?> ReadFromTradeDealsQueueAsync();
    public Task<IEnumerable<QueueMessage>?> ReadFromQueueAsync(int maxMessages);
    public Task DeleteFromQueueAsync(string messageId, string popReceipt);
}