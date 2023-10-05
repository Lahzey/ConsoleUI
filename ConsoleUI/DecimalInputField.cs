using System.Globalization;

namespace ConsoleUI;

/// <summary>
/// An input field that only allows decimals.
/// </summary>
public class DecimalInputField : ConsoleInputField
{
    
    public DecimalInputField(decimal value) : base(value + "")
    {
        InputFilter = (c) =>
        {
            if (char.IsDigit(c))
            {
                return true;
            }
            else if ((c == '.' || c == ',') && !(Content.Contains('.') || Content.Contains(',')))
            {
                return true;
            }

            return false;
        };

    }

    public decimal GetInput()
    {
        String content = Content;
        if (content.Length == 0)
        {
            return 0;
        }
        
        if (content.StartsWith('.') || content.StartsWith(','))  // add leading 0 if missing
        {
            content = '0' + content;
        }
        
        if (content.EndsWith('.') || content.EndsWith(','))  // add trailing 0 if missing
        {
            content += '0';
        }
        
        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        ci.NumberFormat.CurrencyDecimalSeparator = content.Contains('.') ? "." : ",";
        return decimal.Parse(content, NumberStyles.Any, ci); // makes sure '.' works as a separator, see https://stackoverflow.com/a/1014575
    }
    
}