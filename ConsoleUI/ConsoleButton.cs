using System.Drawing;

namespace ConsoleUI;

public class ConsoleButton : ConsoleLabel, IActivateable
{
    private bool disabled = false;
    /// <summary>
    /// If disabled, the button will no longer be focusable and cannot be activated by any means.
    /// </summary>
    public bool Disabled
    {
        get => disabled;
        set
        {
            disabled = value;
            Focusable = !disabled;
            Rerender();
        }
    }
    
    public ConsoleKey Hotkey { get; set; }

    public Action Action { get; set; }
    
    public ConsoleButton(string content, Action action = null) : base(content)
    {
        this.Action = action;
        TextOrientation = CENTER;
        SetBorder(1, 1, 1, 1);
        SetPadding(0, 0, 1, 1);
        Focusable = true;
        BorderColor = Color.DimGray;
    }

    public override bool HandleInput(ConsoleKeyInfo keyInfo)
    {
        if (!disabled && keyInfo.Key == ConsoleKey.Enter)
        {
            if (Action != null) Activate();
            return true;
        }
        return false;
    }

    public void Activate()
    {
        if (!Disabled)
        {
            if (Focusable) Focus();
            Action();
        }
    }

    public ConsoleKey GetHotkey()
    {
        return Hotkey;
    }
}