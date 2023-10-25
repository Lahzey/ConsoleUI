namespace ConsoleUI.Constraints;

public class AxisConstraints {
	private readonly bool[] GrowingFlags;
	private readonly int[] DefaultComponentOrientation;

	protected AxisConstraints(bool[] growingFlags, int[] defaultComponentOrientation) {
		GrowingFlags = growingFlags;
		DefaultComponentOrientation = defaultComponentOrientation;
	}

	public bool GetGrowingFlag(int column) {
		if (GrowingFlags.Length == 1) {
			return GrowingFlags[0];
		}
		else if (GrowingFlags.Length > column) {
			return GrowingFlags[column];
		}
		else {
			return false;
		}
	}

	public int GetDefaultComponentOrientation(int column) {
		if (DefaultComponentOrientation.Length == 1) {
			return DefaultComponentOrientation[0];
		}

		if (DefaultComponentOrientation.Length > column) {
			return DefaultComponentOrientation[column];
		}

		return 0; // default for both x and y
	}
}