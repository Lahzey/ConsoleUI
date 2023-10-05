using System.Text.RegularExpressions;

namespace ConsoleUI.Constraints;

public class ContainerConstraints
{
    public readonly int AutoWrapAfter;

    public ContainerConstraints(int autoWrapAfter)
    {
        this.AutoWrapAfter = autoWrapAfter;
    }

    public static ContainerConstraints parse(string formatting)
    {
        string[] elements = Regex.Replace(formatting, @"\s+", "").ToLower().Split(',');

        int autoWrapAfter = 0;
        foreach (string element in elements)
        {
            switch (true)
            {
                case bool _ when Regex.IsMatch(element, @"^wrap\d+$", RegexOptions.IgnoreCase):
                    autoWrapAfter = int.Parse(Regex.Match(element, @"\d+").Value);
                    break;
            }
        }

        return new ContainerConstraints(autoWrapAfter);
    }
}