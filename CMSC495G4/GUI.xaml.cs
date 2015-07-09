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

        private double fromAmount = -1;
        private Currency fromCurrency = Currency.None;
        private Currency toCurrency = Currency.None;

        private ConversionEngine conversionEngine = null;

        //
        // Interface methods
        //
        public GUI()
        {
            conversionEngine = new ConversionEngine(this);

            GUIInitialize();
        }

        public void buttonConvert()
        {
            conversionEngine.updateToAmount();
        }

        public void buttonClear()
        {
            GUISetFromAmount("");
            GUISetFromCurrency(Currency.None);
            GUISetToCurrency(Currency.None);
        }

        public void fromAmountChanged()
        {
            conversionEngine.setFromAmount(fromAmount);
        }

        public void fromCurrencyChanged()
        {
            conversionEngine.setFromCurrency(fromCurrency);
        }

        public void toCurrencyChanged()
        {
            conversionEngine.setToCurrency(toCurrency);
        }

        public void updateDisplay()
        {
            double toAmount = conversionEngine.getToAmount();
            GUISetToAmount(toAmount >= 0 ? toAmount.ToString("N2") : "");

            GUISetStatus(conversionEngine.getStatus());
        }

        //
        // Windows Presentation Framework 
        //

        private string previousTBAmount;

        private void GUIInitialize()
        {
            InitializeComponent();
            previousTBAmount = tbAmount.Text;
        }

        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            buttonConvert();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            buttonClear();
        }

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

        private void cbFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Currency wasCurrency = fromCurrency;
            fromCurrency = (Currency)cbFrom.SelectedIndex;
            if (fromCurrency != wasCurrency) fromCurrencyChanged();
        }

        private void cbTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Currency wasCurrency = toCurrency;
            toCurrency = (Currency)cbTo.SelectedIndex;
            if (toCurrency != wasCurrency) toCurrencyChanged();
        }

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

        private static void LoadComboBox(ComboBox comboBox)
        {
            Currency[] currencies = (Currency[])Enum.GetValues(typeof(Currency));
            foreach (Currency currency in currencies)
            {
                string s = currency != Currency.None ? currency + " (" + GetDescription(currency) + ")" : "";
                comboBox.Items.Add(s);
            }
        }

        private void cbFrom_Loaded(object sender, RoutedEventArgs e)
        {
            LoadComboBox((ComboBox)sender);
        }

        private void cbTo_Loaded(object sender, RoutedEventArgs e)
        {
            LoadComboBox((ComboBox)sender);
        }

        private void GUISetFromCurrency(Currency fromCurrency)
        {
            cbFrom.SelectedIndex = (int)fromCurrency;
        }

        private void GUISetToCurrency(Currency toCurrency)
        {
            cbTo.SelectedIndex = (int)toCurrency;
        }

        private void GUISetFromAmount(string fromAmount)
        {
            tbAmount.Text = fromAmount;
        }

        private void GUISetToAmount(string toAmount)
        {
            tbEquals.Text = toAmount;
        }

        private void GUISetStatus(string status)
        {
            tbStatus.Text = status;
        }

        private void GUISetDebug(string debug)
        {
            lblDebug.Content = debug;
        }
    }
}
