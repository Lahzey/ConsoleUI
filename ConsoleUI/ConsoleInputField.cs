using System.Drawing;

namespace ConsoleUI;

public class ConsoleInputField : ConsoleLabel
{
    /// <summary>
    /// Can limit user input to certain characters or patterns.
    /// </summary>
    public Func<char, bool> InputFilter { get; set; }
    /// <summary>
    /// Called whenever the user inputs a character.
    /// </summary>
    public Action OnInput { get; set; }
    
    public ConsoleInputField(string text = "") : base(text)
    {
        Focusable = true;
        SetBorder(0, 0, 1, 1);
        BackgroundColor = Color.FromArgb(255, 66, 66, 66);
        BorderLeftChar = '[';
        BorderRightChar = ']';
    }

    public override bool HandleInput(ConsoleKeyInfo keyInfo)
    {
        if (keyInfo.Key == ConsoleKey.Backspace)
        {
            Content = Content.Substring(0, Math.Max(Content.Length - 1, 0));
            if (OnInput != null) OnInput();
            return true;
        }
        else if (keyInfo.Key == ConsoleKey.Enter)
        {
            RootConsoleContainer root = ConsoleUIUtils.GetRoot(this);
            if (root != null) root.FocusNext(this);
            return true;
        }
        else if (char.IsLetterOrDigit(keyInfo.KeyChar) || char.IsPunctuation(keyInfo.KeyChar) || char.IsSymbol(keyInfo.KeyChar) || keyInfo.KeyChar == ' ')
        {
            if (InputFilter == null || InputFilter(keyInfo.KeyChar))
            {
                Content = Content + keyInfo.KeyChar;
                if (OnInput != null) OnInput();
                return true;
            }
        }
        return false;
    }
}