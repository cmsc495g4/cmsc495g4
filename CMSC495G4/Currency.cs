using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSC495G4
{
    enum Currency
    {
        [Description("None")]
        None,
        [Description("Swiss Franc")]
        CHF,
        [Description("Euro")]
        EUR,
        [Description("British Pound")]
        GBP,
        [Description("Japanese Yen")]
        JPY,
        [Description("U.S. Dollar")]
        USD,
        //[Description("Canadian Dollar")]
        //CAD,
        //[Description("Australian Dollar")]
        //AUD 
    };
}
