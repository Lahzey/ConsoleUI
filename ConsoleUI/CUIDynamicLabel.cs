namespace ConsoleUI;

/// <summary>
/// A label that can dynamically get the text to be displayed.
/// Rerendering must be done manually.
/// </summary>
public class CUIDynamicLabel : CUILabel {
	
	private Func<string> _getText;
	public Func<string> GetText {
		get => _getText;
		set {
			_getText = value;
			Rerender();
		}
	}


	public CUIDynamicLabel(Func<string> getText) : base(getText()) {
		_getText = getText; // no need to rerender in a constructor
	}

	public override void Render(RenderBuffer renderBuffer) {
		_content = GetText();
		base.Render(renderBuffer);
	}
}