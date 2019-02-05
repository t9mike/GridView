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

            // Layout subviews
            foreach (var cell in this.CurrentLayout.Cells)
            {
                Console.WriteLine($"{debugIndent}   Laying out tag " + cell.Position.Tag);

                cell.View.LayoutSubviews();

                var position = GetCellAbsolutePosition(absoluteColumnWidth, absoluteRowHeight, cell.Position);
                var cell_size = GetCellAbsoluteSize(absoluteColumnWidth, absoluteRowHeight, cell.Position);
                var view_size = cell.View.Frame.Size;
                if (view_size.Width == 0 && view_size.Height == 0)
                {
                    view_size = cell.InitialSize;
                }
                bool layout = false;

                switch (cell.Position.Vertical)
                {
                    case Layout.Alignment.Stretched:
                        // Honor Margin.Top and Bottom
                        position.Y += cell.Position.Margin.Top;
                        cell_size.Height -= cell.Position.Margin.Height();
                        break;

                    case Layout.Alignment.Center:
                        // Ignore Margin
                        position.Y += (cell_size.Height / 2) - (view_size.Height / 2);
                        cell_size.Height = view_size.Height;
                        layout = true;
                        break;

                    case Layout.Alignment.Start:
                        // Honor Margin.Top
                        position.Y += cell.Position.Margin.Top;
                        cell_size.Height = view_size.Height;
                        layout = true;
                        break;

                    case Layout.Alignment.End:
                        // Honor Margin.Bottom
                        position.Y += cell_size.Height - view_size.Height - cell.Position.Margin.Bottom;
                        cell_size.Height = view_size.Height;
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
                        cell_size.Width -= cell.Position.Margin.Width();
                        break;

                    case Layout.Alignment.Center:
                        // Ignore Margin
                        position.X += (cell_size.Width / 2) - (view_size.Width / 2);
                        cell_size.Width = view_size.Width;
                        layout = true;
                        break;

                    case Layout.Alignment.Start:
                        // Honor Margin.Left
                        cell_size.Width = view_size.Width;
                        position.X += cell.Position.Margin.Left;
                        layout = true;
                        break;

                    case Layout.Alignment.End:
                        // Honor Margin.Right
                        position.X += cell_size.Width - view_size.Width - cell.Position.Margin.Right;
                        cell_size.Width = view_size.Width;
                        layout = true;
                        break;

                    default:
                        break;
                }

                //if (layout)
                //{
                //}
                if (cell.Position.NoResize)
                {
                    cell_size = cell.View.Frame.Size;
                }
                var newFrame = new CGRect(position, cell_size);
                if (newFrame != cell.View.Frame)
                {
                    Console.WriteLine($"{debugIndent}   Changing frame from {cell.View.Frame} to {newFrame}");
                    cell.View.Frame = newFrame;
                }
            }

            if (AutoWidth && !this.CurrentLayout.ColumnDefinitions.Any(c => c.SizeType == Layout.SizeType.Percentage))
            {
                nfloat min_left = this.CurrentLayout.Cells.Min(c => c.View.Frame.X);
                nfloat max_right = this.CurrentLayout.Cells.Max(c => c.View.Frame.Right);
                nfloat width = max_right - min_left;

                if (Frame.Width != width)
                {
                    Console.WriteLine($"{debugIndent}   SetWidth {width} (was {Frame.Width})");
                    this.SetWidth(width);
                }
            }

            if (AutoHeight && !this.CurrentLayout.RowDefinitions.Any(c => c.SizeType == Layout.SizeType.Percentage))
            {
                nfloat min_top = this.CurrentLayout.Cells.Min(c => c.View.Frame.Y);
                nfloat max_bottom = this.CurrentLayout.Cells.Max(c => c.View.Frame.Bottom);
                nfloat height = max_bottom - min_top;

                if (Frame.Height != height)
                {
                    Console.WriteLine($"{debugIndent}   SetHeight {height} (was {Frame.Height})");
                    this.SetHeight(height);
                }
            }

            firstLayout = false;
        }


        #endregion

    }
}
