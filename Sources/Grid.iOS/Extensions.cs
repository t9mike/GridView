using System;
using CoreGraphics;
using UIKit;

namespace GridView
{
	public static class Extensions
	{
		public static Grid.Layout.Cell At(this UIView view, int row, int column) => new Grid.Layout.Cell(view, new Grid.Layout.Position(row, column));

		public static Grid.Layout.Cell Span(this Grid.Layout.Cell cell, int rowspan, int columnspan) => new Grid.Layout.Cell(cell.View, new Grid.Layout.Position(cell.Position) { RowSpan = rowspan, ColumnSpan = columnspan });

        public static Grid.Layout.Cell ColSpan(this Grid.Layout.Cell cell, int columnspan) => new Grid.Layout.Cell(cell.View, new Grid.Layout.Position(cell.Position) { RowSpan = 1, ColumnSpan = columnspan });

        public static Grid.Layout.Cell RowSpan(this Grid.Layout.Cell cell, int rowspan) => new Grid.Layout.Cell(cell.View, new Grid.Layout.Position(cell.Position) { RowSpan = rowspan, ColumnSpan = 1 });

        public static Grid.Layout.Cell Vertically(this Grid.Layout.Cell cell, Grid.Layout.Alignment alignment) => new Grid.Layout.Cell(cell.View, new Grid.Layout.Position(cell.Position) { Vertical = alignment });

		public static Grid.Layout.Cell Horizontally(this Grid.Layout.Cell cell, Grid.Layout.Alignment alignment) => new Grid.Layout.Cell(cell.View, new Grid.Layout.Position(cell.Position) { Horizontal = alignment });

        public static Grid.Layout.Cell CollapseHidden(this Grid.Layout.Cell cell, bool collapseHidden) => new Grid.Layout.Cell(cell.View, new Grid.Layout.Position(cell.Position) { CollapseHidden = collapseHidden });

        /// <summary>
        /// Defines a new column after last and sets the width.
        /// If width is not specified, it is auto sized. Also
        /// automatically creates a row in the underlying grid
        /// as needed (row 0 is always used).
        /// </summary>
        public static Grid.Layout.Cell AddStackColumn(this UIView view, float width = -1)
        {
            // Row,Column is updated via + operator (when added to layout)
            return new Grid.Layout.Cell(view, new Grid.Layout.Position(0, 0)
            { 
                StackType = Grid.StackType.Horizontal,
                StackCellSize = width
            });
        }

        /// <summary>
        /// Defines a new row after last and sets the height.
        /// If height is not specified, it is auto sized. Also
        /// automatically creates a column in the underlying grid
        /// as needed (col 0 is always used).
        /// </summary>
        public static Grid.Layout.Cell AddStackRow(this UIView view, float height = -1)
        {
            // Row,Column is updated via + operator (when added to layout)
            return new Grid.Layout.Cell(view, new Grid.Layout.Position(0, 0)
            { 
                StackType = Grid.StackType.Vertical,
                StackCellSize = height
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
                Margin = cell.Position.Margin.SetTop(bottom)
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
    }
}
