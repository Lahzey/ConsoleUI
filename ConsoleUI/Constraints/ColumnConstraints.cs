using System.Text.RegularExpressions;

namespace ConsoleUI.Constraints;

public class ColumnConstraints : AxisConstraints {
	public ColumnConstraints(bool[] growingFlags, int[] defaultComponentOrientation) : base(growingFlags, defaultComponentOrientation) { }

	/// <summary>
	/// Parses a string containing grow flags and a list of component constraints within '[' and ']' for each column
	/// </summary>
	/// <param name="constraints">The constraints as string.</param>
	/// <returns>ColumnConstraints, containing all the parsed information.</returns>
	/// <exception cref="ArgumentException">If any non valid constraints are encountered.</exception>
	public static ColumnConstraints parse(string constraints) {
		Regex regex = new Regex(@"\[([^\]]*)\]");
		MatchCollection matches = regex.Matches(constraints);
		bool[] growingFlags = new bool[matches.Count];
		int[] defaultComponentOrientation = new int[matches.Count];

		int columnIndex = 0;
		foreach (Match match in matches) {
			string[] constraintArray = Regex.Replace(match.Groups[1].Value, @"\s+", "").ToLower().Split(',');
			defaultComponentOrientation[columnIndex] = ComponentConstraints.DEFAULT_X;
			foreach (string constraint in constraintArray) {
				switch (constraint) {
					case "grow":
						growingFlags[columnIndex] = true;
						break;
					case "fill":
						defaultComponentOrientation[columnIndex] = ComponentConstraints.FILL;
						break;
					case "center":
						defaultComponentOrientation[columnIndex] = ComponentConstraints.CENTER;
						break;
					case "left":
						defaultComponentOrientation[columnIndex] = ComponentConstraints.LEFT;
						break;
					case "right":
						defaultComponentOrientation[columnIndex] = ComponentConstraints.RIGHT;
						break;
					case "":
						// ignore
						break;
					default:
						throw new ArgumentException(constraint + " is not a valid formatting argument.");
				}
			}

			columnIndex++;
		}

		return new ColumnConstraints(growingFlags, defaultComponentOrientation);
	}
}