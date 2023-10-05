namespace ConsoleUI;

/// <summary>
/// A label that can dynamically get the text to be displayed.
/// Rerendering must be done manually.
/// </summary>
public class DynamicLabel : ConsoleLabel
{
    public Func<string> GetText { get => getText; set { getText = value; Rerender(); } }
    private Func<string> getText;
    
    public DynamicLabel(Func<string> getText)  : base(getText())
    {
        this.GetText = getText;
    }


    /**
     * Set a thrust strength from 0 - 1l
     */
    public override void Render(RenderBuffer renderBuffer)
    {
        content = GetText();
        base.Render(renderBuffer);
    }
}