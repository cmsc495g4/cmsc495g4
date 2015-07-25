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
 *      File: Currency.cs
 * 
 *  Contents: Class Currency - enumeration of supported currencies
 *
 *   History: Jul  8, 2015 - Christopher DeVault-Edmondson - initial build for testing
 *            Jul 24, 2015 - Christopher DeVault-Edmondson - code documentation and cleanup
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSC495G4
{
    // enumerate all of the possible currencies including no currency selected; also provide descriptions for each
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
    };
}
