using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CMSC495G4
{
    class DataProvider
    {
        //
        // Field variables
        //

        private const double updateMilliseconds = 500;
        private const string dataURL = "http://webrates.truefx.com/rates/connect.html?f=csv";
        private const string backupURL = "http://www.google.com/finance?q=";
        private const string backupMarker = "<span class=bld>";

        private static List<DataProvider> dataProviderList = new List<DataProvider>();

        private ConversionEngine conversionEngine = null;

        private Currency fromCurrency = Currency.None;
        private Currency toCurrency = Currency.None;
        private double rate = 0;
        private List<DataQuote> dataQuotes = new List<DataQuote>();
        private string status = "";

        private static System.Timers.Timer timer;

        //
        // Interface methods
        //

        public DataProvider(ConversionEngine conversionEngine)
        {
            this.conversionEngine = conversionEngine;

            updateDataQuotes();

            if (timer == null)
            {
                timer = new System.Timers.Timer(updateMilliseconds);
                timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
                timer.Enabled = true;
            }

            dataProviderList.Add(this);
        }

        ~DataProvider()
        {
            dataProviderList.Remove(this);

            if ((timer != null) && (dataProviderList.Count <= 0))
            {
                timer.Enabled = false;
                timer.Dispose();
            }
        }

        public void setFromCurrency(Currency fromCurrency)
        {
            Currency wasCurrency = this.fromCurrency;

            this.fromCurrency = fromCurrency;

            if (this.fromCurrency != wasCurrency) updateRate();
        }

        public void setToCurrency(Currency toCurrency)
        {
            Currency wasCurrency = this.toCurrency;

            this.toCurrency = toCurrency;

            if (this.toCurrency != wasCurrency) updateRate();
        }

        public Currency getFromCurrency()
        {
            return fromCurrency;
        }

        public Currency getToCurrency()
        {
            return toCurrency;
        }

        public double getRate()
        {
            return rate;
        }

        public void updateDataQuotes()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(dataURL);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                response.Close();
                readStream.Close();

                dataQuotes.Clear();
                string[] lines = data.Split('\n');
                foreach (string line in lines)
                {
                    string[] fields = line.Split(',');
                    string pair = fields.Length >= 1 ? fields[0] : "";
                    string timestr = fields.Length >= 2 ? fields[1] : "";
                    string bidbig = fields.Length >= 3 ? fields[2] : "";
                    string bidpts = fields.Length >= 4 ? fields[3] : "";
                    string askbig = fields.Length >= 5 ? fields[4] : "";
                    string askpts = fields.Length >= 6 ? fields[5] : "";
                    string bidstr = bidbig + bidpts;
                    string askstr = askbig + askpts;
                    long tsnum = 0;
                    bool tssuccess = long.TryParse(timestr, out tsnum);
                    if (tsnum <= 0) tssuccess = false;
                    double bidnum = 0;
                    double asknum = 0;
                    bool bidsuccess = double.TryParse(bidstr, out bidnum);
                    if (bidsuccess && (bidnum <= 0)) bidsuccess = false;
                    bool asksuccess = double.TryParse(askstr, out asknum);
                    if (asksuccess && (asknum <= 0)) asksuccess = false;
                    string curr1 = "";
                    string curr2 = "";
                    if (pair.Length == 7)
                    {
                        if (pair[3] == '/')
                        {
                            curr1 = pair.Substring(0, 3).ToUpper().Trim();
                            if (curr1.Length != 3) curr1 = "";
                            curr2 = pair.Substring(4, 3).ToUpper().Trim();
                            if (curr2.Length != 3) curr2 = "";
                        }
                    }
                    Currency[] currencies = (Currency[])Enum.GetValues(typeof(Currency)).Cast<Currency>();
                    Currency currency1 = Currency.None;
                    Currency currency2 = Currency.None;
                    foreach (Currency currency in currencies)
                    {
                        if (currency.ToString() == curr1)
                        {
                            currency1 = currency;
                            break;
                        }
                    }
                    foreach (Currency currency in currencies)
                    {
                        if (currency.ToString() == curr2)
                        {
                            currency2 = currency;
                            break;
                        }
                    }

                    if ((currency1 != Currency.None) && (currency2 != Currency.None) && bidsuccess && asksuccess && tssuccess)
                    {
                        dataQuotes.Add(new DataQuote(currency1, currency2, bidnum, asknum, tsnum));
                    }
                }

                updateRate();
            }
        }

        public void timerTick()
        {
            updateDataQuotes();
        }

        public void updateRate()
        {
            double wasRate = rate;
            string wasStatus = status;

            if ((fromCurrency == Currency.None) || (toCurrency == Currency.None))
            {
                rate = 0;
                status = "";
            }
            else if (fromCurrency == toCurrency)
            {
                rate = 1;
                status = "";
            }
            else
            {
                rate = scanRate(fromCurrency, toCurrency, true);
                if (rate <= 0)
                {
                    rate = backupRate(fromCurrency, toCurrency);
                    status = rate > 0 ? "backup" : fromCurrency + " to " + toCurrency + " unavailable";
                }
                else
                {
                    status = rate > 0 ? "" : fromCurrency + " to " + toCurrency + " unavailable";
                }
            }

            if ((rate != wasRate) || (status != wasStatus)) conversionEngine.setRate(rate);
        }

        public string getStatus()
        {
            return status;
        }

        private double scanRate(Currency fromCurrency, Currency toCurrency, bool Recurse)
        {
            if ((fromCurrency == Currency.None) || (toCurrency == Currency.None)) return 0;

            foreach (DataQuote dataQuote in dataQuotes)
            {
                Currency quoteFromCurrency = dataQuote.getFromCurrency();
                Currency quoteToCurrency = dataQuote.getToCurrency();
                double quoteBidRate = dataQuote.getBidRate();
                double quoteAskRate = dataQuote.getAskRate();

                if ((quoteFromCurrency == Currency.None) || (quoteToCurrency == Currency.None) || (quoteBidRate <= 0) || (quoteAskRate <= 0)) continue;

                if ((quoteFromCurrency == fromCurrency) && (quoteToCurrency == toCurrency))
                {
                    return quoteBidRate;
                }
                else if ((quoteToCurrency == fromCurrency) && (quoteFromCurrency == toCurrency))
                {
                    return 1.0 / quoteBidRate;
                }
                else if ((quoteFromCurrency == fromCurrency) && Recurse)
                {
                    double secRate = scanRate(quoteToCurrency, toCurrency, false);
                    if (secRate > 0) return quoteBidRate * secRate;
                }
                else if ((quoteToCurrency == fromCurrency) && Recurse)
                {
                    double secRate = scanRate(quoteFromCurrency, toCurrency, false);
                    if (secRate > 0) return quoteBidRate / secRate;
                }
            }

            return 0;
        }

        private double backupRate(Currency fromCurrency, Currency toCurrency)
        {
            string url = backupURL + fromCurrency + toCurrency;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                response.Close();
                readStream.Close();

                dataQuotes.Clear();
                string[] lines = data.Split('\n');
                foreach (string line in lines)
                {
                    int i1 = line.IndexOf(backupMarker);
                    if (i1 > 0) 
                    {
                        string s = line.Substring(i1 + backupMarker.Length).Trim();
                        int i2 = s.IndexOf(" ");
                        if (i2 > 0)
                        {
                            s = s.Substring(0, i2);
                            double r = 0;
                            bool success = double.TryParse(s, out r);
                            if (success && (r > 0))
                            {
                                return r;
                            }
                        }
                    }
                }
            }

            return 0;
        }

        //
        // Windows Presentation Framework 
        //

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            foreach (DataProvider dataProvider in dataProviderList)
            {
                //dataProvider.timerTick();
            }
        }
    }
}
