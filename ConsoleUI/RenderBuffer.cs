using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace ConsoleUI;

/// <summary>
/// Utility to buffer the rendered chars in a coordinate system, with sub buffers that only have access to certain areas.
/// </summary>
public class RenderBuffer {
	private static readonly Color DEFAULT_FOREGROUND_COLOR = Color.White;
	private static readonly Color DEFAULT_BACKGROUND_COLOR = Color.Black;

	private readonly RenderBuffer? parent; // parent buffer, null for the root buffer
	private readonly char[][]? characters; // buffer, only not null for the root buffer
	private readonly Color[][]? foregrounds; // foreground colors for each char in buffer, only not null for the root buffer
	private readonly Color[][]? backgrounds; // background colors for each char in buffer, only not null for the root buffer

	private readonly int xOffset; // x offset to its parent
	private readonly int yOffset; // y offset to its parent

	private readonly int width;
	private readonly int height;
	public Color ForegroundColor { get; set; }
	public Color BackgroundColor { get; set; }

	// copied DllImport and content of static constructor from https://stackoverflow.com/a/43321133

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool GetConsoleMode(IntPtr handle, out int mode);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr GetStdHandle(int handle);

	static RenderBuffer() {
		IntPtr handle = GetStdHandle(-11);
		GetConsoleMode(handle, out int mode);
		SetConsoleMode(handle, mode | 0x4);
		Console.OutputEncoding = Encoding.UTF8;
	}

	public RenderBuffer(RenderBuffer? parent, int xOffset, int yOffset, int width, int height) {
		this.parent = parent;

		this.xOffset = xOffset;
		this.yOffset = yOffset;
		this.width = width;
		this.height = height;

		ForegroundColor = parent?.ForegroundColor ?? DEFAULT_FOREGROUND_COLOR;
		BackgroundColor = parent?.BackgroundColor ?? DEFAULT_BACKGROUND_COLOR;

		if (parent != null) return;
		
		characters = new char[height][];
		for (int i = 0; i < height; i++) {
			characters[i] = new string(' ', width).ToCharArray();
		}

		foregrounds = new Color[height][];
		for (int i = 0; i < height; i++) {
			foregrounds[i] = new Color[width];
			for (int ii = 0; ii < width; ii++) foregrounds[i][ii] = DEFAULT_FOREGROUND_COLOR;
		}

		backgrounds = new Color[height][];
		for (int i = 0; i < height; i++) {
			backgrounds[i] = new Color[width];
			for (int ii = 0; ii < width; ii++) backgrounds[i][ii] = DEFAULT_BACKGROUND_COLOR;
		}
	}

	public RenderBuffer(int width, int height) : this(null, 0, 0, width, height) {}

	public int GetWidth() {
		return width;
	}

	public int GetHeight() {
		return height;
	}

	public void Set(int x, int y, char c) {
		Set(x, y, c, ForegroundColor, BackgroundColor);
	}

	public void Set(int x, int y, char c, Color foregroundColor, Color backgroundColor) {
		if (x < 0 || x >= width || y < 0 || y >= height || c == '\r') // ignore invalid coordinates and \r (\r breaks the whole layout)
		{
			return;
		}

		if (parent == null) {
			characters[y][x] = c;
			foregrounds[y][x] = foregroundColor;
			if (backgroundColor.A > 0) backgrounds[y][x] = backgroundColor;
		}
		else {
			parent.Set(x + xOffset, y + yOffset, c, foregroundColor, backgroundColor);
		}
	}

	public void SetAll(int x, int y, string s) {
		for (int i = 0; i < s.Length; i++) {
			Set(x + i, y, s[i]);
		}
	}

	public char GetChar(int x, int y) {
		return parent?.GetChar(x + xOffset, y + yOffset) ?? characters[y][x];
	}
	
	public Color GetForegroundColor(int x, int y) {
		return parent?.GetForegroundColor(x + xOffset, y + yOffset) ?? foregrounds[y][x];
	}
	
	public Color GetBackgroundColor(int x, int y) {
		return parent?.GetBackgroundColor(x + xOffset, y + yOffset) ?? backgrounds[y][x];
	}

	public void Clear() {
		if (parent != null) {
			throw new NotImplementedException("Only the root buffer can be cleared.");
		}

		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				characters[y][x] = ' ';
			}
		}
	}

	public RenderBuffer GetRoot() {
		return parent == null ? this : parent.GetRoot();
	}

	public void RenderToConsole() {
		if (parent != null) {
			throw new NotImplementedException("Only the root buffer can be rendered.");
		}

		Console.Clear();

		Color currentForeground = Color.White;
		Color currentBackground = Color.Black;
		StringBuilder sb = new StringBuilder();
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				Color nextForeground = foregrounds[y][x];
				if (nextForeground == null) nextForeground = DEFAULT_FOREGROUND_COLOR;
				if (nextForeground != currentForeground) {
					sb.Append(CUIUtils.CreateColorPrefix(nextForeground, true));
					currentForeground = nextForeground;
				}

				Color nextBackground = backgrounds[y][x];
				if (nextBackground == null) nextBackground = DEFAULT_BACKGROUND_COLOR;
				if (nextBackground != currentBackground) {
					sb.Append(CUIUtils.CreateColorPrefix(nextBackground, false));
					currentBackground = nextBackground;
				}

				sb.Append(characters[y][x]);
			}

			sb.Append('\n');
		}

		Console.Write(sb.ToString());
	}

	private static void Flush(StringBuilder sb) {
		if (sb.Length > 0) {
			Console.Write(sb.ToString());
			sb.Clear();
		}
	}
}