﻿using System;
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
    }
}
