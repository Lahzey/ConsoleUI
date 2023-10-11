namespace ConsoleUI;

public class CUIRootContainer : CUIContainer {
	private CUIComponent currentlyFocused = null;

	public readonly Dictionary<char, Action> Hotkeys = new Dictionary<char, Action>();

	public CUIRootContainer(CUIComponent content) : base("", "[grow, fill]", "[grow, fill]") {
		base.Add(content);
	}

	private void Render() {
		// using the full width will cause the console to display empty lines between each line, and the console scrolls to one line below content
		RenderBuffer renderBuffer = new RenderBuffer(Console.WindowWidth - 1, Console.WindowHeight - 1);
		Render(renderBuffer);
		Console.Clear();
		Console.Write(renderBuffer);
	}

	public void Focus(CUIComponent component) {
		if (currentlyFocused != null && currentlyFocused.OnFocusLost != null) currentlyFocused.OnFocusLost();
		currentlyFocused = component;
		if (currentlyFocused != null && currentlyFocused.OnFocusGained != null) currentlyFocused.OnFocusGained();
		CUIRenderManager.Instance.Rerender();
	}

	public void FocusNext(params CUIComponent[] blockedFromFocus) {
		List<CUIComponent> focusableComponents = CUIUtils.AccumulateAll(this, comp => comp.Focusable);
		int currentFocusIndex = currentlyFocused == null ? -1 : focusableComponents.IndexOf(currentlyFocused);
		int startIndex = focusableComponents.Count > currentFocusIndex + 1 ? currentFocusIndex + 1 : 0;

		for (int i = startIndex; i != currentFocusIndex; i = i + 1 < focusableComponents.Count ? i + 1 : 0) {
			CUIComponent component = focusableComponents[i];
			bool isBlocked = false;
			foreach (CUIComponent blocked in blockedFromFocus) {
				if (component == blocked || (blocked is CUIContainer && CUIUtils.Contains((CUIContainer)blocked, component))) {
					isBlocked = true;
					continue;
				}
			}

			if (!isBlocked) {
				Focus(component);
				return;
			}
		}

		Focus(null); // if no other component can be focused, focus nothing
	}

	public CUIComponent getFocused() {
		return currentlyFocused;
	}

	protected virtual void Exit() {
		new Thread(() => {
			if (CUIPopup.Show(new CUILabel("Are you sure you want to quit?"))) {
				Environment.Exit(0);
			}
		}).Start();
	}

	public override bool HandleInput(ConsoleKeyInfo key) {
		if (key.Key == ConsoleKey.Tab) {
			FocusNext();
			return true;
		}
		else if (key.Key == ConsoleKey.Escape) {
			Exit();
			return true;
		}

		bool triggerHotkeys = true;
		if (currentlyFocused != null) {
			triggerHotkeys = !currentlyFocused.HandleInput(key);
		}

		if (triggerHotkeys) {
			foreach (IActivateable activateable in CUIUtils.AccumulateAll(this, (comp) => comp is IActivateable)) {
				if (activateable.GetHotkey() == key.Key) {
					activateable.Activate();
					break;
				}
			}
		}

		return true;
	}
}