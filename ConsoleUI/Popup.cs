namespace ConsoleUI;

/// <summary>
/// Utility for showing a popup window over the main content.
/// </summary>
public class Popup : RootConsoleContainer {
	private Action close;

	private Popup(ConsoleComponent content, Action close) : base(content) {
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
	public static bool Show(ConsoleComponent content) {
		bool open = true;
		bool result = false;
		ConsoleContainer popupContainer = new ConsoleContainer("", "[grow, center]", "[grow, center]");
		popupContainer.Opaque = false;
		Popup popup = new Popup(popupContainer, () => open = false);

		ConsoleContainer contentContainer = new ConsoleContainer("wrap 1", "[grow, fill]", "[grow, fill][]");
		contentContainer.SetBorder(1, 1, 1, 1);
		contentContainer.SetPadding(0, 0, 1, 1);
		contentContainer.Add(content);
		LabelButton confirmButton = new LabelButton("(C)onfirm", () => {
			open = false;
			result = true;
		});
		confirmButton.Hotkey = ConsoleKey.C;
		confirmButton.MarginTop = 1;
		contentContainer.Add(confirmButton);
		popupContainer.Add(contentContainer);

		ConsoleRenderManager.Instance.AddContainer(popup);
		while (open) {
			Thread.Sleep(10);
		}

		ConsoleRenderManager.Instance.RemoveContainer(popup);

		return result;
	}

	public static string? ShowInput(string label) {
		ConsoleContainer container = new ConsoleContainer("", "[grow, fill]", "[][grow, fill]");
		container.SetBorder(1, 1, 1, 1);
		container.SetPadding(0, 0, 1, 1);
		container.Add(new ConsoleLabel(label));
		ConsoleInputField inputField = new ConsoleInputField();
		container.Add(inputField);
		if (Show(container)) {
			return inputField.Content;
		} else {
			return null;
		}
	}
}