using System;
using System.Net;

public class WebClientWithTimeout : WebClient
{
    public int Timeout { get; set; } = 10000; // default 10s

    protected override WebRequest GetWebRequest(Uri address)
    {
        WebRequest request = base.GetWebRequest(address);
        if (request != null)
        {
            request.Timeout = Timeout;
            if (request is HttpWebRequest httpRequest)
            {
                httpRequest.ReadWriteTimeout = Timeout;
            }
        }
        return request;
    }
}