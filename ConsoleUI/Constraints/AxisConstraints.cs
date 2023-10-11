namespace ConsoleUI.Constraints;

public class AxisConstraints {
	private readonly bool[] growingFlags;
	private readonly int[] defaultComponentOrientation;

	protected AxisConstraints(bool[] growingFlags, int[] defaultComponentOrientation) {
		this.growingFlags = growingFlags;
		this.defaultComponentOrientation = defaultComponentOrientation;
	}

	public bool getGrowingFlag(int column) {
		if (growingFlags.Length == 1) {
			return growingFlags[0];
		}
		else if (growingFlags.Length > column) {
			return growingFlags[column];
		}
		else {
			return false;
		}
	}

	public int getDefaultComponentOrientation(int column) {
		if (defaultComponentOrientation.Length == 1) {
			return defaultComponentOrientation[0];
		}

		if (defaultComponentOrientation.Length > column) {
			return defaultComponentOrientation[column];
		}

		return 0; // default for both x and y
	}
}