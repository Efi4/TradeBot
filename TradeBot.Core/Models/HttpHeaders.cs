using System.Collections.Generic;

namespace TradeBot.Core.Models;

public class RequestData
{
    public string BaseUrl { get; set; } = string.Empty;
    public Dictionary<string, string> HttpHeadersDictionary { get; set; } = new Dictionary<string, string>();
}