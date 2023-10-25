namespace ConsoleUI;

public class CUIRenderManager {
	private static readonly int FRAME_DELAY = 1000 / 60;

	/// <summary>
	/// Can be used to execute code without risking to interfere with the rendering.
	/// </summary>
	public static readonly object RENDER_LOCK = new object();

	public static readonly CUIRenderManager Instance = new CUIRenderManager();

	private readonly Thread Refresher;
	private readonly Thread InputPoller;

	private bool RenderRequested = false;
	private int LastWidth = 0;
	private int LastHeight = 0;

	private readonly List<CUIRootContainer> Containers = new List<CUIRootContainer>();

	private CUIRenderManager() {
		Refresher = new Thread(Refresh);
		Refresher.Start();

		InputPoller = new Thread(PollInput);
		InputPoller.Start();
	}

	/// <summary>
	/// Adds the given root the the list of containers to be rendered.
	/// Containers are rendered on top of each other and in the order they are added.
	/// Only the top container may receive any input.
	/// </summary>
	/// <param name="container"></param>
	public void AddContainer(CUIRootContainer container) {
		lock (RENDER_LOCK) {
			Containers.Add(container);
			Rerender();
		}
	}

	public void RemoveContainer(CUIRootContainer container) {
		lock (RENDER_LOCK) {
			Containers.Remove(container);
			Rerender();
		}
	}

	/// <summary>
	/// Requests that a rerender be performed.
	/// The rerender happens asynchronously and not instantly.
	/// </summary>
	public void Rerender() {
		RenderRequested = true;
	}

	private void Render() {
		lock (RENDER_LOCK) {
			// using the full width will cause the console to display empty lines between each line, and the console scrolls to one line below content
			RenderBuffer renderBuffer = new RenderBuffer(Console.WindowWidth - 1, Console.WindowHeight - 1);

			// rendering each container in on top of each other, allowing for popups and other overlays
			foreach (CUIRootContainer container in Containers) {
				container.Render(renderBuffer);
			}

			renderBuffer.RenderToConsole();
		}
	}

	private void HandleInput(ConsoleKeyInfo key) {
		lock (RENDER_LOCK) {
			if (Containers.Count > 0) {
				Containers[^1].HandleInput(key);
			}
		}
	}

	private void PollInput() {
		while (true) {
			ConsoleKeyInfo key = Console.ReadKey(true);
			HandleInput(key);
		}
	}

	private void Refresh() {
		while (true) {
			Thread.Sleep(FRAME_DELAY);
			if (RenderRequested || LastWidth != Console.WindowWidth || LastHeight != Console.WindowHeight) {
				RenderRequested = false;
				LastWidth = Console.WindowWidth;
				LastHeight = Console.WindowHeight;
				Render();
			}
		}
	}
}