using System;
using CoreGraphics;
using UIKit;

namespace GridView
{
	public static class Extensions
	{
		public static Grid.Layout.Cell At(this UIView view, int row, int column) => new Grid.Layout.Cell(view, new Grid.Layout.Position(row, column));

		public static Grid.Layout.Cell Span(this Grid.Layout.Cell cell, int rowspan, int columnspan) => new Grid.Layout.Cell(cell.View, new Grid.Layout.Position(cell.Position) { RowSpan = rowspan, ColumnSpan = columnspan });

        public static Grid.Layout.Cell ColSpan(this Grid.Layout.Cell cell, int columnspan) => new Grid.Layout.Cell(cell.View, new Grid.Layout.Position(cell.Position) { ColumnSpan = columnspan });

        public static Grid.Layout.Cell RowSpan(this Grid.Layout.Cell cell, int rowspan) => new Grid.Layout.Cell(cell.View, new Grid.Layout.Position(cell.Position) { RowSpan = rowspan });

        public static Grid.Layout.Cell Vertically(this Grid.Layout.Cell cell, Grid.Layout.Alignment alignment) => new Grid.Layout.Cell(cell.View, new Grid.Layout.Position(cell.Position) { Vertical = alignment });

		public static Grid.Layout.Cell Horizontally(this Grid.Layout.Cell cell, Grid.Layout.Alignment alignment) => new Grid.Layout.Cell(cell.View, new Grid.Layout.Position(cell.Position) { Horizontal = alignment });

        /// <summary>
        /// This is an alternative to using Vertically() and/or Horizontally() to set
        /// alignment. It sets one of both directions to Stretched.
        /// </summary>
        public static Grid.Layout.Cell AlignStretch(this Grid.Layout.Cell cell, 
            Grid.Layout.Direction direction = Grid.Layout.Direction.Both) => 
            Align(cell, Grid.Layout.Alignment.Stretched, direction);

        /// <summary>
        /// This is an alternative to using Vertically() and/or Horizontally() to set
        /// alignment. It sets one of both directions to Centered.
        /// </summary>
        public static Grid.Layout.Cell AlignCenter(this Grid.Layout.Cell cell,
            Grid.Layout.Direction direction = Grid.Layout.Direction.Both) =>
            Align(cell, Grid.Layout.Alignment.Center, direction);

        /// <summary>
        /// This is an alternative to using Vertically() and/or Horizontally() to set
        /// alignment. It sets one of both directions to Start.
        /// </summary>
        public static Grid.Layout.Cell AlignStart(this Grid.Layout.Cell cell,
            Grid.Layout.Direction direction = Grid.Layout.Direction.Both) =>
            Align(cell, Grid.Layout.Alignment.Start, direction);

        /// <summary>
        /// This is an alternative to using Vertically() and/or Horizontally() to set
        /// alignment. It sets one of both directions to Start.
        /// </summary>
        public static Grid.Layout.Cell AlignEnd(this Grid.Layout.Cell cell,
            Grid.Layout.Direction direction = Grid.Layout.Direction.Both) =>
            Align(cell, Grid.Layout.Alignment.End, direction);

        /// <summary>
        /// If a View is Hidden or has Alpha set to 0, whether to
        /// skip the view when auto sizing the width and/or height. 
        /// </summary>
        public static Grid.Layout.Cell CollapseHidden(this Grid.Layout.Cell cell, Grid.Layout.Collapse collapse) => new Grid.Layout.Cell(cell.View, new Grid.Layout.Position(cell.Position) { CollapseHidden = collapse });

        /// <summary>
        /// Defines a new column after last and sets the width.
        /// If width is not specified, it is auto sized. Also
        /// automatically creates a row in the underlying grid
        /// as needed (row 0 is always used).
        /// </summary>
        public static Grid.Layout.Cell AddStackColumn(this UIView view, float width = -1, float height = -1)
        {
            // Row,Column is updated via + operator (when added to layout)
            return new Grid.Layout.Cell(view, new Grid.Layout.Position(0, 0)
            { 
                StackType = Grid.Layout.StackType.Horizontal,
                StackCellSize = new CGSize(width, height)
            });
        }

        /// <summary>
        /// Defines a new row after last and sets the height.
        /// If height is not specified, it is auto sized. Also
        /// automatically creates a column in the underlying grid
        /// as needed (col 0 is always used).
        /// </summary>
        public static Grid.Layout.Cell AddStackRow(this UIView view, float width = -1, float height = -1)
        {
            // Row,Column is updated via + operator (when added to layout)
            return new Grid.Layout.Cell(view, new Grid.Layout.Position(0, 0)
            { 
                StackType = Grid.Layout.StackType.Vertical,
                StackCellSize = new CGSize(width, height)
            });
        }

        /// <summary>
        /// Provides a way to attach a .NET value to the grid cell. Useful for debugging.
        /// </summary>
        public static Grid.Layout.Cell Tag(this Grid.Layout.Cell cell, object tag) => new Grid.Layout.Cell(cell.View, new Grid.Layout.Position(cell.Position) { Tag = tag });

        /// <summary>
        /// When set, the cell's view will not be sized during layout.
        /// </summary>
        public static Grid.Layout.Cell NoResize(this Grid.Layout.Cell cell) => new Grid.Layout.Cell(cell.View, new Grid.Layout.Position(cell.Position) { NoResize = true });

        /// <summary>
        /// When set, the cell's view will not be positioned during layout nor sized. This is useful
        /// when the X,Y position of a sub-view is set in some other way. 
        /// </summary>
        public static Grid.Layout.Cell NoPosition(this Grid.Layout.Cell cell) => new Grid.Layout.Cell(cell.View, new Grid.Layout.Position(cell.Position) { NoPosition = true });

        public static nfloat Width(this UIEdgeInsets edge) => edge.Left + edge.Right;
        public static nfloat Height(this UIEdgeInsets edge) => edge.Top + edge.Bottom;

        /// <summary>
        /// Margin is included in size auto calculation (when a col or row spec is -1).
        /// </summary>
        public static Grid.Layout.Cell Margin(this Grid.Layout.Cell cell, UIEdgeInsets margin) => new Grid.Layout.Cell(cell.View, new Grid.Layout.Position(cell.Position) { Margin = margin });

        /// <summary>
        /// Margin is included in size auto calculation (when a col or row spec is -1).
        /// </summary>
        public static Grid.Layout.Cell Margin(this Grid.Layout.Cell cell, nfloat top, nfloat left, nfloat bottom, nfloat right) => new Grid.Layout.Cell(cell.View, new Grid.Layout.Position(cell.Position) { Margin = new UIEdgeInsets(top, left, bottom, right) });

        /// <summary>
        /// Margin is included in size auto calculation (when a col or row spec is -1).
        /// </summary>
        public static Grid.Layout.Cell Margin(this Grid.Layout.Cell cell, nfloat all) => new Grid.Layout.Cell(cell.View, new Grid.Layout.Position(cell.Position) { Margin = new UIEdgeInsets(all, all, all, all) });

        public static UIEdgeInsets SetTop(this UIEdgeInsets inset, nfloat top) =>
            new UIEdgeInsets(top, inset.Left, inset.Bottom, inset.Right);
        public static UIEdgeInsets SetLeft(this UIEdgeInsets inset, nfloat left) =>
            new UIEdgeInsets(inset.Top, left, inset.Bottom, inset.Right);
        public static UIEdgeInsets SetRight(this UIEdgeInsets inset, nfloat right) =>
            new UIEdgeInsets(inset.Top, inset.Left, inset.Bottom, right);
        public static UIEdgeInsets SetBottom(this UIEdgeInsets inset, nfloat bottom) =>
            new UIEdgeInsets(inset.Top, inset.Left, bottom, inset.Right);
        
        public static Grid.Insets SetTop(this Grid.Insets inset, float top) =>
            new Grid.Insets(inset.Left, top, inset.Right, inset.Bottom);
        public static Grid.Insets SetLeft(this Grid.Insets inset, float left) =>
            new Grid.Insets(left, inset.Top, inset.Right, inset.Bottom);
        public static Grid.Insets SetRight(this Grid.Insets inset, float right) =>
            new Grid.Insets(inset.Left, inset.Top, right, inset.Bottom);
        public static Grid.Insets SetBottom(this Grid.Insets inset, float bottom) =>
            new Grid.Insets(inset.Left, inset.Top, inset.Right, bottom);

        public static Grid.Layout.Cell MarginTop(this Grid.Layout.Cell cell,
            nfloat top) => new Grid.Layout.Cell(cell.View,
            new Grid.Layout.Position(cell.Position)
            {
                Margin = cell.Position.Margin.SetTop(top)
            });

        public static Grid.Layout.Cell MarginLeft(this Grid.Layout.Cell cell, 
            nfloat left) => new Grid.Layout.Cell(cell.View, 
            new Grid.Layout.Position(cell.Position) 
            { 
                Margin = cell.Position.Margin.SetLeft(left) 
            });

        public static Grid.Layout.Cell MarginRight(this Grid.Layout.Cell cell,
            nfloat right) => new Grid.Layout.Cell(cell.View,
            new Grid.Layout.Position(cell.Position)
            {
                Margin = cell.Position.Margin.SetRight(right)
            });

        public static Grid.Layout.Cell MarginBottom(this Grid.Layout.Cell cell,
            nfloat bottom) => new Grid.Layout.Cell(cell.View,
            new Grid.Layout.Position(cell.Position)
            {
                Margin = cell.Position.Margin.SetBottom(bottom)
            });

        /// <summary>
        /// Instead of setting size based on the view's Frame, use this.
        /// Only used when column and/or row size is -1.
        /// </summary>
        public static Grid.Layout.Cell UseFixedSize(this Grid.Layout.Cell cell,
            CGSize size) => new Grid.Layout.Cell(cell.View,
            new Grid.Layout.Position(cell.Position)
            {
                UseFixedSize = true,
                FixedSize = size

            });

        /// <summary>
        /// Instead of setting size based on the view's Frame, use this.
        /// Only used when column and/or row size is -1.
        /// </summary>
        public static Grid.Layout.Cell UseFixedSize(this Grid.Layout.Cell cell,
            nfloat w, nfloat h) => new Grid.Layout.Cell(cell.View,
            new Grid.Layout.Position(cell.Position)
            {
                UseFixedSize = true,
                FixedSize = new CGSize(w, h)

            });

        // Users may have this defined in other libraries: don't pollute
        internal static void SetWidth(this UIView view, nfloat width)
        {
            view.Frame = new CGRect(view.Frame.X, view.Frame.Y, width, view.Frame.Height);
        }

        // Users may have this defined in other libraries: don't pollute
        internal static void SetHeight(this UIView view, nfloat height)
        {
            view.Frame = new CGRect(view.Frame.X, view.Frame.Y, view.Frame.Width, height);
        }

        internal static Grid.Layout.Cell Align(Grid.Layout.Cell cell, Grid.Layout.Alignment alignment,
            Grid.Layout.Direction direction)
        {
            if (direction.HasFlag(Grid.Layout.Direction.Horizontal))
            {
                cell = cell.Horizontally(alignment);
            }
            if (direction.HasFlag(Grid.Layout.Direction.Vertical))
            {
                cell = cell.Vertically(alignment);
            }
            return cell;
        }

        /// <summary>
        /// Rounds X, Y to to 0.5 precision.
        /// </summary>
        internal static CGPoint Round(this CGPoint point)
        {
            return new CGPoint(RoundToScale(point.X), RoundToScale(point.Y));
        }

        /// <summary>
        /// Rounds X, Y to to 0.5 precision.
        /// </summary>
        internal static CGRect RoundLocation(this CGRect rect)
        {
            return new CGRect(RoundToScale(rect.X), RoundToScale(rect.Y), rect.Width, rect.Height);
        }

        internal static nfloat RoundToScale(this nfloat value)
        {
            return (nfloat)(Math.Round((double)value * UIScreen.MainScreen.Scale, MidpointRounding.AwayFromZero) / UIScreen.MainScreen.Scale);
        }
    }
}
