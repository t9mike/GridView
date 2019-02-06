namespace GridView
{
    using System;
    using System.Collections.Generic;
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
        /// horizontally and vertically within the bounds of outer_rect.
        /// This is useful when you are not nesting inner_view in a
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
        /// as specified within the bounds of outer_rect. This is useful when 
        /// you are not nesting inner_view in a larger layout.
        /// </summary>
        /// <seealso cref="CreateAutoCenterGrid"/>
        public static Grid CreateAlignmentGrid(UIView inner_view, CGRect outer_rect,
            Layout.Alignment horizAlignment, Layout.Alignment verticalAlignment)
        {
            var layout = new Layout()
                + inner_view.AddStackColumn((float)outer_rect.Width).
                    Horizontally(horizAlignment).
                    Vertically(verticalAlignment).Tag("AlignmentGrid");
            var grid = new Grid(layout);
            grid.Frame = outer_rect;
            return grid;
        }

        #region Layout

        /// <summary>
        /// If all columns have fixed or auto sizing, then set the width
        /// of the grid view automatically. Ignored if at least one column
        /// has percentage width spec.
        /// </summary>
        public bool AutoWidth = true;

        /// <summary>
        /// If all rows have fixed or auto sizing, then set the height
        /// of the grid view automatically. Ignored if at least one row
        /// has percentage height spec.
        /// </summary>
        public bool AutoHeight = true;

        private Layout currentLayout;
        private bool firstLayout = true;

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
                firstLayout = true;
            }
        }

        public void AddLayout(Layout layout, Func<Grid, bool> trigger = null)
        {
            layout.Trigger = trigger;
            this.layouts.Add(layout);
        }

        private CGPoint GetCellAbsolutePosition(nfloat[] absoluteColumnWidth, nfloat[] absoluteRowHeight, Layout.Position pos)
        {
            var position = new CGPoint(pos.Column * this.CurrentLayout.Spacing, pos.Row * this.CurrentLayout.Spacing);

            position.X += this.CurrentLayout.Padding.Left;
            position.Y += this.CurrentLayout.Padding.Top;

            for (int i = 0; i < pos.Column; i++)
                position.X += absoluteColumnWidth[i];

            for (int i = 0; i < pos.Row; i++)
                position.Y += absoluteRowHeight[i];

            return position;
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
            Console.WriteLine($"{debugIndent}LayoutSubviews {GetHashCode()}");
            base.LayoutSubviews();

            this.UpdateLayout();

            // Calculating sizes
            var absoluteRowHeight = this.CurrentLayout.CalculateAbsoluteRowHeight(this.Frame.Height);
            var absoluteColumnWidth = this.CurrentLayout.CalculateAbsoluteColumnWidth(this.Frame.Width);

            Console.WriteLine($"{debugIndent}   RowDefinitions      = [{string.Join(",", this.CurrentLayout.RowDefinitions.Select(r => r.Size))}]");
            Console.WriteLine($"{debugIndent}   absoluteRowHeight   = [{string.Join(",", absoluteRowHeight)}]");
            Console.WriteLine($"{debugIndent}   ColumnDefinitions   = [{string.Join(",", this.CurrentLayout.ColumnDefinitions.Select(r => r.Size))}]");
            Console.WriteLine($"{debugIndent}   absoluteColumnWidth = [{string.Join(",", absoluteColumnWidth)}]");

            // Layout subviews
            foreach (var cell in this.CurrentLayout.Cells)
            {
                Console.WriteLine();
                Console.WriteLine($"{debugIndent}   Laying out tag " + cell.Position.Tag + ", " +
                    cell.View.GetType());

                cell.View.LayoutSubviews();

                var position = GetCellAbsolutePosition(absoluteColumnWidth, absoluteRowHeight, cell.Position);
                var cellSize = GetCellAbsoluteSize(absoluteColumnWidth, absoluteRowHeight, cell.Position);
                var viewSize = cell.View.Frame.Size;

                Console.WriteLine($"{debugIndent}      position = {position}");
                Console.WriteLine($"{debugIndent}      cellSize = {cellSize}");
                Console.WriteLine($"{debugIndent}      viewSize = {viewSize}");


                if (viewSize.Width == 0 && viewSize.Height == 0)
                {
                    viewSize = cell.InitialSize;
                    Console.WriteLine($"{debugIndent}      viewSize changed to cell.InitialSize = {viewSize}");
                }
                bool layout = false;

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
                        layout = true;
                        break;

                    case Layout.Alignment.Start:
                        // Honor Margin.Top
                        position.Y += cell.Position.Margin.Top;
                        cellSize.Height = viewSize.Height;
                        layout = true;
                        break;

                    case Layout.Alignment.End:
                        // Honor Margin.Bottom
                        position.Y += cellSize.Height - viewSize.Height - cell.Position.Margin.Bottom;
                        cellSize.Height = viewSize.Height;
                        layout = true;
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
                        layout = true;
                        break;

                    case Layout.Alignment.Start:
                        // Honor Margin.Left
                        cellSize.Width = viewSize.Width;
                        position.X += cell.Position.Margin.Left;
                        layout = true;
                        break;

                    case Layout.Alignment.End:
                        // Honor Margin.Right
                        position.X += cellSize.Width - viewSize.Width - cell.Position.Margin.Right;
                        cellSize.Width = viewSize.Width;
                        layout = true;
                        break;

                    default:
                        break;
                }

                if (cell.Position.NoResize)
                {
                    cellSize = cell.View.Frame.Size;
                }
                var newFrame = new CGRect(position, cellSize);
                Console.Write($"{debugIndent}      newFrame={newFrame}, cell.View.Frame={cell.View.Frame}");
                if (newFrame != cell.View.Frame)
                {
                    Console.Write(": UPDATE");
                    cell.View.Frame = newFrame;
                }
                Console.WriteLine();
            }

            Console.WriteLine();

            if (AutoWidth && !this.CurrentLayout.ColumnDefinitions.Any(c => c.SizeType == Layout.SizeType.Percentage))
            {
                nfloat min_left = this.CurrentLayout.Cells.Where(c => c.IncludeInAutoSizeCalcs).Min(c => c.View.Frame.X);
                nfloat max_right = this.CurrentLayout.Cells.Where(c => c.IncludeInAutoSizeCalcs).Max(c => c.View.Frame.Right);
                nfloat newWidth = max_right - min_left;

                Console.Write($"{debugIndent}   AutoWidth newWidth={newWidth}, Frame.Width={Frame.Width}");
                if (Frame.Width != newWidth)
                {
                    Console.Write(": UPDATE");
                    this.SetWidth(newWidth);
                }
                Console.WriteLine();
            }

            if (AutoHeight && !this.CurrentLayout.RowDefinitions.Any(c => c.SizeType == Layout.SizeType.Percentage))
            {
                nfloat min_top = this.CurrentLayout.Cells.Where(c => c.IncludeInAutoSizeCalcs).Min(c => c.View.Frame.Y);
                nfloat max_bottom = this.CurrentLayout.Cells.Where(c => c.IncludeInAutoSizeCalcs).Max(c => c.View.Frame.Bottom);
                nfloat newHeight = max_bottom - min_top;

                Console.Write($"{debugIndent}   AutoHeight newHeight={newHeight}, Frame.Height={Frame.Height}");
                if (Frame.Height != newHeight)
                {
                    Console.Write($": UPDATE");
                    this.SetHeight(newHeight);
                }
                Console.WriteLine();
            }

            firstLayout = false;
        }


        #endregion

    }
}
