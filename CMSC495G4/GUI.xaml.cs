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
 *      File: GUI.xaml.cs
 * 
 *  Contents: Class GUI - provides the Windows Presentation Framework graphical user interface 
 *
 *   History: Jul  8, 2015 - Christopher DeVault-Edmondson - initial build for testing
 *            Jul 12, 2015 - Christopher DeVault-Edmondson - added prompts and code structure
 *            Jul 24, 2015 - Christopher DeVault-Edmondson - code documentation and cleanup
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CMSC495G4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class GUI : Window
    {
        //
        // Field variables
        //

        private double fromAmount = -1; // from amount input from the user
        private Currency fromCurrency = Currency.None; // from currency input from the user
        private Currency toCurrency = Currency.None; // to currency input from the user

        private ConversionEngine conversionEngine = null; // conversion engine instance at construction

        //
        // Interface methods
        //
        
        // constructor - create the conversion engine, initialize the GUI and set up the display
        public GUI()
        {
            conversionEngine = new ConversionEngine(this);

            GUIInitialize();

            updateDisplay();
        }

        // when user presses convert button, update the amount immediately even if timer has not fired
        public void buttonConvert()
        {
            conversionEngine.updateToAmount();
        }

        // when user presses clear, reset all inputs to defaults
        public void buttonClear()
        {
            GUISetFromAmount("");
            GUISetFromCurrency(Currency.None);
            GUISetToCurrency(Currency.None);
        }

        // when the from amount has changed, update the display and conversion engine
        public void fromAmountChanged()
        {
            updateDisplay();
            conversionEngine.setFromAmount(fromAmount);
        }

        // when the from currency has changed, update the display and conversion engine
        public void fromCurrencyChanged()
        {
            updateDisplay();
            conversionEngine.setFromCurrency(fromCurrency);
        }

        // when the to currency has changed, update the display and conversion engine
        public void toCurrencyChanged()
        {
            updateDisplay();
            conversionEngine.setToCurrency(toCurrency);
        }
        
        // when the display needs to be updated, display the converted amount if known and any appropriate message
        public void updateDisplay()
        {
            // display the to amount if we know it
            double toAmount = conversionEngine.getToAmount();
            GUISetToAmount(toAmount >= 0 ? toAmount.ToString("N2") : "");

            // determine which user inputs are known
            bool haveFromAmount = fromAmount >= 0;
            bool haveFromCurrency = fromCurrency != Currency.None;
            bool haveToCurrency = toCurrency != Currency.None;
            bool haveAll = haveFromAmount && haveFromCurrency && haveToCurrency;

            // compose an appropriate prompt if not all inputs are known
            string message = "";
            if (!haveAll)
            {
                message = "Welcome to Currency Converter. Please select";
                int num = 0;
                int exp = 0 + (!haveFromAmount ? 1 : 0) + (!haveFromCurrency ? 1 : 0) + (!haveToCurrency ? 1 : 0);
                if (!haveFromAmount) { message += (num > 0 && exp > 2 ? "," : "") + (num == exp - 1 && exp >= 2 ? " and" : "") + " from amount"; num++; }
                if (!haveFromCurrency) { message += (num > 0 && exp > 2 ? "," : "") + (num == exp - 1 && exp >= 2 ? " and" : "") + " from currency"; num++; }
                if (!haveToCurrency) { message += (num > 0 && exp > 2 ? "," : "") + (num == exp - 1 && exp >= 2 ? " and" : "") + " to currency"; num++; }
                message += ".";
            }
            // if they are all known use the prompt from the conversion engine
            else
            {
                message = conversionEngine.getStatus();
            }
            
            // update the WPF controls with this composed information     
            GUISetStatus(message);
        }

        //
        // Windows Presentation Framework - these methods are the raw interface to Windows
        //

        // this is the previous input amount for comparison
        private string previousTBAmount;

        // at initialization, set up the components and the previous amount
        private void GUIInitialize()
        {
            InitializeComponent();
            previousTBAmount = tbAmount.Text;
        }

        // handle convert button click by passing up
        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            buttonConvert();
        }

        // handle the clear button click by passing up
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            buttonClear();
        }

        // when the text box for from amount changes, validate it as a two decimal place currency amount before accepting
        private void tbAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            double wasAmount = fromAmount;

            if (string.IsNullOrEmpty(((TextBox)sender).Text))
            {
                previousTBAmount = "";
                fromAmount = -1;
                if (fromAmount != wasAmount) fromAmountChanged();
            }
            else
            {
                bool success = true;
                string txt = ((TextBox)sender).Text;

                int periods = 0;
                for (int i = 0; i < txt.Length; i++)
                {
                    if ((txt[i] < '0') || (txt[i] > '9'))
                    {
                        if ((txt[i] == '.') && (periods == 0) && (i > 0) && (i >= txt.Length - 3))
                            periods++;
                        else
                        {
                            success = false;
                            break;
                        }
                    }
                }

                double num = 0;
                if (success) success = double.TryParse(((TextBox)sender).Text, out num);

                if (success && (num >= 0))
                {
                    ((TextBox)sender).Text.Trim();
                    previousTBAmount = ((TextBox)sender).Text;
                    fromAmount = num;
                    if (fromAmount != wasAmount) fromAmountChanged();
                }
                else
                {
                    ((TextBox)sender).Text = previousTBAmount;
                    ((TextBox)sender).SelectionStart = ((TextBox)sender).Text.Length;
                }
            }
        }

        // when from currency changes, pass up the information
        private void cbFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Currency wasCurrency = fromCurrency;
            fromCurrency = (Currency)cbFrom.SelectedIndex;
            if (fromCurrency != wasCurrency) fromCurrencyChanged();
        }

        // when to currency changes, pass up the information
        private void cbTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Currency wasCurrency = toCurrency;
            toCurrency = (Currency)cbTo.SelectedIndex;
            if (toCurrency != wasCurrency) toCurrencyChanged();
        }

        // pass up the description of an enumerated currency type e.g. EUR -> Euro
        private static string GetDescription(Enum en)
        {
            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            return en.ToString();
        }

        // load the combo boxes with the list of possible supported currencies
        private static void LoadComboBox(ComboBox comboBox)
        {
            Currency[] currencies = (Currency[])Enum.GetValues(typeof(Currency));
            foreach (Currency currency in currencies)
            {
                string s = currency != Currency.None ? currency + " (" + GetDescription(currency) + ")" : "";
                comboBox.Items.Add(s);
            }
        }

        // when the from box is loaded, set up the selections
        private void cbFrom_Loaded(object sender, RoutedEventArgs e)
        {
            LoadComboBox((ComboBox)sender);
        }

        // when the to box is loaded, set up the selections
        private void cbTo_Loaded(object sender, RoutedEventArgs e)
        {
            LoadComboBox((ComboBox)sender);
        }

        // allow setting the from currency combo box
        private void GUISetFromCurrency(Currency fromCurrency)
        {
            cbFrom.SelectedIndex = (int)fromCurrency;
        }

        // allow setting the to currency combo box
        private void GUISetToCurrency(Currency toCurrency)
        {
            cbTo.SelectedIndex = (int)toCurrency;
        }

        // allow setting the from amount
        private void GUISetFromAmount(string fromAmount)
        {
            tbAmount.Text = fromAmount;
        }

        // allow setting the to amount 
        private void GUISetToAmount(string toAmount)
        {
            tbEquals.Text = toAmount;
        }

        // allow setting the status display
        private void GUISetStatus(string status)
        {
            tbStatus.Text = status;
        }
    }
}
