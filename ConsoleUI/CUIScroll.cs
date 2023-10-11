using System.Drawing;

namespace ConsoleUI; 

public class CUIScroll : CUIContainer {
	
	private static readonly ScrollBarChars HorizontalBarChars = new ScrollBarChars('▐', '▌', '║', '█');
	private static readonly ScrollBarChars VerticalBarChars = new ScrollBarChars('_', '‾', '=', '█');

	public int Width { get; set; } = 1;
	public int Height { get; set; } = 1;
	
	public int HorizontalScrollSpeed { get; set; } = 1;
	public int VerticalScrollSpeed { get; set; } = 1;
	
	public bool ShowHorizontalScrollbar { get; set; } = true;
	public bool ShowVerticalScrollbar { get; set; } = true;
	
	public int HorizontalScrollPosition { get; set; } = 0;
	public int VerticalScrollPosition { get; set; } = 0;
	
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
		// create a new root buffer to temporarily render the content to
		int viewportWidth = buffer.GetWidth() - (ShowVerticalScrollbar ? 1 : 0);
		int viewportHeight = buffer.GetHeight() - (ShowHorizontalScrollbar ? 1 : 0);
		int contentWidth = Math.Max(base.GetContentWidth(), viewportWidth);
		int contentHeight = Math.Max(base.GetContentHeight(), viewportHeight);
		RenderBuffer viewportBuffer = new RenderBuffer(contentWidth, contentHeight);
		viewportBuffer.ForegroundColor = buffer.ForegroundColor;
		viewportBuffer.BackgroundColor = buffer.BackgroundColor;
		
		// check max scroll position based on viewport and buffer size
		HorizontalScrollPosition = Math.Min(HorizontalScrollPosition, contentWidth - viewportWidth);
		VerticalScrollPosition = Math.Min(VerticalScrollPosition, contentHeight - viewportHeight);
		
		// TODO: this currently renders all children, even if they are completely out of view. This could be improved.
		base.RenderContent(viewportBuffer);
		
		// render the created buffer to the actual buffer with the scroll position as offset
		for (int x = 0; x < buffer.GetWidth(); x++) {
			for (int y = 0; y < buffer.GetHeight(); y++) {
				int offsetX = x + HorizontalScrollPosition;
				int offsetY = y + VerticalScrollPosition;
				if (offsetX >= contentWidth || offsetY >= contentHeight) continue; // we could break if x is higher (but not y), but this is more readable
				buffer.Set(x, y, viewportBuffer.GetChar(offsetX, offsetY), viewportBuffer.GetForegroundColor(offsetX, offsetY), viewportBuffer.GetBackgroundColor(offsetX, offsetY));
			}
		}

		Color foreground = buffer.ForegroundColor;
		Color background = LerpColor(buffer.BackgroundColor, foreground, 0.2f);
		
		int maxX = buffer.GetWidth() - (ShowVerticalScrollbar ? 1 : 0);
		if (ShowHorizontalScrollbar) {
			char[] horizontalScrollBar = CreateScrollBar(contentWidth, HorizontalScrollPosition, viewportWidth, HorizontalBarChars);
			int y = buffer.GetHeight() - 1;
			for (int x = 0; x < maxX; x++) {
				buffer.Set(x, y, horizontalScrollBar[x]);
			}
		}
		
		int maxY = buffer.GetHeight() - (ShowHorizontalScrollbar ? 1 : 0);
		if (ShowVerticalScrollbar) {
			char[] verticalScrollBar = CreateScrollBar(contentHeight, VerticalScrollPosition, viewportHeight, VerticalBarChars);
			int x = buffer.GetWidth() - 1;
			for (int y = 0; y < maxY; y++) {
				buffer.Set(x, y, verticalScrollBar[y]);
			}
		}
	}

	public override bool HandleInput(ConsoleKeyInfo keyInfo) {
		if (keyInfo.Key == ConsoleKey.DownArrow) {
			VerticalScrollPosition += VerticalScrollSpeed;
			Rerender();
			return true;
		} else if (keyInfo.Key == ConsoleKey.UpArrow) {
			VerticalScrollPosition -= VerticalScrollSpeed;
			if (VerticalScrollPosition < 0) VerticalScrollPosition = 0;
			Rerender();
			return true;
		} else if (keyInfo.Key == ConsoleKey.RightArrow) {
			HorizontalScrollPosition += HorizontalScrollSpeed;
			Rerender();
			return true;
		} else if (keyInfo.Key == ConsoleKey.LeftArrow) {
			HorizontalScrollPosition -= HorizontalScrollSpeed;
			if (HorizontalScrollPosition < 0) HorizontalScrollPosition = 0;
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

	private class ScrollBarChars {
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