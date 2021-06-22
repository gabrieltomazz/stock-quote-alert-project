using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Threading;


namespace StockQuoteAlertProject
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            // PETR4 28.95 26.90

            // verify args
            if (args.Length == 3)
            {
                AlertData alertData = new AlertData();
                alertData.stock = args[0];
                alertData.gain = Convert.ToDecimal(args[1]);
                alertData.loss = Convert.ToDecimal(args[2]);

                Console.WriteLine("Aplicação iniciada! ");
                RunAlertStock alert = new RunAlertStock();
                alert.run(alertData);

            }
            else
            {
                Console.WriteLine("Error! Fornece os parâmetros adequadamente! ");
            }
        }
    }

    // class to params 
    public class AlertData
    {

        public string stock { get; set; }
        public decimal gain { get; set; }
        public decimal loss { get; set; }

    }

    // stock price from API
    public class ResponseStockAPI
    {
        // last price
        public decimal c { get; set; }
    }

    // Api Data
    public class StockData
    {
        public bool status { get; set; }
        public int code { get; set; }
        public string msg { get; set; }
        public List<ResponseStockAPI> response { get; set; }
    }

    // connecting to API
    public class FCSConnection
    {
        private readonly string _apiKey;

        // constructor to API Key
        public FCSConnection(string apiKey)
        {
            this._apiKey = apiKey;
        }

        // function to get StockData from FCS API
        public decimal GetStockPrice(string stock)
        {
           
            // API Request
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://" + $@"fcsapi.com/api-v3/stock/latest?symbol={stock}&exchange=Real-time%20derived,brazil&access_key={this._apiKey}");
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            // convert JSson to a Object
            StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
            string results = sr.ReadToEnd();
            StockData stockData = JsonConvert.DeserializeObject<StockData>(results);
            sr.Close();

            decimal stockPrice = stockData.code == 200 && stockData.response.Count > 0 ? stockData.response[0].c : 0;
         
            return stockPrice;
        }
    }

    public class MailHelper
    {
        // public static IConfiguration AppSetting { get; }

        public void MailSender(string content)
        {
            //var builder = new ConfigurationBuilder()
            //.AddJsonFile("appsettings.json");
            //var config = builder.Build();
            // var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", true).AddEnvironmentVariables().Build()

            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("80ce30703e957b", "784f1f672a1977"),
                EnableSsl = true
            };
            client.Send("no-reply@stockalert.com", "gabrieltomazlima1@gmail.com", "Stock Quote Alert", content);
            Console.WriteLine("Alerta enviado!");
        }

    }

    public class RunAlertStock
    {
        public void run(AlertData alertData)
        {
            // method once every 20 seconds.
            Timer t = new Timer(TimerCallback, alertData, 1, 20000);
            Console.ReadLine();

        }

        private static void TimerCallback(Object alertData)
        {
            // Call method to build alert
            BuildAlert(alertData);

            // Force a garbage collection to occur for this demo.
            GC.Collect();
        }

        private static void BuildAlert(Object alert)
        {
            // cast object to AlertData
            AlertData alertData = (AlertData)alert;

            // init MailHelper
            MailHelper mail = new MailHelper();
            // init Api connection
            FCSConnection connect1 = new FCSConnection("it1lZ2rhzBCaMeV86Jfw4yFH");
            // Get stockData from API 
            decimal priceStock = connect1.GetStockPrice(alertData.stock);

            // verify alert
            if (priceStock > alertData.gain)
            {
                mail.MailSender($@" Alerta! Ação {alertData.stock} está cotada a R${priceStock} - VENDA! ");
            }
            else if (priceStock < alertData.loss )
            {
                mail.MailSender($@" Alerta! Ação {alertData.stock} está cotada a R${priceStock} - COMPRA! ");
            }

        }
    }

}
