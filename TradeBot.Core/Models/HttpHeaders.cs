using System.Collections.Generic;

namespace TradeBot.Core.Models;

public class RequestData
{
    public required string BaseUrl { get; set; }
    public required string BaseBatchUrl { get; set; }
    public required Dictionary<string, string> HttpHeadersDictionary { get; set; }
}