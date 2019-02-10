namespace GridView
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using CoreGraphics;
    using UIKit;

    public partial class Grid : UIView
    {
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
                        this.AddSubview(cell.View);
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
                            pos[col, row] = new CGPoint(x, y);
                            x += width;
                        }
                    }

                    y += height;
                }
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

        public override void LayoutSubviews()
        {
            string debugIndent = String.Concat(Enumerable.Repeat("   ", numSuperviews(this)));
            LogLine($"{debugIndent}LayoutSubviews {GetHashCode()}");
            base.LayoutSubviews();

            this.UpdateLayout();

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
            LogLine($"{debugIndent}   newGridFrame={newGridFrame}, Frame={Frame}");
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
                LogLine($"{debugIndent}   Laying out tag " + cell.Position.Tag + ", " +
                    cell.View.GetType());

                if (!cell.IncludeInAutoSizeCalcs)
                {
                    LogLine($"{debugIndent}      skipping: View.Hidden=true and CollapseHidden=true");
                    continue;
                }

                cell.View.LayoutSubviews();

                var position = positions[cell.Position.Column, cell.Position.Row];
                var cellSize = GetCellAbsoluteSize(absoluteColumnWidth, absoluteRowHeight, cell.Position);
                var viewSize = cell.View.Frame.Size;

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
                        // Ignore Margin
                        position.Y += (cellSize.Height / 2) - (viewSize.Height / 2);
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
                        // Ignore Margin
                        position.X += (cellSize.Width / 2) - (viewSize.Width / 2);
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

                if (cell.Position.NoResize)
                {
                    cellSize = cell.View.Frame.Size;
                }
                var newFrame = new CGRect(position, cellSize);
                Log($"{debugIndent}      newFrame={newFrame}, cell.View.Frame={cell.View.Frame}");
                if (newFrame != cell.View.Frame)
                {
                    Log(": UPDATE");
                    cell.View.Frame = newFrame;
                }
                LogLine();
            }

            LogLine();
        }


        #endregion

        [Conditional("DEBUG")]
        internal static void Log(string msg = "")
        {
#if ENABLE_DEBUG_LOG
            Console.Write(msg);
#endif
        }

        [Conditional("DEBUG")]
        internal static void LogLine(string msg = "")
        {
#if ENABLE_DEBUG_LOG
            Console.WriteLine(msg);
#endif
        }
    }
}
