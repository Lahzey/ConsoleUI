namespace ConsoleUI;

/// <summary>
/// Simple interface for components that can be activated by hotkey and possibly other means.
/// </summary>
public interface IActivateable {
	public void Activate();
	public ConsoleKey GetHotkey();
}