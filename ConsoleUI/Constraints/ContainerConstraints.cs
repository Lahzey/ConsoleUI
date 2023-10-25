using System.Text.RegularExpressions;

namespace ConsoleUI.Constraints;

public class ContainerConstraints {
	public readonly int AutoWrapAfter;

	public ContainerConstraints(int autoWrapAfter) {
		AutoWrapAfter = autoWrapAfter;
	}

	public static ContainerConstraints Parse(string formatting) {
		string[] elements = Regex.Replace(formatting, @"\s+", "").ToLower().Split(',');

		int autoWrapAfter = 0;
		foreach (string element in elements) {
			if (Regex.IsMatch(element, @"^wrap\d+$", RegexOptions.IgnoreCase))
				autoWrapAfter = int.Parse(Regex.Match(element, @"\d+").Value);
		}

		return new ContainerConstraints(autoWrapAfter);
	}
}