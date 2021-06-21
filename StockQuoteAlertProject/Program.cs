using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace StockQuoteAlertProject
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            AVConnection connect1 = new AVConnection("it1lZ2rhzBCaMeV86Jfw4yFH");
            StockData stock =  connect1.GetStockPrice("PETR3");
            Console.WriteLine(stock.response[0].c);
        }
    }

    public class ResponseStockAPI
    {
        // last price
        public string c { get; set; }
    }

    public class StockData
    {
        public bool status { get; set; }
        public int code { get; set; }
        public string msg { get; set; }
        public List<ResponseStockAPI> response { get; set; }
    }

    public class AVConnection
    {
        private readonly string _apiKey;

        public AVConnection(string apiKey)
        {
            this._apiKey = apiKey;
        }

        public StockData GetStockPrice(string stock)
        {
            // https://fcsapi.com/api-v3/stock/latest?symbol=PETR3&exchange=Real-time%20derived,brazil&access_key=it1lZ2rhzBCaMeV86Jfw4yFH
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://" + $@"fcsapi.com/api-v3/stock/latest?symbol={stock}&exchange=Real-time%20derived,brazil&access_key={this._apiKey}");
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            Console.WriteLine(resp.GetResponseStream());

            StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
            string results = sr.ReadToEnd();
            StockData myDeserializedClass = JsonConvert.DeserializeObject<StockData>(results);
            Console.WriteLine(myDeserializedClass);
            sr.Close();

            return myDeserializedClass;
        }
    }
}
