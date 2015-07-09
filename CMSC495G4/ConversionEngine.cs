using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSC495G4
{
    class ConversionEngine
    {
        private GUI gui = null;
        private DataProvider dataProvider = null;

        private Currency fromCurrency = Currency.None;
        private Currency toCurrency = Currency.None;
        private double fromAmount = -1;
        private double toAmount = -1;
        private double rate = 0;
        private string status = "";

        public ConversionEngine(GUI gui)
        {
            this.gui = gui;

            dataProvider = new DataProvider(this);
        }

        public void setFromCurrency(Currency fromCurrency)
        {
            Currency wasCurrency = this.fromCurrency;

            this.fromCurrency = fromCurrency;
            dataProvider.setFromCurrency(fromCurrency);

            if (this.fromCurrency != wasCurrency) updateToAmount();
        }

        public void setToCurrency(Currency toCurrency)
        {
            Currency wasCurrency = this.toCurrency;

            this.toCurrency = toCurrency;
            dataProvider.setToCurrency(toCurrency);

            if (this.toCurrency != wasCurrency) updateToAmount();
        }

        public void setFromAmount(double fromAmount)
        {
            double wasAmount = this.fromAmount;

            this.fromAmount = fromAmount;

            if (this.fromAmount != wasAmount) updateToAmount();
        }

        public Currency getFromCurrency()
        {
            return fromCurrency;
        }

        public Currency getToCurrency()
        {
            return toCurrency;
        }

        public double getFromAmount()
        {
            return fromAmount;
        }

        public double getToAmount()
        {
            return toAmount;
        }

        public double getRate()
        {
            return rate;
        }

        public void setRate(double rate)
        {
            double wasRate = this.rate;

            this.rate = rate;

            if (this.rate != wasRate) updateToAmount();
        }

        public string getStatus()
        {
            return status;
        }

        public void updateToAmount()
        {
            double wasAmount = toAmount;
            string wasStatus = status;

            if ((fromCurrency == Currency.None) || (toCurrency == Currency.None) || (fromAmount < 0) || (rate <= 0))
            {
                toAmount = -1;
                status = dataProvider.getStatus();
            }
            else
            {
                toAmount = fromAmount * rate;
                status = "Conversion Rate: 1 " + fromCurrency + " = " + rate.ToString("N5") + " " + toCurrency;
            }

            if ((toAmount != wasAmount) || (status != wasStatus)) gui.updateDisplay();
        }
    }
}
