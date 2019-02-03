using System;
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

        public static Grid.Layout.Cell NoResize(this Grid.Layout.Cell cell) => new Grid.Layout.Cell(cell.View, new Grid.Layout.Position(cell.Position) { NoResize = true });
    }
}
