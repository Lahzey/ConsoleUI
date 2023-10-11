namespace ConsoleUI;

/// <summary>
/// Utility for showing a popup window over the main content.
/// </summary>
public class CUIPopup : CUIRootContainer {
	private Action close;

	private CUIPopup(CUIComponent content, Action close) : base(content) {
		this.close = close;
		Opaque = false;
	}

	protected override void Exit() {
		close();
	}

	/// <summary>
	/// Shows the given content in a popup together with a confirm button that closes it.
	/// Esc also closes the popup.
	/// Waits for the popup to be closed before returning.
	/// </summary>
	/// <param name="content">the content of the popup</param>
	/// <returns>true if the user pressed confirm, false if he pressed Esc</returns>
	public static bool Show(CUIComponent content) {
		bool open = true;
		bool result = false;
		CUIContainer popupContainer = new CUIContainer("", "[grow, center]", "[grow, center]");
		popupContainer.Opaque = false;
		CUIPopup cuiPopup = new CUIPopup(popupContainer, () => open = false);

		CUIContainer contentContainer = new CUIContainer("wrap 1", "[grow, fill]", "[grow, fill][]");
		contentContainer.SetBorder(1, 1, 1, 1);
		contentContainer.SetPadding(0, 0, 1, 1);
		contentContainer.Add(content);
		CUILabelButton confirmButton = new CUILabelButton("(C)onfirm", () => {
			open = false;
			result = true;
		});
		confirmButton.Hotkey = ConsoleKey.C;
		confirmButton.MarginTop = 1;
		contentContainer.Add(confirmButton);
		popupContainer.Add(contentContainer);

		CUIRenderManager.Instance.AddContainer(cuiPopup);
		while (open) {
			Thread.Sleep(10);
		}

		CUIRenderManager.Instance.RemoveContainer(cuiPopup);

		return result;
	}

	public static string? ShowInput(string label) {
		CUIContainer container = new CUIContainer("", "[grow, fill]", "[][grow, fill]");
		container.SetBorder(1, 1, 1, 1);
		container.SetPadding(0, 0, 1, 1);
		container.Add(new CUILabel(label));
		CUIInputField inputField = new CUIInputField();
		container.Add(inputField);
		if (Show(container)) {
			return inputField.Content;
		}
		else {
			return null;
		}
	}
}