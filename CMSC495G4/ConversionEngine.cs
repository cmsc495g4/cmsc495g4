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
 *      File: ConversionEngine.cs
 * 
 *  Contents: Class ConversionEngine - provides conversion functionality from one currency to another
 *
 *   History: Jul  8, 2015 - Christopher DeVault-Edmondson - initial build for testing
 *            Jul 12, 2015 - Christopher DeVault-Edmondson - added prompts, code structure
 *            Jul 19, 2015 - Christopher DeVault-Edmondson - added backup data source capability and reporting
 *            Jul 24, 2015 - Christopher DeVault-Edmondson - code documentation and cleanup
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSC495G4
{
    // model the ConversionEngine class - provide conversion functionality from one currency to another
    class ConversionEngine
    {
        //
        // Field variables
        //

        private GUI gui = null; // reference to the outer object passed during construction
        private DataProvider dataProvider = null; // reference to the inner object instantiated at construction

        private Currency fromCurrency = Currency.None; // conversion from which currency
        private Currency toCurrency = Currency.None; // conversion to which currency
        private double fromAmount = -1; // the amount to convert from (<0 = none)
        private double toAmount = -1; // the amount converted to (<0 = none)
        private double rate = 0; // the conversion to divided by from rate (<=0 = unknown)
        private string status = ""; // the status of the conversion ("" for none)

        //
        // Interface methods
        //

        // Constructor - save a reference to the GUI object and create a data provider
        public ConversionEngine(GUI gui)
        {
            this.gui = gui;

            dataProvider = new DataProvider(this);
        }

        // accessor setter method for fromCurrency - update the to amount if appropriate
        public void setFromCurrency(Currency fromCurrency)
        {
            Currency wasCurrency = this.fromCurrency;

            this.fromCurrency = fromCurrency;
            dataProvider.setFromCurrency(fromCurrency);

            if (this.fromCurrency != wasCurrency) updateToAmount();
        }

        // accessor setter method for toCurrency - update the to amount if appropriate
        public void setToCurrency(Currency toCurrency)
        {
            Currency wasCurrency = this.toCurrency;

            this.toCurrency = toCurrency;
            dataProvider.setToCurrency(toCurrency);

            if (this.toCurrency != wasCurrency) updateToAmount();
        }

        // accessor setter method for fromAmount - update the from amount if appropriate
        public void setFromAmount(double fromAmount)
        {
            double wasAmount = this.fromAmount;

            this.fromAmount = fromAmount;

            if (this.fromAmount != wasAmount) updateToAmount();
        }

        // accessor getter method for fromCurrency
        public Currency getFromCurrency()
        {
            return fromCurrency;
        }

        // accessor getter method for toCurrency
        public Currency getToCurrency()
        {
            return toCurrency;
        }

        // accessor getter method for fromAmount
        public double getFromAmount()
        {
            return fromAmount;
        }

        // accessor getter method for toAmount
        public double getToAmount()
        {
            return toAmount;
        }

        // accessor getter method for rate
        public double getRate()
        {
            return rate;
        }

        // accessor setter method for rate - update the to amount if appropriate
        public void setRate(double rate)
        {
            double wasRate = this.rate;

            this.rate = rate;

            if (this.rate != wasRate) updateToAmount();
        }

        // accessor getter method for status
        public string getStatus()
        {
            return status;
        }

        // update the to amount by extending the from amount using the rate, if all information is available
        // also update the status using information from the data provider, and update the display on the GUI
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
                if (dataProvider.getStatus() != "")
                {
                    status += " (" + dataProvider.getStatus() + ")";
                }
            }

            if ((toAmount != wasAmount) || (status != wasStatus)) gui.updateDisplay();
        }
    }
}
