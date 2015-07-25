/*
 *  CMSC 495 6980 Current Trends and Projects in Computer Science (2155)
 *  Prof. Hung Dao
 * 
 *  Group 4: Christopher DeVault-Edmondson, Zebider Firde, Leah Rojesky
 * 
 *  Project: Currency Converter
 *  
 */

/*
 *  Solution: CMSC495G4
 *  
 *      File: DataProvider.cs
 * 
 *  Contents: Class DataProvider - provides functionality to retrieve live conversion quotes from the web
 *
 *   History: Jul  8, 2015 - Christopher DeVault-Edmondson - initial build for testing
 *            Jul 12, 2015 - Christopher DeVault-Edmondson - added prompts and code structure
 *            Jul 19, 2015 - Christopher DeVault-Edmondson - added backup data source capability and reporting
 *            Jul 23, 2015 - Christopher DeVault-Edmondson - fixed issue with quote to base conversions
 *            "              "                             - removed temporary console outputs
 *            Jul 24, 2015 - Christopher DeVault-Edmondson - code documentation and cleanup
 */

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
    // model the DataProvider class - retrieve live conversion quotes from the web
    class DataProvider
    {
        // 
        // Constants
        //

        private const double updateMilliseconds = 500; // how frequently to update the display?
        private const string dataURL = "http://webrates.truefx.com/rates/connect.html?f=csv"; // primary data source URL
        private const string backupURL = "http://www.google.com/finance?q="; // backup data source URL base
        private const string backupMarker = "<span class=bld>"; // backup data source data result marker

        //
        // Field variables
        //

        private static List<DataProvider> dataProviderList = new List<DataProvider>(); // static list of data providers for timer

        private ConversionEngine conversionEngine = null; // conversion engine reference from construction

        private Currency fromCurrency = Currency.None; // currency to convert from
        private Currency toCurrency = Currency.None; // currency to convert to
        private double rate = 0; // conversion rate determined, or <=0 for none/unknown
        private List<DataQuote> dataQuotes = new List<DataQuote>(); // list of primary data source quotes that are known
        private string status = ""; // status from conversion

        private static System.Timers.Timer timer; // singleton static timer instance to update all data provider instances

        //
        // Interface methods
        //

        // constructor - save conversion engine reference, make initial primary data request, start the timer (if not already) and add this instance to the list
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

        // destructor - remove this instance from the list, and if it's the last instance, stop the timer 
        ~DataProvider()
        {
            dataProviderList.Remove(this);

            if ((timer != null) && (dataProviderList.Count <= 0))
            {
                timer.Enabled = false;
                timer.Dispose();
            }
        }

        // setter accessor method for fromCurrency - update the rate if appropriate
        public void setFromCurrency(Currency fromCurrency)
        {
            Currency wasCurrency = this.fromCurrency;

            this.fromCurrency = fromCurrency;

            if (this.fromCurrency != wasCurrency) updateRate();
        }

        // setter accessor method for toCurrency - update the rate if appropriate
        public void setToCurrency(Currency toCurrency)
        {
            Currency wasCurrency = this.toCurrency;

            this.toCurrency = toCurrency;

            if (this.toCurrency != wasCurrency) updateRate();
        }

        // getter accessor method for fromCurrency
        public Currency getFromCurrency()
        {
            return fromCurrency;
        }

        // getter accessor method for toCurrency
        public Currency getToCurrency()
        {
            return toCurrency;
        }

        // getter accessor method for rate
        public double getRate()
        {
            return rate;
        }

        // update the primary list of known data quotes from the primary provider
        public void updateDataQuotes()
        {
            // make a request from the primary data source URL
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(dataURL);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // only if we successfully received a response, receive it and process
            if (response.StatusCode == HttpStatusCode.OK)
            {
                // retrieve a stream of the response in whatever character set it is provider in
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

                // read the entire stream contents
                string data = readStream.ReadToEnd();

                // clean up the response and the stream
                response.Close();
                readStream.Close();

                // clear the list of known quotes, split the response by line feed and iterate the lines
                dataQuotes.Clear();
                string[] lines = data.Split('\n');
                foreach (string line in lines)
                {
                    // split the CSV lines into each field and break down the contents;
                    // for numeric values, attempt to parse to a valid number; 
                    // for currency values, attempt to parse to a supported currency code e.g. EUR
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

                    // after parsing each field, if all are known and available, add the quote to the list of known quotes
                    if ((currency1 != Currency.None) && (currency2 != Currency.None) && bidsuccess && asksuccess && tssuccess)
                    {
                        dataQuotes.Add(new DataQuote(currency1, currency2, bidnum, asknum, tsnum));
                    }
                }

                // now that the list of known quotes has been updated, update the rate with this new information
                updateRate();
            }
        }

        // on each tick of the timer for this instance, update the known quote list from the live primary data source
        public void timerTick()
        {
            updateDataQuotes();
        }

        // update the rate using the known primary data source if possible; if not possible but from and to currencies are
        // known, then try to get the corresponding information from the secondary data source
        public void updateRate()
        {
            double wasRate = rate;
            string wasStatus = status;

            // if either from or to currency is unknown the rate is unknowable
            if ((fromCurrency == Currency.None) || (toCurrency == Currency.None))
            {
                rate = 0;
                status = "";
            }
            // if it is from a currency to itself, the rate is obviously 1:1
            else if (fromCurrency == toCurrency)
            {
                rate = 1;
                status = "";
            }
            // otherwise, scan the primary rate quote table to look for the answer
            else
            {
                rate = scanRate(fromCurrency, toCurrency, true);
                // if that fails, check the backup data source and if that fails, report failure
                if (rate <= 0)
                {
                    rate = backupRate(fromCurrency, toCurrency);
                    status = rate > 0 ? "backup" : fromCurrency + " to " + toCurrency + " unavailable";
                }
                // report the result from the primary - no status indicates success with no comment
                else
                {
                    status = "";
                }
            }

            // if the rate has ultimately been updated, update the conversion engine
            if ((rate != wasRate) || (status != wasStatus)) conversionEngine.setRate(rate);
        }

        // accessor method to return the status
        public string getStatus()
        {
            return status;
        }

        // recursively scan the primary rate quote table
        private double scanRate(Currency fromCurrency, Currency toCurrency, bool Recurse)
        {
            // if either rate is unknown we cannot do this
            if ((fromCurrency == Currency.None) || (toCurrency == Currency.None)) return 0;

            // first, try to match a forwards or backwards quote with a single step
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
                    return 1.0 / quoteAskRate;
                }
            }

            // if that fails and this is the first step, try to get halfway there and match the second half with a second step
            if (Recurse)
            {
                foreach (DataQuote dataQuote in dataQuotes)
                {
                    Currency quoteFromCurrency = dataQuote.getFromCurrency();
                    Currency quoteToCurrency = dataQuote.getToCurrency();
                    double quoteBidRate = dataQuote.getBidRate();
                    double quoteAskRate = dataQuote.getAskRate();

                    if ((quoteFromCurrency == Currency.None) || (quoteToCurrency == Currency.None) || (quoteBidRate <= 0) || (quoteAskRate <= 0)) continue;

                    if (quoteFromCurrency == fromCurrency) 
                    {
                        double secRate = scanRate(quoteToCurrency, toCurrency, false);
                        if (secRate > 0) return quoteBidRate * secRate;
                    }
                    else if (quoteToCurrency == fromCurrency) 
                    {
                        double secRate = scanRate(quoteFromCurrency, toCurrency, false);
                        if (secRate > 0) return secRate / quoteAskRate;
                    }
                }
            }

            // if that fails also, return that we cannot know the rate from the information we have
            return 0;
        }

        // retrieve a spot quote conversion rate from the backup data source
        private double backupRate(Currency fromCurrency, Currency toCurrency)
        {
            // compose a URL request for this specific currency as a spot request
            string url = backupURL + fromCurrency + toCurrency;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // only if we get a successful response, retrieve and parse it
            if (response.StatusCode == HttpStatusCode.OK)
            {
                // receive the whole response into a stream of the right encoding
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

                // read the entire stream contents
                string data = readStream.ReadToEnd();

                // clean up the response and stream
                response.Close();
                readStream.Close();

                // clear the primary quotes since they do not contain the answer, and parse the lines we did receive
                dataQuotes.Clear();
                string[] lines = data.Split('\n');
                foreach (string line in lines)
                {
                    // check each line for the marker indicating the answer
                    int i1 = line.IndexOf(backupMarker);
                    if (i1 > 0) 
                    {
                        // if we find that, scan until the next space
                        string s = line.Substring(i1 + backupMarker.Length).Trim();
                        int i2 = s.IndexOf(" ");
                        if (i2 > 0)
                        {
                            // get the data after the marker and before the space, and convert it to a double rate
                            s = s.Substring(0, i2);
                            double r = 0;
                            bool success = double.TryParse(s, out r);
                            if (success && (r > 0))
                            {
                                // if it's a valid positive number, return the answer
                                return r;
                            }
                        }
                    }
                }
            }

            // if all else fails, return that we could not find the answer from the backup data source
            return 0;
        }

        //
        // Windows Presentation Framework 
        //

        // when the static timer fires, trigger the timer on each instance to update the GUI with live information
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            foreach (DataProvider dataProvider in dataProviderList)
            {
                dataProvider.timerTick();
            }
        }
    }
}
