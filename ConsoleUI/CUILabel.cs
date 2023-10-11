namespace ConsoleUI;

public class CUILabel : CUIComponent {
	public static readonly int LEFT = 0;
	public static readonly int CENTER = 1;
	public static readonly int RIGHT = 2;

	public string Content {
		get => content;
		set {
			content = value;
			Rerender();
		}
	}

	/// <summary>
	///  Only use this access if you want to prevent a rerender.
	/// </summary>
	protected string content;

	/// <summary>
	/// Can be ConsoleLabel.LEFT, ConsoleLabel.CENTER or ConsoleLabel.RIGHT and controls how to align the text.
	/// </summary>
	public int TextOrientation {
		get => textOrientation;
		set {
			textOrientation = value;
			Rerender();
		}
	}

	private int textOrientation = LEFT;

	/// <summary>
	/// If true the text will be wrapped to new lines if no more space is available.
	/// </summary>
	public bool WrapText {
		get => wrapText;
		set {
			wrapText = value;
			Rerender();
		}
	}

	private bool wrapText = false;

	/// <summary>
	/// Gives the parent a width preference, useful when WrapText is true.
	/// </summary>
	public int PreferredWidth {
		get => preferredWidth;
		set {
			preferredWidth = value;
			Rerender();
		}
	}

	private int preferredWidth = 0;

	// for wrapping text, so the needed height can be calculated in GetHeight()
	private int lastWidth = int.MaxValue;

	public CUILabel(string content) {
		Content = content;
	}

	protected override int GetContentHeight() {
		return GetLines().Length;
	}

	protected override int GetContentWidth() {
		if (PreferredWidth > 0) return PreferredWidth;

		if (wrapText) return 0; // lets other components decide how wide this is

		int maxWidth = 0;
		// TODO: can be done more efficiently by iterating over string chars manually
		foreach (string line in Content.Split('\n')) {
			maxWidth = Math.Max(maxWidth, line.Length);
		}

		return maxWidth;
	}

	protected override void RenderContent(RenderBuffer buffer) {
		if (WrapText && lastWidth != buffer.GetWidth()) Rerender(); // only requests a rerender, so the lastWidth will be changed by the time the rerender happens
		lastWidth = buffer.GetWidth();

		string[] lines = GetLines();
		for (int i = 0; i < lines.Length; i++) {
			string line = lines[i];
			int startIndex = 0;
			if (TextOrientation == LEFT) {
				startIndex = 0;
			}
			else if (TextOrientation == CENTER) {
				startIndex = (buffer.GetWidth() - line.Length) / 2;
			}
			else if (TextOrientation == RIGHT) {
				startIndex = buffer.GetWidth() - line.Length;
			}

			buffer.SetAll(startIndex, i, line);
		}
	}

	private string[] GetLines() {
		if (WrapText) {
			List<string> lines = new List<string>();
			foreach (string contentLine in Content.Split('\n')) {
				string currentLine = "";
				foreach (string word in contentLine.Split(' ')) {
					if (currentLine.Length == 0) currentLine += word;
					else {
						if (currentLine.Length + 1 + word.Length > lastWidth) {
							lines.Add(currentLine);
							currentLine = "";
						}
						else currentLine += " ";

						currentLine += word;
					}
				}

				lines.Add(currentLine);
			}

			return lines.ToArray();
		}
		else return Content.Split('\n');
	}
}