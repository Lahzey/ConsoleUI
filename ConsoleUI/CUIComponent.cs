using System.Drawing;

namespace ConsoleUI;

public abstract class CUIComponent {
	private const char HORIZONTAL_LINE = '─';
	private const char VERTICAL_LINE = '│';
	private const char CORNER = '+';

	public CuiContainer? Parent { get; protected internal set; }

	#region margins
	private int _marginTop = 0;
	public int MarginTop {
		get => _marginTop;
		set {
			_marginTop = value;
			Rerender();
		}
	}
	private int _marginBottom = 0;
	public int MarginBottom {
		get => _marginBottom;
		set {
			_marginBottom = value;
			Rerender();
		}
	}
	private int _marginRight = 0;
	public int MarginRight {
		get => _marginRight;
		set {
			_marginRight = value;
			Rerender();
		}
	}
	private int _marginLeft = 0;
	public int MarginLeft {
		get => _marginLeft;
		set {
			_marginLeft = value;
			Rerender();
		}
	}
	#endregion margins

	#region borders
	private int _borderTop = 0;
	public int BorderTop {
		get => _borderTop;
		set {
			_borderTop = value;
			Rerender();
		}
	}
	private int _borderBottom = 0;
	public int BorderBottom {
		get => _borderBottom;
		set {
			_borderBottom = value;
			Rerender();
		}
	}
	private int _borderRight = 0;
	public int BorderRight {
		get => _borderRight;
		set {
			_borderRight = value;
			Rerender();
		}
	}
	private int _borderLeft = 0;
	public int BorderLeft {
		get => _borderLeft;
		set {
			_borderLeft = value;
			Rerender();
		}
	}
	#endregion borders

	#region paddings
	private int _paddingTop = 0;
	public int PaddingTop {
		get => _paddingTop;
		set {
			_paddingTop = value;
			Rerender();
		}
	}
	private int _paddingBottom = 0;
	public int PaddingBottom {
		get => _paddingBottom;
		set {
			_paddingBottom = value;
			Rerender();
		}
	}
	private int _paddingRight = 0;
	public int PaddingRight {
		get => _paddingRight;
		set {
			_paddingRight = value;
			Rerender();
		}
	}
	private int _paddingLeft = 0;
	public int PaddingLeft {
		get => _paddingLeft;
		set {
			_paddingLeft = value;
			Rerender();
		}
	}
	#endregion paddings

	#region borderChars
	private char _borderTopChar = HORIZONTAL_LINE;
	public char BorderTopChar {
		get => _borderTopChar;
		set {
			_borderTopChar = value;
			Rerender();
		}
	}
	private char _borderBottomChar = HORIZONTAL_LINE;
	public char BorderBottomChar {
		get => _borderBottomChar;
		set {
			_borderBottomChar = value;
			Rerender();
		}
	}
	private char _borderLeftChar = VERTICAL_LINE;
	public char BorderLeftChar {
		get => _borderLeftChar;
		set {
			_borderLeftChar = value;
			Rerender();
		}
	}
	private char _borderRightChar = VERTICAL_LINE;
	public char BorderRightChar {
		get => _borderRightChar;
		set {
			_borderRightChar = value;
			Rerender();
		}
	}
	private char _borderCornerChar = CORNER;
	public char BorderCornerChar {
		get => _borderCornerChar;
		set {
			_borderCornerChar = value;
			Rerender();
		}
	}
	#endregion borderChars

	#region colors
	private Color _foregroundColor = CUIDefaults.ForegroundColor;
	public Color ForegroundColor {
		get => _foregroundColor;
		set {
			_foregroundColor = value;
			Rerender();
		}
	}
	private Color _backgroundColor = CUIDefaults.BackgroundColor;
	public Color BackgroundColor {
		get => _backgroundColor;
		set {
			_backgroundColor = value;
			Rerender();
		}
	}
	private Color _borderColor = CUIDefaults.BorderColor;
	public Color BorderColor {
		get => _borderColor;
		set {
			_borderColor = value;
			Rerender();
		}
	}
	private Color _focusedForegroundColor = CUIDefaults.FocusedForegroundColor;
	public Color FocusedForegroundColor {
		get => _focusedForegroundColor;
		set {
			_focusedForegroundColor = value;
			Rerender();
		}
	}
	private Color _focusedBackgroundColor = CUIDefaults.FocusedBackgroundColor;
	public Color FocusedBackgroundColor {
		get => _focusedBackgroundColor;
		set {
			_focusedBackgroundColor = value;
			Rerender();
		}
	}
	private Color _focusedBorderColor = CUIDefaults.FocusedBorderColor;
	public Color FocusedBorderColor {
		get => _focusedBorderColor;
		set {
			_focusedBorderColor = value;
			Rerender();
		}
	}
	#endregion colors

	#region misc
	private bool _opaque = true;
	public bool Opaque {
		get => _opaque;
		set {
			_opaque = value;
			Rerender();
		}
	}
	private bool _focusable;
	public bool Focusable {
		get => _focusable;
		set {
			_focusable = value;
			Rerender();
		}
	}
	private bool _debug;
	public bool Debug {
		get => _debug;
		set {
			_debug = value;
			Rerender();
		}
	}
	#endregion
	
	public Action? OnFocusGained { get; set; }
	public Action? OnFocusLost { get; set; }

	/// <summary>
	/// Handles the given input. Called by the RootConsoleContainer on the currently focused component whenever the user presses a key.
	/// </summary>
	/// <param name="keyInfo">The key that has been pressed.</param>
	/// <returns>True if the key input has been consumed (prevents triggers of hotkeys), false if not.</returns>
	public virtual bool HandleInput(ConsoleKeyInfo keyInfo) {
		return false;
	}

	public bool IsFocused() {
		CUIRootContainer? cuiRoot = CUIUtils.GetRoot(this);
		return cuiRoot != null && cuiRoot.GetFocused() == this;
	}

	public void Focus() {
		CUIRootContainer? cuiRoot = CUIUtils.GetRoot(this);
		cuiRoot?.Focus(this);
	}

	/// <summary>
	/// Searches for the next component in the component tree, relevant for focusing.
	/// </summary>
	/// <returns>The next component in the component tree, containers come before their children.</returns>
	protected virtual CUIComponent GetNext() {
		CuiContainer? parent = Parent;
		CUIComponent component = this;
		while (parent != null) {
			int index = parent.IndexOf(component);
			if (index < parent.GetComponentCount() - 1) {
				return parent.GetComponent(index + 1);
			}

			component = parent;
			parent = parent.Parent;
		}

		return component; // if returning at this point, component will be set to the root container
	}

	public void SetMargin(int top, int bottom, int left, int right) {
		MarginTop = top;
		MarginBottom = bottom;
		MarginLeft = left;
		MarginRight = right;
	}

	public void SetBorder(int top, int bottom, int left, int right) {
		BorderTop = top;
		BorderBottom = bottom;
		BorderLeft = left;
		BorderRight = right;
	}

	public void SetPadding(int top, int bottom, int left, int right) {
		PaddingTop = top;
		PaddingBottom = bottom;
		PaddingLeft = left;
		PaddingRight = right;
	}

	public int GetWidth() {
		return MarginLeft + BorderLeft + PaddingLeft + GetContentWidth() + PaddingRight + BorderRight + MarginRight;
	}

	/// <summary>
	/// Checks the width of the content itself, not including the borders, margins and paddings.
	/// </summary>
	/// <returns>The presumed width the actual content would take up, it can still expand to take up more if space is available.</returns>
	protected abstract int GetContentWidth();

	public int GetHeight() {
		return MarginTop + BorderTop + PaddingTop + GetContentHeight() + PaddingBottom + BorderBottom + MarginBottom;
	}

	/// <summary>
	/// Checks the height of the content itself, not including the borders, margins and paddings.
	/// </summary>
	/// <returns>The presumed height the actual content would take up, it can still expand to take up more if space is available.</returns>
	protected abstract int GetContentHeight();

	/// <summary>
	/// Renders the content of this component, not including the borders, margins and paddings.
	/// </summary>
	/// <param name="buffer">A RenderBuffer already cut to the available size.</param>
	protected abstract void RenderContent(RenderBuffer buffer);

	public virtual void Render(RenderBuffer buffer) {
		RenderBuffer originalBuffer = buffer;
		Color prevForegroundColor = buffer.ForegroundColor;
		Color prevBackgroundColor = buffer.BackgroundColor;
		bool focused = IsFocused();

		buffer = new RenderBuffer(buffer, MarginLeft, MarginTop, buffer.GetWidth() - MarginLeft - MarginRight, buffer.GetHeight() - MarginTop - MarginBottom);

		buffer.ForegroundColor = focused ? FocusedBorderColor : BorderColor;
		buffer = CUIUtils.Pad(buffer, BorderTop, BorderBottom, BorderLeft, BorderRight, BorderTopChar, BorderBottomChar, BorderLeftChar, BorderRightChar, BorderCornerChar);

		buffer.BackgroundColor = focused ? FocusedBackgroundColor : BackgroundColor;
		if (Opaque) {
			for (int y = 0; y < buffer.GetHeight(); y++) {
				for (int x = 0; x < buffer.GetWidth(); x++) {
					buffer.Set(x, y, ' ');
				}
			}
		}

		buffer = new RenderBuffer(buffer, PaddingLeft, PaddingTop, buffer.GetWidth() - PaddingLeft - PaddingRight, buffer.GetHeight() - PaddingTop - PaddingBottom);

		buffer.ForegroundColor = focused ? FocusedForegroundColor : ForegroundColor;
		RenderContent(buffer);

		if (Debug) {
			originalBuffer.SetAll(0, 0, originalBuffer.GetWidth() + "x" + originalBuffer.GetHeight());
		}

		originalBuffer.ForegroundColor = prevForegroundColor;
		originalBuffer.BackgroundColor = prevBackgroundColor;
	}

	/// <summary>
	/// Shortcut for ConsoleRenderManager.Instance.Rerender()
	/// </summary>
	public void Rerender() {
		CUIRenderManager.Instance.Rerender();
	}
}