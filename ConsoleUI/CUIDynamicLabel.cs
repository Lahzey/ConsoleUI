namespace ConsoleUI;

/// <summary>
/// A label that can dynamically get the text to be displayed.
/// Rerendering must be done manually.
/// </summary>
public class CUIDynamicLabel : CUILabel {
	public Func<string> GetText {
		get => getText;
		set {
			getText = value;
			Rerender();
		}
	}

	private Func<string> getText;

	public CUIDynamicLabel(Func<string> getText) : base(getText()) {
		this.GetText = getText;
	}

	public override void Render(RenderBuffer renderBuffer) {
		content = GetText();
		base.Render(renderBuffer);
	}
}