using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace StockQuoteAlertProject
{
    class MainClass
    {
        public static void Main(string[] args)
        {
           
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

            if (stockData.code != 200 && !stockData.status)
            {
                Console.WriteLine("Erro! Algo deu errado!");
                Environment.Exit(0);
            }
         
            return stockData.response[0].c;
        }
    }

    public class MailConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string From { get; set; }
        public string Recipient { get; set; }
    }

    public class MailHelper
    {
        public void MailSender(string content)
        {
            // Read file appsettings.json 
            IConfiguration config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .Build();
            // Get content file
            var mailConfig = config.GetSection("Mail").Get<MailConfiguration>();

            // build Smtp Connection
            var client = new SmtpClient(mailConfig.Host, mailConfig.Port)
            {
                Credentials = new NetworkCredential(mailConfig.Username, mailConfig.Password),
                EnableSsl = true
            };

            // Send email
            client.Send(mailConfig.From, mailConfig.Recipient, "Stock Quote Alert", content);
            Console.WriteLine("Alerta enviado!");
        }

    }

    public class RunAlertStock
    {
        public void run(AlertData alertData)
        {
            // Method once every 20 seconds.
            Timer t = new Timer(TimerCallback, alertData, 1, 20000);
            Console.ReadLine();

        }

        private static void TimerCallback(Object alertData)
        {
            // Call method to build alert
            BuildAlert(alertData);

            // Force a garbage collection
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
            // get stockData from API 
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
