using UIKit;

namespace GridView
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public partial class Grid : UIView
	{   
		public partial class Layout
		{
            #region Margins

            /// <summary>
            /// This is global offset of all cells from the grid's outer frame.
            /// </summary>
            public Insets Padding { get; set; }

            /// <summary>
            /// This is the offset between each column and row. There is no
            /// offset between the first/last column/row: only between.
            /// </summary>
			public float Spacing { get; set; }

			#endregion

			#region Trigger

			public Func<Grid, bool> Trigger { get; set; }

			#endregion

			#region Definitions

			public List<Definition> ColumnDefinitions { get; private set; } = new List<Definition>();

			public List<Definition> RowDefinitions { get; private set; } = new List<Definition>();

			public Layout WithRows(params float[] rows)
			{
				this.RowDefinitions = rows.Select(size => new Definition(size)).ToList();
				return this;
			}

			public Layout WithColumns(params float[] columns)
			{
				this.ColumnDefinitions = columns.Select(size => new Definition(size)).ToList();
				return this;
			}

            #endregion

            #region Cells

            private List<Cell> cells = new List<Cell>();

			public IEnumerable<Cell> Cells => cells.ToArray();

			public Layout Add(Cell cell)
			{
				this.cells.Add(cell);
				return this;
			}

			public Position GetPosition(UIView cell) => this.cells.First(c => c.View == cell).Position;

			#endregion

			#region Absolute sizes

			public nfloat[] CalculateAbsoluteColumnWidth(nfloat totalWidth)
			{
				var absoluteColumnWidth = new nfloat[this.ColumnDefinitions.Count()];

                // Calculate full width of grid

                // Add width of fixed size columns
                var remaining = totalWidth - this.ColumnDefinitions.Where((d) => d.Size > 1).Select(d => d.Size).Sum();

                // Add width of auto sized columns
                for (int column = 0; column < this.ColumnDefinitions.Count(); column++)
                {
                    var definition = this.ColumnDefinitions.ElementAt(column);
                    if (definition.Size == -1)
                    {
                        var autoSizedCells = Cells.Where(c => c.Position.Column == column &&
                            c.Position.ColumnSpan == 1 && c.IncludeInAutoSizeCalcs);
                        if (autoSizedCells.Any())
                        {
                            absoluteColumnWidth[column] = autoSizedCells.Max(c => c.View.Frame.Width + c.Position.Margin.Width());
                            remaining -= absoluteColumnWidth[column];
                        }
                    }
                }

                // Add size of padding and spacing
				remaining -= this.Padding.Left + this.Padding.Right;
				remaining -= (this.ColumnDefinitions.Count() - 1) * this.Spacing;

				remaining = (nfloat)Math.Max(0, remaining);

				for (int column = 0; column < this.ColumnDefinitions.Count(); column++)
				{
					var definition = this.ColumnDefinitions.ElementAt(column);
                    if (definition.Size != -1)
                    {
                        absoluteColumnWidth[column] = definition.Size > 1 ? definition.Size : definition.Size * remaining;
                    }
                }

				return absoluteColumnWidth;
			}

			public nfloat[] CalculateAbsoluteRowHeight(nfloat totalHeight)
			{
				var absoluteRowHeight = new nfloat[this.RowDefinitions.Count()];

                // Calculate full height of grid

                // Add height of fixed size rows
                var remaining = totalHeight - this.RowDefinitions.Where((d) => d.Size > 1).Select(d => d.Size).Sum();

                // Add height of auto sized rows
                for (int row = 0; row < this.RowDefinitions.Count(); row++)
                {
                    var definition = this.RowDefinitions.ElementAt(row);
                    if (definition.Size == -1)
                    {
                        var autoSizedCells = Cells.Where(c => c.Position.Row == row && c.Position.RowSpan == 1 && c.IncludeInAutoSizeCalcs);
                        if (autoSizedCells.Any())
                        {
                            absoluteRowHeight[row] = autoSizedCells.Max(c => c.View.Frame.Height + c.Position.Margin.Height());
                            remaining -= absoluteRowHeight[row];
                        }
                    }
                }

                // Add size of padding and spacing
                remaining -= this.Padding.Top + this.Padding.Bottom;
				remaining -= (this.RowDefinitions.Count() - 1) * this.Spacing;

				remaining = (nfloat)Math.Max(0, remaining);

				for (int row = 0; row < this.RowDefinitions.Count(); row++)
				{
					var definition = this.RowDefinitions.ElementAt(row);
                    if (definition.Size != -1)
                    {
                        absoluteRowHeight[row] = definition.Size > 1 ? definition.Size : definition.Size * remaining;
                    }
                }

				return absoluteRowHeight;
			}

			#endregion

			#region Operator

			public static Layout operator +(Layout layout, Cell cell)
			{
                // "Stack" is supported through automatic creation of 
                // necessary grid rows/columns
                if (cell.Position.StackType == StackType.Horizontal)
                {
                    if (!layout.RowDefinitions.Any())
                    {
                        layout.RowDefinitions.Add(new Definition((float)cell.Position.StackCellSize.Height));
                    }
                    layout.ColumnDefinitions.Add(new Definition((float)cell.Position.StackCellSize.Width));
                    cell.Position = new Position(cell.Position) { Row = 0, Column = layout.ColumnDefinitions.Count - 1 };

                }
                if (cell.Position.StackType == StackType.Vertical)
                {
                    if (!layout.ColumnDefinitions.Any())
                    {
                        layout.ColumnDefinitions.Add(new Definition((float)cell.Position.StackCellSize.Width));
                    }
                    layout.RowDefinitions.Add(new Definition((float)cell.Position.StackCellSize.Height));
                    cell.Position = new Position(cell.Position) { Row = layout.RowDefinitions.Count - 1, Column = 0 };
                }

                layout.Add(cell);
				return layout;
			}

			#endregion
		}
	}
}
