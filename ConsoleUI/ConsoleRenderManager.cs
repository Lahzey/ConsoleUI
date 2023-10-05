namespace ConsoleUI;

public class ConsoleRenderManager
{
    private static readonly int FRAME_DELAY = 1000 / 60;
    
    /// <summary>
    /// Can be used to execute code without risking to interfere with the rendering.
    /// </summary>
    public static readonly object RENDER_LOCK = new object();
    
    public static readonly ConsoleRenderManager Instance = new ConsoleRenderManager();
    
    private Thread refresher;
    private Thread inputPoller;

    private bool renderRequested = false;
    private int lastWidth = 0;
    private int lastHeight = 0;
    
    private readonly List<RootConsoleContainer> containers = new List<RootConsoleContainer>();
    
    private ConsoleRenderManager()
    {
        refresher = new Thread(new ThreadStart(Refresh));
        refresher.Start();
        
        inputPoller = new Thread(new ThreadStart(PollInput));
        inputPoller.Start();
    }
    
    /// <summary>
    /// Adds the given root the the list of containers to be rendered.
    /// Containers are rendered on top of each other and in the order they are added.
    /// Only the top container may receive any input.
    /// </summary>
    /// <param name="container"></param>
    public void AddContainer(RootConsoleContainer container)
    {
        lock (RENDER_LOCK)
        {
            containers.Add(container);
            Rerender();
        }
    }
    
    public void RemoveContainer(RootConsoleContainer container)
    {
        lock (RENDER_LOCK)
        {
            containers.Remove(container);
            Rerender();
        }
    }
    
    /// <summary>
    /// Requests that a rerender be performed.
    /// The rerender happens asynchronously and not instantly.
    /// </summary>
    public void Rerender()
    {
        renderRequested = true;
    }

    private void Render()
    {
        lock(RENDER_LOCK) {
            // using the full width will cause the console to display empty lines between each line, and the console scrolls to one line below content
            RenderBuffer renderBuffer = new RenderBuffer(Console.WindowWidth - 1, Console.WindowHeight - 1);
            
            // rendering each container in on top of each other, allowing for popups and other overlays
            foreach (RootConsoleContainer container in containers)
            {
                container.Render(renderBuffer);
            }
            
            renderBuffer.RenderToConsole();
        }
    }
    
    private void HandleInput(ConsoleKeyInfo key)
    {
        lock(RENDER_LOCK)
        {
            if (containers.Count > 0)
            {
                containers[^1].HandleInput(key);
            }
        }
    }
    
    private void PollInput()
    {
        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey(true);
            HandleInput(key);
        }
    }

    private void Refresh()
    {
        while (true)
        {
            Thread.Sleep(FRAME_DELAY);
            if (renderRequested || lastWidth != Console.WindowWidth || lastHeight != Console.WindowHeight)
            {
                renderRequested = false;
                lastWidth = Console.WindowWidth;
                lastHeight = Console.WindowHeight;
                Render();
            }
        }
    }
}