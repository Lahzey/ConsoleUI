using ConsoleUI.Constraints;

namespace ConsoleUI;

/// <summary>
/// A component that can contain other components.
/// Any component wanting to display child components should inherit from this class in order for them to be accounted for in the component tree.
/// </summary>
public class CUIContainer : CUIComponent
{
    private readonly ContainerConstraints containerConstraints;
    private readonly ColumnConstraints columnConstraints;
    private readonly RowConstraints rowConstraints;

    private readonly List<List<CUIComponent>> componentRows = new List<List<CUIComponent>>();

    private readonly Dictionary<CUIComponent, ComponentConstraints> componentFormatting = new Dictionary<CUIComponent, ComponentConstraints>();

    public CUIContainer() : this("", "", "")
    {
    }

    public CUIContainer(string containerConstraints, string columnConstraints, string rowConstraints)
    {
        this.containerConstraints = ContainerConstraints.parse(containerConstraints);
        this.columnConstraints = ColumnConstraints.parse(columnConstraints);
        this.rowConstraints = RowConstraints.parse(rowConstraints);

        componentRows.Add(new List<CUIComponent>());
    }

    /// <summary>
    /// Adds the given component to the container.
    /// </summary>
    /// <param name="component">the component to add</param>
    /// <param name="formatting">the formatting for this specific component, potentially overwriting default formatting from the column and row constraints.</param>
    public void Add(CUIComponent component, string formatting = "")
    {
        int defaultXOrientation = columnConstraints.getDefaultComponentOrientation(componentRows.Last().Count);
        int defaultYOrientation = rowConstraints.getDefaultComponentOrientation(componentRows.Count - 1);
        componentFormatting.Add(component, ComponentConstraints.parse(formatting, defaultXOrientation, defaultYOrientation));
        componentRows.Last().Add(component);
        
        component.Parent = this;

        if (containerConstraints.AutoWrapAfter == componentRows.Last().Count) Wrap();
        
        Rerender();
    }

    /// <summary>
    /// Wraps this container to the next row.
    /// </summary>
    public void Wrap()
    {
        componentRows.Add(new List<CUIComponent>());
        Rerender();
    }


    /// <summary>
    /// Removes the given component from the container.
    /// </summary>
    /// <param name="component">the component to remove</param>
    /// <returns>true if the component was contained in this container, false otherwise</returns>
    public bool Remove(CUIComponent component)
    {
        // search componentRows for component
        componentFormatting.Remove(component);
        for (int i = 0; i < componentRows.Count; i++)
        {
            List<CUIComponent> row = componentRows[i];
            if (row.Remove(component)) // returns true if it contained the component, which means it has been found
            {
                // remove row if now empty
                if (row.Count == 0)
                {
                    componentRows.Remove(row);
                }

                // unfocus component if focused
                if (component.IsFocused())
                {
                    CUIRootContainer cuiRoot = CUIUtils.GetRoot(this);
                    if (cuiRoot != null) cuiRoot.FocusNext();
                }
                
                component.Parent = null;
                
                Rerender();
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Removes all components from this container.
    /// </summary>
    public void RemoveAll()
    {
        // get all components in this container
        List<CUIComponent> components = new List<CUIComponent>();
        foreach (List<CUIComponent> row in componentRows)
        {
            components.AddRange(row);
        }
        
        // check if any of them are focused and focus the next one
        CUIRootContainer cuiRoot = CUIUtils.GetRoot(this);
        foreach (CUIComponent component in components)
        {
            component.Parent = null;
            if (component.IsFocused() && cuiRoot != null) cuiRoot.FocusNext(components.ToArray());
        }
        
        // clear all contents
        componentFormatting.Clear();
        componentRows.Clear();
        componentRows.Add(new List<CUIComponent>());
        
        Rerender();
    }
    
    public int GetComponentCount()
    {
        int count = 0;
        foreach (List<CUIComponent> row in componentRows)
        {
            count += row.Count;
        }

        return count;
    }
    
    /// <summary>
    /// Creates a list of all components from all rows in this container, in the order they are added and rendered.
    /// </summary>
    /// <returns>the list of components</returns>
    public List<CUIComponent> GetAllComponents()
    {
        List<CUIComponent> components = new List<CUIComponent>();
        foreach (List<CUIComponent> row in componentRows)
        {
            components.AddRange(row);
        }

        return components;
    }

    /// <summary>
    /// Gets the component at the given index, returns the same values as GetAllComponents()[index].
    /// </summary>
    /// <param name="index">the index to search</param>
    /// <returns>the component at the given index</returns>
    public CUIComponent GetComponent(int index)
    {
        return GetAllComponents()[index];
    }
    
    /// <summary>
    /// Gets the components in the given row and column.
    /// </summary>
    /// <param name="row">the row to search</param>
    /// <param name="column">the column to search</param>
    /// <returns>the component at given row and column</returns>
    public CUIComponent GetComponent(int row, int column)
    {
        return componentRows[row][column];
    }

    /// <summary>
    /// Returns the index of given component in a list of all components (returned by GetAll)
    /// </summary>
    /// <param name="component">the component to get the index of</param>
    /// <returns>the index or -1, if this does not contain given component</returns>
    public int IndexOf(CUIComponent component)
    {
        return GetAllComponents().IndexOf(component);
    }

    protected override CUIComponent GetNext()
    {
        if (componentRows[0].Count == 0) return base.GetNext();
        return componentRows[0][0];
    }

    protected override int GetContentHeight() // returns the combined height of each row of components within this container
    {
        int height = 0;
        foreach (List<CUIComponent> row in componentRows)
        {
            int maxHeight = 0;
            foreach (CUIComponent component in row)
            {
                int componentHeight = component.GetHeight();
                if (componentHeight > maxHeight)
                {
                    maxHeight = componentHeight;
                }
            }

            height += maxHeight;
        }

        return height;
    }

    protected override int GetContentWidth()
    {
        int width = 0;
        foreach (int columnWidth in GetColumnWidths()) width += columnWidth;
        return width;
    }

    /// <summary>
    /// Gets the (minimal) width of each column in this container.
    /// </summary>
    /// <returns>the width each column in this container takes up, they can potentially expand to take up more space.</returns>
    private List<int> GetColumnWidths()
    {
        List<int> columnWidthList = new List<int>();
        foreach (List<CUIComponent> row in componentRows)
        {
            for (int i = 0; i < row.Count; i++)
            {
                if (columnWidthList.Count > i)
                {
                    columnWidthList[i] = Math.Max(columnWidthList[i], row[i].GetWidth());
                }
                else
                {
                    columnWidthList.Add(row[i].GetWidth());
                }
            }
        }

        return columnWidthList;
    }

    protected override void RenderContent(RenderBuffer buffer)
    {
        int[] columnWidths = CalculateColumnWidths(buffer.GetWidth());
        int[] rowHeights = CalculateRowHeights(buffer.GetHeight());

        int yOffset = 0;
        for (int rowIndex = 0; rowIndex < componentRows.Count; rowIndex++)
        {
            List<CUIComponent> row = componentRows[rowIndex];
            int rowHeight = rowHeights[rowIndex];

            int xOffset = 0;
            for (int columnIndex = 0; columnIndex < row.Count; columnIndex++)
            {
                CUIComponent component = row[columnIndex];
                ComponentConstraints constraints = componentFormatting[component];

                int cellWidth = columnWidths[columnIndex];
                int cellHeight = rowHeight;

                int componentWidth = constraints.orientationX == ComponentConstraints.FILL ? cellWidth : component.GetWidth();
                int componentHeight = constraints.orientationY == ComponentConstraints.FILL ? cellHeight : component.GetHeight();

                int componentX = xOffset;
                if (constraints.orientationX == ComponentConstraints.CENTER) componentX += (cellWidth - componentWidth) / 2;
                else if (constraints.orientationX == ComponentConstraints.RIGHT) componentX += cellWidth - componentWidth;

                int componentY = yOffset;
                if (constraints.orientationY == ComponentConstraints.CENTER) componentY += (cellHeight - componentHeight) / 2;
                else if (constraints.orientationY == ComponentConstraints.BOTTOM) componentY += cellHeight - componentHeight;

                component.Render(new RenderBuffer(buffer, componentX, componentY, componentWidth, componentHeight));

                xOffset += cellWidth;
            }

            yOffset += rowHeight;
        }
    }

    /// <summary>
    /// Calculates the width of each column given the total available width, taking constraints into account.
    /// </summary>
    /// <param name="totalWidth">the total available width</param>
    /// <returns>an array containing the width of each column in the order of the columns</returns>
    private int[] CalculateColumnWidths(int totalWidth)
    {
        List<int> columnWidthList = GetColumnWidths();

        int usedWidth = 0;
        foreach (int width in columnWidthList)
        {
            usedWidth += width;
        }

        int[] columnWidths = columnWidthList.ToArray();
        DistributeSizes(columnWidths, columnConstraints, totalWidth - usedWidth);

        return columnWidths;
    }

    /// <summary>
    /// Calculates the height of each row given the total available height, taking constraints into account.
    /// </summary>
    /// <param name="totalHeight">the total available height</param>
    /// <returns>an array containing the height of each row in the order of the rows</returns>
    private int[] CalculateRowHeights(int totalHeight)
    {
        int[] rowHeights = new int[componentRows.Count];
        if (componentRows.Count == 0) return rowHeights;

        int usedHeight = 0;
        for (int i = 0; i < componentRows.Count; i++)
        {
            int rowHeight = 0;
            foreach (CUIComponent component in componentRows[i])
            {
                rowHeight = Math.Max(rowHeight, component.GetHeight());
            }

            rowHeights[i] = rowHeight;
            usedHeight += rowHeight;
        }

        DistributeSizes(rowHeights, rowConstraints, totalHeight - usedHeight);

        return rowHeights;
    }

    /// <summary>
    /// Distributes additional size to the given sizes array, according to the given constraints.
    /// If no axis wants to grow, the sizes array will be left untouched.
    /// </summary>
    /// <param name="sizes">the currently allocated sizes</param>
    /// <param name="constraints">the constraints to control the allocation</param>
    /// <param name="toDistribute">the leftover size to be allocated</param>
    private static void DistributeSizes(int[] sizes, AxisConstraints constraints, int toDistribute)
    {
        // distributes to smallest first
        while (toDistribute > 0)
        {
            int smallestIndex = -1;
            for (int i = 0; i < sizes.Length; i++)
            {
                if (constraints.getGrowingFlag(i) && (smallestIndex < 0 || sizes[i] < sizes[smallestIndex]))
                {
                    smallestIndex = i;
                }
            }

            if (smallestIndex < 0) return; // indicates that no axis can grow, the size to be distributed will be left empty
            sizes[smallestIndex]++;
            toDistribute--;
        }
    }

    public bool FocusHierarchy() {
        if (Focusable) {
            Focus();
            return true;
        }

        return false;
    }
}