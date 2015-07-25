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
 *      File: DataQuote.cs
 * 
 *  Contents: Class DataQuote - provides a model of a single conversion quote from one currency to another
 *
 *   History: Jul  8, 2015 - Christopher DeVault-Edmondson - initial build for testing
 *            Jul 12, 2015 - Christopher DeVault-Edmondson - added prompts and code structure
 *            Jul 24, 2015 - Christopher DeVault-Edmondson - code documentation and cleanup
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSC495G4
{
    // DataQuote class to model a single conversion quote
    class DataQuote
    {
        private Currency fromCurrency; // currency from
        private Currency toCurrency; // currency to
        private double bidRate; // bid rate (to buy the pair)
        private double askRate; // ask rate (to sell the pair)
        private long timeStamp; // timestamp of the quote

        // construction - save all of the provided information
        public DataQuote(Currency fromCurrency, Currency toCurrency, double bidRate, double askRate, long timeStamp)
        {
            this.fromCurrency = fromCurrency;
            this.toCurrency = toCurrency;
            this.bidRate = bidRate;
            this.askRate = askRate;
            this.timeStamp = timeStamp;
        }

        // accessor for fromCurrency
        public Currency getFromCurrency()
        {
            return fromCurrency;
        }

        // accessor for toCurrency
        public Currency getToCurrency()
        {
            return toCurrency;
        }

        // accessor for bidRate
        public double getBidRate()
        {
            return bidRate;
        }

        // accessor for askRate
        public double getAskRate()
        {
            return askRate;
        }

        // accessor for timeStamp
        public long getTimeStamp()
        {
            return timeStamp;
        }
    }
}
