using System.Drawing;

namespace ConsoleUI;

/// <summary>
/// A button that looks like a label.
/// </summary>
public class LabelButton : ConsoleButton
{
    public LabelButton(string content, Action action) : base(content, action)
    {
        TextOrientation = CENTER;
        
        SetBorder(0, 0, 1, 1);
        BorderLeftChar = '>';
        BorderRightChar = '<';
        BorderColor = Color.Black; // Hides border when not selected
        FocusedBackgroundColor = BackgroundColor;
    }
}