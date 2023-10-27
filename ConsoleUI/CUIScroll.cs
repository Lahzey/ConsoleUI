using System.Drawing;

namespace ConsoleUI; 

public class CUIScroll : CuiContainer {
	
	private static readonly ScrollBarChars DEFAULT_HORIZONTAL_BAR_CHARS = new ScrollBarChars('▐', '▌', '║', '█');
	private static readonly ScrollBarChars DEFAULT_VERTICAL_BAR_CHARS = new ScrollBarChars('_', '‾', '=', '█');

	public int Width { get; set; } = 1;
	public int Height { get; set; } = 1;
	
	private ScrollBar _horizontalScrollBar = new ScrollBar(DEFAULT_HORIZONTAL_BAR_CHARS);
	public ScrollBar HorizontalScrollBar { get => _horizontalScrollBar; set { _horizontalScrollBar = value; Rerender(); } }
	private ScrollBar _verticalScrollBar = new ScrollBar(DEFAULT_VERTICAL_BAR_CHARS);
	public ScrollBar VerticalScrollBar { get => _verticalScrollBar; set { _verticalScrollBar = value; Rerender(); } }
	
	public CUIScroll() : this("", "", "") {}

	public CUIScroll(string containerConstraints, string columnConstraints, string rowConstraints) : base(containerConstraints, columnConstraints, rowConstraints) {
		Focusable = true;
		SetBorder(1, 1, 1, 1);
		FocusedBackgroundColor = BackgroundColor;
		FocusedForegroundColor = ForegroundColor;
		FocusedBorderColor = Color.CornflowerBlue;
	}

	protected override int GetContentWidth() {
		return Math.Max(Width, 1);
	}

	protected override int GetContentHeight() {
		return Math.Max(Height, 1);
	}

	protected override void RenderContent(RenderBuffer buffer) {
		#region Size calculation and creation of viewport buffer
		int contentWidth = base.GetContentWidth();
		int viewportWidth = buffer.GetWidth();
		int contentHeight = base.GetContentHeight();
		int viewportHeight = buffer.GetHeight();
		
		bool showHorizontalScrollbar = HorizontalScrollBar.Show(viewportWidth, contentWidth);
		if (showHorizontalScrollbar) viewportHeight--;
		
		bool showVerticalScrollbar = VerticalScrollBar.Show(viewportHeight, contentHeight);
		if (showVerticalScrollbar) {
			viewportWidth--;
			showHorizontalScrollbar = HorizontalScrollBar.Show(viewportWidth, contentWidth); // needs to be recalculated because the available space was reduced
		}
		
		if (contentWidth < viewportWidth) contentWidth = viewportWidth;
		if (contentHeight < viewportHeight) contentHeight = viewportHeight;
		
		RenderBuffer viewportBuffer = new RenderBuffer(contentWidth, contentHeight);
		viewportBuffer.ForegroundColor = buffer.ForegroundColor;
		viewportBuffer.BackgroundColor = buffer.BackgroundColor;
		#endregion
		
		#region Clamp scroll positions and check fo scroll to bottom flags
		int maxScrollX = contentWidth - viewportWidth;
		int scrollX = _horizontalScrollBar.ScrollToBottom ? maxScrollX : _horizontalScrollBar.Position;
		if (scrollX > maxScrollX && _horizontalScrollBar.AutoScrollToBottom) _horizontalScrollBar.ScrollToBottom = true;
		_horizontalScrollBar.Position = Math.Clamp(scrollX, 0, maxScrollX);
		
		int maxScrollY = contentHeight - viewportHeight;
		int scrollY = _verticalScrollBar.ScrollToBottom ? maxScrollY : _verticalScrollBar.Position;
		if (scrollY > maxScrollY && _verticalScrollBar.AutoScrollToBottom) _verticalScrollBar.ScrollToBottom = true;
		_verticalScrollBar.Position = Math.Clamp(scrollY, 0, maxScrollY);
		#endregion
		
		// TODO: this currently renders all children, even if they are completely out of view. This could be improved.
		base.RenderContent(viewportBuffer);
		
		// render the created buffer to the actual buffer with the scroll position as offset
		for (int x = 0; x < buffer.GetWidth(); x++) {
			for (int y = 0; y < buffer.GetHeight(); y++) {
				int offsetX = x + _horizontalScrollBar.Position;
				int offsetY = y + _verticalScrollBar.Position;
				if (offsetX >= contentWidth || offsetY >= contentHeight) continue; // we could break if x is higher (but not y), but this is more readable
				buffer.Set(x, y, viewportBuffer.GetChar(offsetX, offsetY), viewportBuffer.GetForegroundColor(offsetX, offsetY), viewportBuffer.GetBackgroundColor(offsetX, offsetY));
			}
		}

		Color foreground = buffer.ForegroundColor;
		Color background = LerpColor(buffer.BackgroundColor, foreground, 0.2f);
		
		int maxX = buffer.GetWidth() - (showVerticalScrollbar ? 1 : 0);
		if (showHorizontalScrollbar) {
			char[] horizontalScrollBar = CreateScrollBar(contentWidth, _horizontalScrollBar.Position, viewportWidth, DEFAULT_HORIZONTAL_BAR_CHARS);
			int y = buffer.GetHeight() - 1;
			for (int x = 0; x < maxX; x++) {
				buffer.Set(x, y, horizontalScrollBar[x]);
			}
		}
		
		int maxY = buffer.GetHeight() - (showHorizontalScrollbar ? 1 : 0);
		if (showVerticalScrollbar) {
			char[] verticalScrollBar = CreateScrollBar(contentHeight, _verticalScrollBar.Position, viewportHeight, DEFAULT_VERTICAL_BAR_CHARS);
			int x = buffer.GetWidth() - 1;
			for (int y = 0; y < maxY; y++) {
				buffer.Set(x, y, verticalScrollBar[y]);
			}
		}
	}

	public override bool HandleInput(ConsoleKeyInfo keyInfo) {
		if (keyInfo.Key == ConsoleKey.DownArrow) {
			_verticalScrollBar.Position += _verticalScrollBar.Speed;
			Rerender();
			return true;
		} else if (keyInfo.Key == ConsoleKey.UpArrow) {
			_verticalScrollBar.Position -= _verticalScrollBar.Speed;
			_verticalScrollBar.ScrollToBottom = false;
			Rerender();
			return true;
		} else if (keyInfo.Key == ConsoleKey.RightArrow) {
			_horizontalScrollBar.Position += _horizontalScrollBar.Speed;
			Rerender();
			return true;
		} else if (keyInfo.Key == ConsoleKey.LeftArrow) {
			_horizontalScrollBar.Position -= _horizontalScrollBar.Speed;
			_horizontalScrollBar.ScrollToBottom = false;
			Rerender();
			return true;
		}

		return false;
	}

	private static Color LerpColor(Color a, Color b, float distance) {
		return Color.FromArgb(
			(int) (a.R + (b.R - a.R) * distance),
			(int) (a.G + (b.G - a.G) * distance),
			(int) (a.B + (b.B - a.B) * distance)
		);
	}

	private static char[] CreateScrollBar(int contentSize, int scrollPosition, int sizeOnScreen, ScrollBarChars chars) {
		char[] result = new char[sizeOnScreen];
		 
		// in the following calculations 1 equals the size of one "pixel" (character on the screen)
		float sizeReduction = sizeOnScreen / (float) contentSize;
		float scrollBarSize = sizeOnScreen * sizeReduction;
		float scrollBarStart = scrollPosition * sizeReduction;
		float scrollBarEnd = scrollBarStart + scrollBarSize;
		float scrollBarCenter = (scrollBarStart + scrollBarEnd) / 2f;

		float halfScrollBarSize = scrollBarSize / 2f;
		for (int i = 0; i < sizeOnScreen; i++) {
			float center = i + 0.5f;
			bool aboveScrollCenter = center < scrollBarCenter;
			float distToScrollBarCenter = Math.Abs(center - scrollBarCenter);
			float toScrollBarEdge = halfScrollBarSize - distToScrollBarCenter;

			char edgeChar = aboveScrollCenter ? chars.StartEdgeChar : chars.EndEdgeChar;
			if (distToScrollBarCenter < 0.5f) result[i] = scrollBarSize >= 0.5f ? chars.FillChar : distToScrollBarCenter <= 0.25f ? chars.CenterLineChar : edgeChar; // if this pixel contains the scrollbar center
			else if (toScrollBarEdge >= 0f) result[i] = chars.FillChar; // if scrollbar reaches at lest the current pixels center, render it fully
			else if (toScrollBarEdge > -0.5f) result[i] = edgeChar; // if it reaches the current pixels edge, render it as an edge
			else result[i] = ' ';
		}

		return result;
	}

	public struct ScrollBar {
		/// <summary>
		/// The character set used to render the scrollbar.
		/// </summary>
		public ScrollBarChars Chars;
		/// <summary>
		/// Defines if and when the scrollbar is shown.
		/// </summary>
		public ShowMode ShowMode = ShowMode.WhenNeeded;
		/// <summary>
		/// The amount of characters the view is offset by in the axis of this scrollbar.
		/// </summary>
		public int Position = 0;
		/// <summary>
		/// The speed of scrolling in characters per key press.
		/// </summary>
		public int Speed = 1;
		/// <summary>
		/// If true, the scrollbar will always be scrolled to the very bottom.
		/// </summary>
		public bool ScrollToBottom = false;
		/// <summary>
		/// Turns on ScrollToBottom when the content is scrolled past the bottom.
		/// </summary>
		public bool AutoScrollToBottom = true;

		public ScrollBar(ScrollBarChars chars) {
			Chars = chars;
		}
		
		public bool Show(int availableSpace, int neededSpace) => ShowMode == ShowMode.Always || ShowMode == ShowMode.WhenNeeded && neededSpace > availableSpace;
	}
	
	public enum ShowMode {
		Always,
		WhenNeeded,
		Never
	}

	public class ScrollBarChars { // can be a class instead of struct because it is immutable (saves memory)
		public readonly char StartEdgeChar;
		public readonly char EndEdgeChar;
		public readonly char CenterLineChar;
		public readonly char FillChar;

		public ScrollBarChars(char startEdgeChar, char endEdgeChar, char centerLineChar, char fillChar) {
			StartEdgeChar = startEdgeChar;
			EndEdgeChar = endEdgeChar;
			CenterLineChar = centerLineChar;
			FillChar = fillChar;
		}
	}
}