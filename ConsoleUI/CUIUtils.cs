using System.Drawing;

namespace ConsoleUI;

public static class CUIUtils {
	/// <summary>
	/// Gets the root container of the given component.
	/// </summary>
	/// <param name="component">the component to find to root of</param>
	/// <returns>The corresponding RootConsoleContainer, or null if the component is not located within a root.</returns>
	public static CUIRootContainer? GetRoot(CUIComponent? component) {
		for (int i = 0; i < 9999; i++) {
			if (component == null || component is CUIRootContainer) return component as CUIRootContainer;
			component = component.Parent;
		}
		throw new InvalidOperationException("Cannot find root container, possibly caused by a loop in the CUI hierarchy.");
	}

	/// <summary>
	/// Checks if the given container, or any of its children (recursively) contains the given component.
	/// </summary>
	/// <param name="container">the container to search in</param>
	/// <param name="component">the component to search for</param>
	/// <returns>true if the component is somewhere within the container, false otherwise</returns>
	public static bool Contains(CuiContainer container, CUIComponent component) {
		foreach (CUIComponent child in AccumulateAll(container, null)) {
			if (child == component) {
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Gets all the components in the given container, and all its children (recursively), that match the given predicate.
	/// </summary>
	/// <param name="container">the container to get components from</param>
	/// <param name="predicate">the predicate to check each component against, or null to allow all</param>
	/// <returns>a list of all components within the given container that pass the given predicate</returns>
	public static List<CUIComponent> AccumulateAll(CuiContainer container, Func<CUIComponent, bool>? predicate) {
		List<CUIComponent> result = new List<CUIComponent>();
		AccumulateAll(result, container, predicate);
		return result;
	}

	private static void AccumulateAll(List<CUIComponent> list, CuiContainer container, Func<CUIComponent, bool>? predicate) {
		foreach (CUIComponent child in container.GetAllComponents()) {
			if (predicate == null || predicate(child)) {
				list.Add(child);
			}

			if (child is CuiContainer) {
				AccumulateAll(list, (CuiContainer)child, predicate);
			}
		}
	}

	/// <summary>
	/// Converts given digit to a ConsoleKey object.
	/// </summary>
	/// <param name="number">A number, must be between 0 and 9.</param>
	/// <returns>the corresponding ConsoleKey</returns>
	/// <exception cref="ArgumentException">if the number was not between 0 and 9</exception>
	public static ConsoleKey ParseConsoleKey(uint number) {
		switch (number) {
			case 0:
				return ConsoleKey.D0;
			case 1:
				return ConsoleKey.D1;
			case 2:
				return ConsoleKey.D2;
			case 3:
				return ConsoleKey.D3;
			case 4:
				return ConsoleKey.D4;
			case 5:
				return ConsoleKey.D5;
			case 6:
				return ConsoleKey.D6;
			case 7:
				return ConsoleKey.D7;
			case 8:
				return ConsoleKey.D8;
			case 9:
				return ConsoleKey.D9;
			default:
				throw new ArgumentException(number + " is not a number on the keyboard.");
		}
	}

	/// <summary>
	/// Creates a prefix that can set the given color in the console
	/// </summary>
	/// <param name="color">the color to be set</param>
	/// <param name="foreground">if true, the prefix changes the foreground, otherwise it changes the background</param>
	/// <returns>the prefix to change colors with</returns>
	public static string CreateColorPrefix(Color color, bool foreground = true) {
		return "\x1b[" + (foreground ? "38" : "48") + ";2;" + color.R + ";" + color.G + ";" + color.B + "m";
	}

	/// <summary>
	/// Adds a padding of the given characters to the edges of given buffer.
	/// </summary>
	/// <param name="buffer">the buffer to write to</param>
	/// <param name="top">the amount of top paddings</param>
	/// <param name="bottom">the amount of bottom paddings</param>
	/// <param name="left">the amount of left paddings</param>
	/// <param name="right">the amount of right paddings</param>
	/// <param name="topChar">the char to pad with at the top</param>
	/// <param name="bottomChar">the char to pad with at the bottom</param>
	/// <param name="leftChar">the char to pad with on the left</param>
	/// <param name="rightChar">the char to pad with on the right</param>
	/// <param name="cornerChar">the char to pad with in the corners</param>
	/// <returns>A child buffer that is limited to within the padding</returns>
	public static RenderBuffer Pad(RenderBuffer buffer, int top, int bottom, int left, int right, char topChar, char bottomChar, char leftChar, char rightChar, char cornerChar) {
		string topPadding = new string(cornerChar, left) + new string(topChar, buffer.GetWidth() - left - right) + new string(cornerChar, right);
		string bottomPadding = new string(cornerChar, left) + new string(bottomChar, buffer.GetWidth() - left - right) + new string(cornerChar, right);
		string leftPadding = new string(leftChar, left);
		string rightPadding = new string(rightChar, right);

		for (int y = 0; y < buffer.GetHeight(); y++) {
			buffer.SetAll(0, y, leftPadding); // left
			buffer.SetAll(buffer.GetWidth() - rightPadding.Length, y, rightPadding); // right
			if (y < top) buffer.SetAll(0, y, topPadding); // top
			else if (y >= buffer.GetHeight() - bottom) buffer.SetAll(0, y, bottomPadding); // bottom
		}

		return new RenderBuffer(buffer, left, top, buffer.GetWidth() - left - right, buffer.GetHeight() - top - bottom);
	}
}