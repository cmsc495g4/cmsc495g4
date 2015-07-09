using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSC495G4
{
    class DataQuote
    {
        private Currency fromCurrency;
        private Currency toCurrency;
        private double bidRate;
        private double askRate;
        private long timeStamp;

        public DataQuote(Currency fromCurrency, Currency toCurrency, double bidRate, double askRate, long timeStamp)
        {
            this.fromCurrency = fromCurrency;
            this.toCurrency = toCurrency;
            this.bidRate = bidRate;
            this.askRate = askRate;
        }

        public Currency getFromCurrency()
        {
            return fromCurrency;
        }

        public Currency getToCurrency()
        {
            return toCurrency;
        }

        public double getBidRate()
        {
            return bidRate;
        }

        public double getAskRate()
        {
            return askRate;
        }

        public long getTimeStamp()
        {
            return timeStamp;
        }
    }
}
