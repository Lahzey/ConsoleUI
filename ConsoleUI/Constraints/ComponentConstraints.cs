using System.Text.RegularExpressions;

namespace ConsoleUI.Constraints;

public class ComponentConstraints {
	public static readonly int LEFT = 0;
	public static readonly int TOP = 0;
	public static readonly int CENTER = 1;
	public static readonly int RIGHT = 2;
	public static readonly int BOTTOM = 2;
	public static readonly int FILL = 3;

	public static readonly int DEFAULT_X = LEFT;
	public static readonly int DEFAULT_Y = TOP;

	public readonly int OrientationX;
	public readonly int OrientationY;

	public ComponentConstraints(int orientationX, int orientationY) {
		OrientationX = orientationX;
		OrientationY = orientationY;
	}

	/// <summary>
	/// Parses a string containing various constraints for an individual component within a container.
	/// </summary>
	/// <param name="constraints">The constraints as string</param>
	/// <param name="defaultXOrientation">The default x-orientation to be used if not overwritten by the constraints.</param>
	/// <param name="defaultYOrientation">The default y-orientation to be used if not overwritten by the constraints.</param>
	/// <returns>ComponentConstraints containing all the parsed information.</returns>
	/// <exception cref="ArgumentException">If any non valid constraints are encountered.</exception>
	public static ComponentConstraints Parse(string constraints, int defaultXOrientation, int defaultYOrientation) {
		string[] elements = Regex.Replace(constraints, @"\s+", "").ToLower().Split(',');

		int orientationX = defaultXOrientation;
		int orientationY = defaultYOrientation;

		foreach (string element in elements) {
			switch (element) {
				case "fill":
					orientationX = FILL;
					orientationY = FILL;
					break;
				case "fillx":
					orientationX = FILL;
					break;
				case "filly":
					orientationY = FILL;
					break;
				case "center":
					orientationX = CENTER;
					orientationY = CENTER;
					break;
				case "centerx":
					orientationX = CENTER;
					break;
				case "centery":
					orientationY = CENTER;
					break;
				case "left":
					orientationX = LEFT;
					break;
				case "right":
					orientationX = RIGHT;
					break;
				case "top":
					orientationY = TOP;
					break;
				case "bottom":
					orientationY = BOTTOM;
					break;
				case "":
					// ignore
					break;
				default:
					throw new ArgumentException(element + " is not a valid formatting argument.");
			}
		}

		return new ComponentConstraints(orientationX, orientationY);
	}
}