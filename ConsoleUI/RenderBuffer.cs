﻿using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace ConsoleUI;

/// <summary>
/// Utility to buffer the rendered chars in a coordinate system, with sub buffers that only have access to certain areas.
/// </summary>
public class RenderBuffer {
	private static readonly Color DEFAULT_FOREGROUND_COLOR = Color.White;
	private static readonly Color DEFAULT_BACKGROUND_COLOR = Color.Black;

	private readonly RenderBuffer? Parent; // parent buffer, null for the root buffer
	private readonly char[][]? Characters; // buffer, only not null for the root buffer
	private readonly Color[][]? Foregrounds; // foreground colors for each char in buffer, only not null for the root buffer
	private readonly Color[][]? Backgrounds; // background colors for each char in buffer, only not null for the root buffer

	private readonly int XOffset; // x offset to its parent
	private readonly int YOffset; // y offset to its parent

	private readonly int Width;
	private readonly int Height;
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
		Parent = parent;

		XOffset = xOffset;
		YOffset = yOffset;
		Width = width;
		Height = height;

		ForegroundColor = parent?.ForegroundColor ?? DEFAULT_FOREGROUND_COLOR;
		BackgroundColor = parent?.BackgroundColor ?? DEFAULT_BACKGROUND_COLOR;

		if (parent != null) return;
		
		Characters = new char[height][];
		for (int i = 0; i < height; i++) {
			Characters[i] = new string(' ', width).ToCharArray();
		}

		Foregrounds = new Color[height][];
		for (int i = 0; i < height; i++) {
			Foregrounds[i] = new Color[width];
			for (int ii = 0; ii < width; ii++) Foregrounds[i][ii] = DEFAULT_FOREGROUND_COLOR;
		}

		Backgrounds = new Color[height][];
		for (int i = 0; i < height; i++) {
			Backgrounds[i] = new Color[width];
			for (int ii = 0; ii < width; ii++) Backgrounds[i][ii] = DEFAULT_BACKGROUND_COLOR;
		}
	}

	public RenderBuffer(int width, int height) : this(null, 0, 0, width, height) {}

	public int GetWidth() {
		return Width;
	}

	public int GetHeight() {
		return Height;
	}

	public void Set(int x, int y, char c) {
		Set(x, y, c, ForegroundColor, BackgroundColor);
	}

	public void Set(int x, int y, char c, Color foregroundColor, Color backgroundColor) {
		if (x < 0 || x >= Width || y < 0 || y >= Height || c == '\r') // ignore invalid coordinates and \r (\r breaks the whole layout)
		{
			return;
		}

		if (Parent == null) {
			// arrays are never null if the parent is null
			Characters[y][x] = c;
			Foregrounds[y][x] = foregroundColor;
			if (backgroundColor.A > 0) Backgrounds[y][x] = backgroundColor;
		}
		else {
			Parent.Set(x + XOffset, y + YOffset, c, foregroundColor, backgroundColor);
		}
	}

	public void SetAll(int x, int y, string s) {
		for (int i = 0; i < s.Length; i++) {
			Set(x + i, y, s[i]);
		}
	}

	public char GetChar(int x, int y) {
		return Parent?.GetChar(x + XOffset, y + YOffset) ?? Characters[y][x];
	}
	
	public Color GetForegroundColor(int x, int y) {
		return Parent?.GetForegroundColor(x + XOffset, y + YOffset) ?? Foregrounds[y][x];
	}
	
	public Color GetBackgroundColor(int x, int y) {
		return Parent?.GetBackgroundColor(x + XOffset, y + YOffset) ?? Backgrounds[y][x];
	}

	public void Clear() {
		if (Parent != null) {
			throw new NotImplementedException("Only the root buffer can be cleared.");
		}

		for (int y = 0; y < Height; y++) {
			for (int x = 0; x < Width; x++) {
				Characters[y][x] = ' ';
			}
		}
	}

	public RenderBuffer GetRoot() {
		return Parent == null ? this : Parent.GetRoot();
	}

	public void RenderToConsole() {
		if (Parent != null) {
			throw new NotImplementedException("Only the root buffer can be rendered.");
		}

		Console.Clear();

		Color currentForeground = Color.White;
		Color currentBackground = Color.Black;
		StringBuilder sb = new StringBuilder();
		for (int y = 0; y < Height; y++) {
			for (int x = 0; x < Width; x++) {
				Color nextForeground = Foregrounds[y][x];
				if (nextForeground != currentForeground) {
					sb.Append(CUIUtils.CreateColorPrefix(nextForeground, true));
					currentForeground = nextForeground;
				}

				Color nextBackground = Backgrounds[y][x];
				if (nextBackground != currentBackground) {
					sb.Append(CUIUtils.CreateColorPrefix(nextBackground, false));
					currentBackground = nextBackground;
				}

				sb.Append(Characters[y][x]);
			}

			sb.Append('\n');
		}

		Console.Write(sb.ToString());
	}
}