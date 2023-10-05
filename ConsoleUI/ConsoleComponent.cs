using System.Drawing;

namespace ConsoleUI;

public abstract class ConsoleComponent
{
    private static readonly char HorizontalLine = '-';
    private static readonly char VerticalLine = '|';
    private static readonly char Corner = '+';
    
    public ConsoleContainer Parent { get; protected internal set; }

    #region margins
    public int MarginTop { get => marginTop; set { marginTop = value; Rerender(); } }
    private int marginTop = 0;
    public int MarginBottom { get => marginBottom; set { marginBottom = value; Rerender(); } }
    private int marginBottom = 0;
    public int MarginRight { get => marginRight; set { marginRight = value; Rerender(); } }
    private int marginRight = 0;
    public int MarginLeft { get => marginLeft; set { marginLeft = value; Rerender(); } }
    private int marginLeft = 0;
    #endregion margins

    #region borders
    public int BorderTop { get => borderTop; set { borderTop = value; Rerender(); } }
    private int borderTop = 0;
    public int BorderBottom { get => borderBottom; set { borderBottom = value; Rerender(); } }
    private int borderBottom = 0;
    public int BorderRight { get => borderRight; set { borderRight = value; Rerender(); } }
    private int borderRight = 0;
    public int BorderLeft { get => borderLeft; set { borderLeft = value; Rerender(); } }
    private int borderLeft = 0;
    #endregion borders

    #region paddings
    public int PaddingTop { get => paddingTop; set { paddingTop = value; Rerender(); } }
    private int paddingTop = 0;
    public int PaddingBottom { get => paddingBottom; set { paddingBottom = value; Rerender(); } }
    private int paddingBottom = 0;
    public int PaddingRight { get => paddingRight; set { paddingRight = value; Rerender(); } }
    private int paddingRight = 0;
    public int PaddingLeft { get => paddingLeft; set { paddingLeft = value; Rerender(); } }
    private int paddingLeft = 0;
    #endregion paddings

    #region borderChars
    public char BorderTopChar { get => borderTopChar; set { borderTopChar = value; Rerender(); } }
    private char borderTopChar = HorizontalLine;
    public char BorderBottomChar { get => borderBottomChar; set { borderBottomChar = value; Rerender(); } }
    private char borderBottomChar = HorizontalLine;
    public char BorderLeftChar { get => borderLeftChar; set { borderLeftChar = value; Rerender(); } }
    private char borderLeftChar = VerticalLine;
    public char BorderRightChar { get => borderRightChar; set { borderRightChar = value; Rerender(); } }
    private char borderRightChar = VerticalLine;
    public char BorderCornerChar { get => borderCornerChar; set { borderCornerChar = value; Rerender(); } }
    private char borderCornerChar = Corner;
    #endregion borderChars
    
    #region colors
    public Color ForegroundColor { get => foregroundColor; set { foregroundColor = value; Rerender(); } }
    private Color foregroundColor = UIDefaults.ForegroundColor;
    public Color BackgroundColor { get => backgroundColor; set { backgroundColor = value; Rerender(); } }
    private Color backgroundColor = UIDefaults.BackgroundColor;
    public Color BorderColor { get => borderColor; set { borderColor = value; Rerender(); } }
    private Color borderColor = UIDefaults.BorderColor;
    public Color FocusedForegroundColor { get => focusedForegroundColor; set { focusedForegroundColor = value; Rerender(); } }
    private Color focusedForegroundColor = UIDefaults.FocusedForegroundColor;
    public Color FocusedBackgroundColor { get => focusedBackgroundColor; set { focusedBackgroundColor = value; Rerender(); } }
    private Color focusedBackgroundColor = UIDefaults.FocusedBackgroundColor;
    public Color FocusedBorderColor { get => focusedBorderColor; set { focusedBorderColor = value; Rerender(); } }
    private Color focusedBorderColor = UIDefaults.FocusedBorderColor;
    #endregion colors

    public bool Opaque { get => opaque; set { opaque = value; Rerender(); } }
    private bool opaque = true;
    public bool Focusable { get => focusable; set { focusable = value; Rerender(); } }
    private bool focusable;
    public bool Debug { get => debug; set { debug = value; Rerender(); } }
    private bool debug;

    public Action OnFocusGained { get; set; }
    public Action OnFocusLost { get; set; }

    /// <summary>
    /// Handles the given input. Called by the RootConsoleContainer on the currently focused component whenever the user presses a key.
    /// </summary>
    /// <param name="keyInfo">The key that has been pressed.</param>
    /// <returns>True if the key input has been consumed (prevents triggers of hotkeys), false if not.</returns>
    public virtual bool HandleInput(ConsoleKeyInfo keyInfo)
    {
        return false;
    }

    public bool IsFocused()
    {
        RootConsoleContainer root = ConsoleUIUtils.GetRoot(this);
        return root != null ? root.getFocused() == this : false;
    }
    
    public void Focus()
    {
        RootConsoleContainer root = ConsoleUIUtils.GetRoot(this);
        if (root != null) root.Focus(this);
    }

    /// <summary>
    /// Searches for the next component in the component tree, relevant for focusing.
    /// </summary>
    /// <returns>The next component in the component tree, containers come before their children.</returns>
    protected virtual ConsoleComponent GetNext()
    {
        ConsoleContainer parent = Parent;
        ConsoleComponent component = this;
        while (parent != null)
        {
            int index = parent.IndexOf(component);
            if (index < parent.GetComponentCount() - 1)
            {
                return parent.GetComponent(index + 1);
            }
            component = parent;
            parent = parent.Parent;
        }

        return component; // if returning at this point, component will be set to the root container
    }

    public void SetMargin(int top, int bottom, int left, int right)
    {
        MarginTop = top;
        MarginBottom = bottom;
        MarginLeft = left;
        MarginRight = right;
    }

    public void SetBorder(int top, int bottom, int left, int right)
    {
        BorderTop = top;
        BorderBottom = bottom;
        BorderLeft = left;
        BorderRight = right;
    }

    public void SetPadding(int top, int bottom, int left, int right)
    {
        PaddingTop = top;
        PaddingBottom = bottom;
        PaddingLeft = left;
        PaddingRight = right;
    }

    public int GetWidth()
    {
        return MarginLeft + BorderLeft + PaddingLeft + GetContentWidth() + PaddingRight + BorderRight + MarginRight;
    }
    
    /// <summary>
    /// Checks the width of the content itself, not including the borders, margins and paddings.
    /// </summary>
    /// <returns>The presumed width the actual content would take up, it can still expand to take up more if space is available.</returns>
    protected abstract int GetContentWidth();

    public int GetHeight()
    {
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

    public virtual void Render(RenderBuffer buffer)
    {
        RenderBuffer originalBuffer = buffer;
        Color prevForegroundColor = buffer.ForegroundColor;
        Color prevBackgroundColor = buffer.BackgroundColor;
        bool focused = IsFocused();
        
        buffer = new RenderBuffer(buffer, MarginLeft, MarginTop, buffer.GetWidth() - MarginLeft - MarginRight, buffer.GetHeight() - MarginTop - MarginBottom);
        
        buffer.ForegroundColor = focused ? FocusedBorderColor : BorderColor;
        buffer = ConsoleUIUtils.Pad(buffer, BorderTop, BorderBottom, BorderLeft, BorderRight, BorderTopChar, BorderBottomChar, BorderLeftChar, BorderRightChar, BorderCornerChar);
        
        buffer.BackgroundColor = focused ? FocusedBackgroundColor : BackgroundColor;
        if (Opaque)
        {
            for(int y = 0; y < buffer.GetHeight(); y++)
            {
                for(int x = 0; x < buffer.GetWidth(); x++)
                {
                    buffer.Set(x, y, ' ');
                }
            }
        }
        buffer = new RenderBuffer(buffer, PaddingLeft, PaddingTop, buffer.GetWidth() - PaddingLeft - PaddingRight, buffer.GetHeight() - PaddingTop - PaddingBottom);
        
        buffer.ForegroundColor = focused ? FocusedForegroundColor : ForegroundColor;
        RenderContent(buffer);

        if (debug)
        {
            originalBuffer.SetAll(0, 0, originalBuffer.GetWidth()  + "x" + originalBuffer.GetHeight());
        }
        
        originalBuffer.ForegroundColor = prevForegroundColor;
        originalBuffer.BackgroundColor = prevBackgroundColor;
    }
    
    /// <summary>
    /// Shortcut for ConsoleRenderManager.Instance.Rerender()
    /// </summary>
    public void Rerender()
    {
        ConsoleRenderManager.Instance.Rerender();
    }
}