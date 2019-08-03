namespace GridView
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using CoreGraphics;
    using UIKit;

    public partial class Grid : UIView
    {
        /// <summary>
        /// Set to true to enable layout logging in DEBUG compile mode.
        /// Default is off. Logging is never performed in non-DEBUG
        /// mode.
        /// </summary>
        public static bool Enable_Debug_Log = false;

        /// <summary>
        /// Fired after the grid has performed a layout.
        /// </summary>
        public event Action<LayoutCompletedEventArgs> LayoutCompleted;

        public class LayoutCompletedEventArgs : EventArgs
        {
            public readonly CGSize OldSize;
            public readonly CGSize NewSize;
            public bool SizeChanged => OldSize != NewSize;

            public LayoutCompletedEventArgs(CGSize oldSize, 
                CGSize newSize)
            {
                OldSize = oldSize;
                NewSize = newSize;
            }
        }

        public Grid()
        {

        }

        /// <summary>
        /// Adds a default layout.
        /// </summary>
        public Grid(Layout layout)
        {
            AddLayout(layout);
        }

        /// <summary>
        /// Creates a Grid that centers the supplied inner_view both
        /// horizontally and vertically within the bounds of outerRect.
        /// This is useful when you are not nesting innerView in a
        /// larger layout.
        /// </summary>
        /// <seealso cref="CreateAlignmentGrid"/>
        public static Grid CreateAutoCenterGrid(UIView innerView, CGRect outerRect)
        {
            return CreateAlignmentGrid(innerView, outerRect, 
                Layout.Alignment.Center, Layout.Alignment.Center);
        }

        /// <summary>
        /// Creates a Grid that aligns inner_view horizontally and vertically 
        /// as specified within the bounds of outerRect. This is useful when 
        /// you are not nesting innerView in a larger layout.
        /// </summary>
        /// <seealso cref="CreateAutoCenterGrid"/>
        public static Grid CreateAlignmentGrid(UIView innerView, CGRect outerRect,
            Layout.Alignment horizAlignment, Layout.Alignment verticalAlignment)
        {
            var layout = new Layout().WithColumns((float)outerRect.Width).WithRows((float)outerRect.Height)
                + innerView.At(0, 0).Horizontally(horizAlignment).Vertically(verticalAlignment).Tag("AlignmentGrid");
            var grid = new Grid(layout);
            grid.Frame = outerRect;
            return grid;
        }

        public void SwapInView(UIView oldView, UIView newView)
        {
            var cell = CurrentLayout.FindCell(oldView);
            if (cell == null)
            {
                throw new ArgumentException($"oldView {oldView} is not in the Grid");
            }
            cell.View = newView;
            oldView.RemoveFromSuperview();
            AddSubview(newView);
            SetNeedsLayout();
        }


        #region Layout

        /// <summary>
        /// Sets width automatically. Note that if a column uses % sizing,
        /// the size of the cell will define a minimum width for the cell
        /// when AutoWidth is true (crude sizing algorithm).
        /// </summary>
        public bool AutoWidth = true;

        /// <summary>
        /// Sets height automatically. Note that if a row uses % sizing,
        /// the size of the cell will define a minimum height for the cell
        /// when AutoHeight is true (crude sizing algorithm).
        /// </summary>
        public bool AutoHeight = true;

        private Layout currentLayout;

        private List<Layout> layouts = new List<Layout>();

        public Layout CurrentLayout
        {
            get
            {
                if (this.currentLayout == null)
                    this.UpdateLayout();
                return this.currentLayout;
            }
        }

        public void UpdateLayout()
        {

            var layout = this.layouts.FirstOrDefault(l => l.Trigger?.Invoke(this) ?? false) ?? this.layouts.FirstOrDefault(l => l.Trigger == null);

            if (currentLayout != layout)
            {
                this.currentLayout = layout;

                if (layout != null)
                {
                    foreach (var cell in this.Subviews.Where(v => !layout.Cells.Any(c => c.View == v)))
                    {
                        cell.RemoveFromSuperview();
                    }

                    foreach (var cell in layout.Cells.Where(v => !this.Subviews.Contains(v.View)))
                    {
                        if (cell.View != null)
                        {
                            this.AddSubview(cell.View);
                        }
                    }
                }
            }
        }

        public void AddLayout(Layout layout, Func<Grid, bool> trigger = null)
        {
            layout.Trigger = trigger;
            this.layouts.Add(layout);
        }

        /// <summary>
        /// Calculate position of all cells, Returns two-dimensional array [col,row].
        /// </summary>
        private CGPoint[,] CalcCellAbsolutePositions(nfloat[] absoluteColumnWidth, nfloat[] absoluteRowHeight)
        {
            var pos = new CGPoint[absoluteColumnWidth.Count(), absoluteRowHeight.Count()];

            nfloat y = this.CurrentLayout.Padding.Top;
            bool firstRealRow = true;

            for (int row = 0; row < absoluteRowHeight.Count(); row++)
            {
                nfloat height = absoluteRowHeight[row];
                if (height > 0)
                {
                    if (firstRealRow)
                    {
                        firstRealRow = false;
                    }
                    else
                    {
                        y += this.CurrentLayout.Spacing;
                    }
                }

                nfloat x = this.CurrentLayout.Padding.Left;
                bool firstRealCol = true;

                for (int col = 0; col < absoluteColumnWidth.Count(); col++)
                {
                    nfloat width = absoluteColumnWidth[col];
                    if (width > 0)
                    {
                        if (firstRealCol)
                        {
                            firstRealCol = false;
                        }
                        else
                        {
                            x += this.CurrentLayout.Spacing;
                        }
                    }
                    pos[col, row] = new CGPoint(x, y);
                    x += width;
                }

                y += height;
            }

            return pos;
        }

        private CGSize GetCellAbsoluteSize(nfloat[] absoluteColumnWidth, nfloat[] absoluteRowHeight, Layout.Position pos)
        {
            var size = new CGSize((pos.ColumnSpan - 1) * this.CurrentLayout.Spacing, (pos.RowSpan - 1) * this.CurrentLayout.Spacing);

            for (int i = 0; i < pos.ColumnSpan; i++)
                size.Width += absoluteColumnWidth[pos.Column + i];

            for (int i = 0; i < pos.RowSpan; i++)
                size.Height += absoluteRowHeight[pos.Row + i];

            return size;
        }

        private int numSuperviews(UIView view)
        {
            int n = 0;
            while (view.Superview != null)
            {
                ++n;
                view = view.Superview;
            }
            return n;
        }

        /// <summary>
        /// This is useful in situations where you need the size of eventual
        /// layout before the view is loaded. To avoid hangs, a limit of
        /// # LayoutSubviews() is done.
        /// </summary>
        /// <param name="maxAttemps">Max LayoutSubviews() to perform. Default is 10.</param>
        public void LayoutSubviewsUntilSizeStable(int maxAttemps = 10)
        {
            int attempt = 0;
            while (++attempt <= maxAttemps)
            {
                var before = Frame.Size;
                LayoutSubviews();
                if (before == Frame.Size)
                {
                    return;
                }
            }
        }

        public override void LayoutSubviews()
        {
            var oldSize = Frame.Size;

            string debugIndent = String.Concat(Enumerable.Repeat("   ", numSuperviews(this)));
            LogLine($"{debugIndent}LayoutSubviews {GetHashCode()} {this.GetType()}");
            base.LayoutSubviews();

            this.UpdateLayout();

            // Allow streched cells that are in an auto sized column or row only if there is at
            // least one non-stretched cell that starts that same column/row
            foreach (var cell in this.CurrentLayout.Cells)             {                 if (cell.Position.Horizontal == Layout.Alignment.Stretched &&                     this.CurrentLayout.ColumnDefinitions[cell.Position.Column].SizeType == Layout.SizeType.Auto &&
                    !this.CurrentLayout.Cells.Any(c => c.Position.Column == cell.Position.Column && c.Position.Horizontal != Layout.Alignment.Stretched))
                {
                    throw new Exception($"Stretched horizontal cell alignment on row {cell.Position.Row}, col {cell.Position.Column} cannot be used in an auto sized column when there is no other cell in that column that is not stretched");
                }
                 if (cell.Position.Vertical == Layout.Alignment.Stretched &&
                    this.CurrentLayout.RowDefinitions[cell.Position.Row].SizeType == Layout.SizeType.Auto &&
                    !this.CurrentLayout.Cells.Any(c => c.Position.Row == cell.Position.Row && c.Position.Vertical != Layout.Alignment.Stretched))
                {
                    throw new Exception($"Stretched vertical cell alignment on row {cell.Position.Row}, col {cell.Position.Column} cannot be used in an auto sized row when there is no other cell in that column that is not stretched");
                }
            }

            LogLine($"{debugIndent}Cells at start of layout:");
            foreach (var cell in this.CurrentLayout.Cells)
            {
                LogLine($"{debugIndent}   {cell}");
            }

            var horizontallyStretchedCells = this.CurrentLayout.Cells.Where(c => c.IncludeInAutoWidthSizeCalcs && 
                c.View != null && c.Position.Horizontal == Layout.Alignment.Stretched);
            if (horizontallyStretchedCells.Any())
            {
                LogLine($"{debugIndent}   Setting width of horizontally stretched cells");
                var widths1 = this.CurrentLayout.CalculateAbsoluteColumnWidth(this).Item2;
                foreach (var cell in horizontallyStretchedCells)
                {
                    nfloat w = CalcSpanWidth(cell, widths1);
                    LogLine($"{debugIndent}      Setting cell.View Width to {w}");
                    cell.View.SetWidth(w - cell.Position.Margin.Width());
                    LogLine($"{debugIndent}      Before LayoutSubviews {cell.DebugLabel}");
                    cell.View.LayoutSubviews();
                    LogLine($"{debugIndent}      After LayoutSubviews cell.View Size={cell.View.Frame.Size} {cell.DebugLabel}");
                }
            }

            var verticallyStretchedCells = this.CurrentLayout.Cells.Where(c => c.IncludeInAutoWidthSizeCalcs &&
                c.View != null && c.Position.Vertical == Layout.Alignment.Stretched);
            if (verticallyStretchedCells.Any())
            {
                LogLine($"{debugIndent}   Setting height of vertically stretched cells");
                var heights1 = this.CurrentLayout.CalculateAbsoluteRowHeight(this).Item2;
                foreach (var cell in verticallyStretchedCells)
                {
                    nfloat h = CalcSpanHeight(cell, heights1);
                    LogLine($"{debugIndent}      Setting cell.View Height to {h}");
                    cell.View.SetHeight(h - cell.Position.Margin.Height());
                    LogLine($"{debugIndent}      Before LayoutSubviews {cell.DebugLabel}");
                    cell.View.LayoutSubviews();
                    LogLine($"{debugIndent}      After LayoutSubviews cell.View Size={cell.View.Frame.Size} {cell.DebugLabel}");
                }
            }

            // Calculate row sizes
            var heights = this.CurrentLayout.CalculateAbsoluteRowHeight(this);
            nfloat gridHeight = heights.Item1;
            nfloat[] absoluteRowHeight = heights.Item2;
            LogLine($"{debugIndent}   AutoHeight={AutoHeight}, gridHeight={gridHeight}, Frame.Height={Frame.Height}");

            // Calculate column sizes
            var widths = this.CurrentLayout.CalculateAbsoluteColumnWidth(this);
            nfloat gridWidth = widths.Item1;
            nfloat[] absoluteColumnWidth = widths.Item2;
            LogLine($"{debugIndent}   AutoWidth={AutoWidth} gridWidth={gridWidth}, Frame.Width={Frame.Width}");

            var newGridFrame = new CGRect(Frame.Location, new CGSize(gridWidth, gridHeight));
            LogLine($"{debugIndent}   newGridFrame={newGridFrame}, old Frame={Frame}");
            Frame = newGridFrame;

            LogLine($"{debugIndent}   RowDefinitions      = [{string.Join(",", this.CurrentLayout.RowDefinitions.Select(r => r.Size))}]");
            LogLine($"{debugIndent}   absoluteRowHeight   = [{string.Join(",", absoluteRowHeight)}]");
            LogLine($"{debugIndent}   ColumnDefinitions   = [{string.Join(",", this.CurrentLayout.ColumnDefinitions.Select(r => r.Size))}]");
            LogLine($"{debugIndent}   absoluteColumnWidth = [{string.Join(",", absoluteColumnWidth)}]");

            // Calculate cell positions
            var positions = CalcCellAbsolutePositions(absoluteColumnWidth, absoluteRowHeight);

            // Layout subviews
            foreach (var cell in this.CurrentLayout.Cells)
            {
                LogLine();
                LogLine($"{debugIndent}   Laying out col {cell.Position.Column}, row {cell.Position.Row}, {cell.DebugLabel}");

                if (!cell.IncludeInAutoWidthSizeCalcs && !cell.IncludeInAutoHeightSizeCalcs)
                {
                    LogLine($"{debugIndent}      skipping: View.Hidden=true and IncludeInAutoWidthSizeCalcs,IncludeInAutoHeightSizeCalcs false");
                    continue;
                }

                if (cell.Position.Row >= this.CurrentLayout.RowDefinitions.Count())
                {
                    throw new Exception($"a cell is defined at row {cell.Position.Row}, col {cell.Position.Column} but there are only {this.CurrentLayout.RowDefinitions.Count()} rows defined in the grid");
                }

                if (cell.Position.Column >= this.CurrentLayout.ColumnDefinitions.Count())
                {
                    throw new Exception($"a cell is defined at row {cell.Position.Row}, col {cell.Position.Column} but there are only {this.CurrentLayout.ColumnDefinitions.Count()} columns defined in the grid");
                }

                cell.View.LayoutSubviews();

                var position = positions[cell.Position.Column, cell.Position.Row];
                var cellSize = GetCellAbsoluteSize(absoluteColumnWidth, absoluteRowHeight, cell.Position);
                var viewSize = cell.AutoSizeSize;

                LogLine($"{debugIndent}      position = {position}");
                LogLine($"{debugIndent}      cellSize = {cellSize}");
                LogLine($"{debugIndent}      viewSize = {viewSize}");

                if (viewSize.Width == 0 && viewSize.Height == 0)
                {
                    viewSize = cell.InitialSize;
                    LogLine($"{debugIndent}      viewSize changed to cell.InitialSize = {viewSize}");
                }

                LogLine($"{debugIndent}      Alignment: Vertical={cell.Position.Vertical}, Horizontal={cell.Position.Horizontal}");

                switch (cell.Position.Vertical)
                {
                    case Layout.Alignment.Stretched:
                        // Honor Margin.Top and Bottom
                        position.Y += cell.Position.Margin.Top;
                        cellSize.Height -= cell.Position.Margin.Height();
                        break;

                    case Layout.Alignment.Center:
                        // Honor Margin.Left and Right
                        position.Y += (cellSize.Height / 2f) - (viewSize.Height / 2f) + cell.Position.Margin.Top/2f - cell.Position.Margin.Bottom/2f;
                        cellSize.Height = viewSize.Height;
                        break;

                    case Layout.Alignment.Start:
                        // Honor Margin.Top
                        position.Y += cell.Position.Margin.Top;
                        cellSize.Height = viewSize.Height;
                        break;

                    case Layout.Alignment.End:
                        // Honor Margin.Bottom
                        position.Y += cellSize.Height - viewSize.Height - cell.Position.Margin.Bottom;
                        cellSize.Height = viewSize.Height;
                        break;

                    default:
                        break;
                }

                switch (cell.Position.Horizontal)
                {
                    case Layout.Alignment.Stretched:
                        // Honor Margin.Left and Right
                        position.X += cell.Position.Margin.Left;
                        cellSize.Width -= cell.Position.Margin.Width();
                        break;

                    case Layout.Alignment.Center:
                        // Honor Margin.Left and Right
                        position.X += (cellSize.Width / 2f) - (viewSize.Width / 2f) + cell.Position.Margin.Left / 2f - cell.Position.Margin.Right / 2f;
                        cellSize.Width = viewSize.Width;
                        break;

                    case Layout.Alignment.Start:
                        // Honor Margin.Left
                        cellSize.Width = viewSize.Width;
                        position.X += cell.Position.Margin.Left;
                        break;

                    case Layout.Alignment.End:
                        // Honor Margin.Right
                        position.X += cellSize.Width - viewSize.Width - cell.Position.Margin.Right;
                        cellSize.Width = viewSize.Width;
                        break;

                    default:
                        break;
                }

                if (cell.Position.NoResize || cell.Position.NoPosition)
                {
                    cellSize = cell.AutoSizeSize;
                }
                if (cell.Position.NoPosition)
                {
                    position = cell.View.Frame.Location;
                }

                var newFrame = new CGRect(position, cellSize);
                Log($"{debugIndent}      newFrame={newFrame}, cell.View.Frame={cell.View?.Frame}");
                if (newFrame != cell.View.Frame)
                {
                    Log(": UPDATE");
                    SetCellFrame(cell, newFrame);
                }
                LogLine();
                Log($"{debugIndent}      Finished {cell.DebugLabel}");
            }

            LogLine();

            OnLayoutCompleted(oldSize, Frame.Size);
        }

        /// <summary>
        /// This is called when the layout system is defining the size and position
        /// of a cell. You could override to perform additional tasks. It is only
        /// called when the cell's frame would change.
        /// </summary>
        /// <remarks>
        /// You can use cell.Position.Tag, cell.Position.Column/Row, or inspect 
        /// cell.View's type to do something special based on the cell.</remarks>
        protected virtual void SetCellFrame(Layout.Cell cell, CGRect frame)
        {
            cell.View.Frame = frame;
        }


        #endregion

        protected virtual void OnLayoutCompleted(CGSize oldSize, CGSize newSize)
        {
            if (LayoutCompleted != null)
            {
                var e = new LayoutCompletedEventArgs(oldSize, newSize);
                LayoutCompleted(e);
            }
        }

        private nfloat CalcSpanWidth(Layout.Cell cell, nfloat[] columnWidth)
        {
            nfloat spanWidth = 0;
            for (int col = cell.Position.Column; col < cell.Position.Column + cell.Position.ColumnSpan; col++)
            {
                spanWidth += columnWidth[col];
            }
            return spanWidth;
        }

        private nfloat CalcSpanHeight(Layout.Cell cell, nfloat[] columnHeight)
        {
            nfloat spanHeight = 0;
            for (int row = cell.Position.Row; row < cell.Position.Row + cell.Position.RowSpan; row++)
            {
                spanHeight += columnHeight[row];
            }
            return spanHeight;
        }

        private bool Disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (!Disposed && disposing)
            {
                foreach (var layout in layouts)
                {
                    foreach (var cell in layout.Cells)
                    {
                        cell.View?.Dispose();
                    }
                }
                layouts.Clear();
            }
            base.Dispose(disposing);
            Disposed = true;
        }

        // Log() and LogLine() will be optimized away in non-DEBUG
        // compile modes, so these calls will not effect performande
        // of the real app

        [Conditional("DEBUG")]
        internal static void Log(string msg = "")
        {
#if ENABLE_DEBUG_LOG
            if (Enable_Debug_Log)
            {
                Debug.Write(msg);
            }
#endif
        }

        [Conditional("DEBUG")]
        internal static void LogLine(string msg = "")
        {
#if ENABLE_DEBUG_LOG
            if (Enable_Debug_Log)
            {
                Debug.WriteLine(msg);
            }
#endif
        }
    }
}
