using System.Text.RegularExpressions;

namespace ConsoleUI.Constraints;

public class RowConstraints : AxisConstraints {
	public RowConstraints(bool[] growingFlags, int[] defaultComponentOrientation) : base(growingFlags, defaultComponentOrientation) { }

	/// <summary>
	/// Parses a string containing grow flags and a list of component constraints within '[' and ']' for each row
	/// </summary>
	/// <param name="constraints">The constraints as string.</param>
	/// <returns>RowConstraints, containing all the parsed information.</returns>
	/// <exception cref="ArgumentException">If any non valid constraints are encountered.</exception>
	public static RowConstraints Parse(string constraints) {
		Regex regex = new Regex(@"\[([^\]]*)\]");
		MatchCollection matches = regex.Matches(constraints);
		bool[] growingFlags = new bool[matches.Count];
		int[] defaultComponentOrientation = new int[matches.Count];

		int rowIndex = 0;
		foreach (Match match in matches) {
			string[] constraintArray = Regex.Replace(match.Groups[1].Value, @"\s+", "").ToLower().Split(',');
			defaultComponentOrientation[rowIndex] = ComponentConstraints.DEFAULT_Y;
			foreach (string constraint in constraintArray) {
				switch (constraint) {
					case "grow":
						growingFlags[rowIndex] = true;
						break;
					case "fill":
						defaultComponentOrientation[rowIndex] = ComponentConstraints.FILL;
						break;
					case "center":
						defaultComponentOrientation[rowIndex] = ComponentConstraints.CENTER;
						break;
					case "top":
						defaultComponentOrientation[rowIndex] = ComponentConstraints.TOP;
						break;
					case "bottom":
						defaultComponentOrientation[rowIndex] = ComponentConstraints.BOTTOM;
						break;
					case "":
						// ignore
						break;
					default:
						throw new ArgumentException(constraint + " is not a valid formatting argument.");
				}
			}

			rowIndex++;
		}

		return new RowConstraints(growingFlags, defaultComponentOrientation);
	}
}