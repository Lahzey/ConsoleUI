using System.Drawing;

namespace ConsoleUI;

public class CUIButton : CUILabel, IActivateable {
	
	private bool _disabled = false;
	/// <summary>
	/// If disabled, the button will no longer be focusable and cannot be activated by any means.
	/// </summary>
	public bool Disabled {
		get => _disabled;
		set {
			_disabled = value;
			Focusable = !_disabled;
			Rerender();
		}
	}

	public ConsoleKey Hotkey { get; set; }

	public Action? Action { get; set; }

	public CUIButton(string content, Action? action = null) : base(content) {
		Action = action;
		TextOrientation = CENTER;
		SetBorder(1, 1, 1, 1);
		SetPadding(0, 0, 1, 1);
		Focusable = true;
		BorderColor = Color.DimGray;
	}

	public override bool HandleInput(ConsoleKeyInfo keyInfo) {
		if (!_disabled && keyInfo.Key == ConsoleKey.Enter) {
			if (Action != null) Activate();
			return true;
		}

		return false;
	}

	public void Activate() {
		if (!Disabled) {
			if (Focusable) Focus();
			Action?.Invoke();
		}
	}

	public ConsoleKey GetHotkey() {
		return Hotkey;
	}
}