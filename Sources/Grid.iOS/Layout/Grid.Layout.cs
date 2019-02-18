using UIKit;
using CoreGraphics;
using HomeKit;

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

            /// <summary>
            /// Finds a cell that holds the specified UIView. Retuns null if there is none.
            /// </summary>
            public Cell FindCell(UIView view)
            {
                return this.Cells.FirstOrDefault(c => c.View == view);
            }

            /// <summary>
            /// Finds a cell by col, row index. Retuns null if there is none.
            /// </summary>
            public Cell FindCell(int col, int row)
            {
                return this.Cells.FirstOrDefault(c => c.Position.Column == col && c.Position.Row == row);
            }

            /// <summary>
            /// Finds a cell by col, row index, but will allow col to be anywhere 
            /// within the colspan for the cell. Retuns null if there is none.
            /// </summary>
            public Cell FindCellWithColspan(int col, int row)
            {
                return this.Cells.FirstOrDefault(c =>
                    (col >= c.Position.Column && col < c.Position.Column + c.Position.ColumnSpan) &&
                    (c.Position.Row == row)
                    );
            }

            /// <summary>
            /// Finds a cell by col, row index, but will allow row to be anywhere 
            /// within the rowspan for the cell. Retuns null if there is none.
            /// </summary>
            public Cell FindCellWithRowspan(int col, int row)
            {
                return this.Cells.FirstOrDefault(c =>
                    (c.Position.Column == col) &&
                    (row >= c.Position.Row && row < c.Position.Row + c.Position.RowSpan)
                    );
            }

            public Layout Add(Cell cell)
			{
				this.cells.Add(cell);
				return this;
			}

			public Position GetPosition(UIView cell) => this.cells.First(c => c.View == cell).Position;

            #endregion

            #region Absolute sizes

            /// <summary>
            /// Calculates grid width and individual column widths.
            /// </summary>
            public Tuple<nfloat, nfloat[]> CalculateAbsoluteColumnWidth(Grid grid)
			{
                bool debug = grid.GetType().ToString().Contains("LunarTimesGridView");
                var absoluteColumnWidth = new nfloat[this.ColumnDefinitions.Count()];

                // Calculate full height of grid
                nfloat totalWidth;

                var minColWidth = new nfloat[this.ColumnDefinitions.Count()];

                if (grid.AutoWidth)
                {
                    LogLine($"CalculateAbsoluteColumnWidth AutoWidth");

                    var maxColWidth = new nfloat[this.ColumnDefinitions.Count()]; 
                    int maxColSpan = grid.CurrentLayout.cells.Max(c => c.Position.ColumnSpan);

                    for (int colSpan = 1; colSpan <= maxColSpan; colSpan++)
                    {
                        for (int row = 0; row < this.RowDefinitions.Count(); row++)
                        {
                            for (int col = 0; col < this.ColumnDefinitions.Count(); col++)
                            {
                                var colDef = this.ColumnDefinitions[col];
                                var cell = FindCell(col, row);
                                if (cell == null) continue;
                                if (cell.IncludeInAutoWidthSizeCalcs && cell.Position.ColumnSpan == colSpan)
                                {
                                    nfloat cellWidth = colDef.SizeType == SizeType.Fixed
                                        ? colDef.Size
                                        : cell.View.Frame.Width;
                                    cellWidth += cell.Position.Margin.Width();
                                    LogLine($"   {cell.Position} cellWidth={cellWidth}");

                                    if (colSpan == 1)
                                    {
                                        maxColWidth[col] = NMath.Max(maxColWidth[col], cellWidth);
                                    }
                                    else
                                    {
                                        // Need to ensure the other columns we span have enough
                                        // width to account for this spanned column. 
                                        nfloat spanWidth = 0;
                                        for (int col2 = cell.Position.Column; col2 < cell.Position.Column + colSpan; col2++)
                                        {
                                            spanWidth += maxColWidth[col2];
                                        }
                                        if (cellWidth > spanWidth)
                                        {
                                            bool hasZeroColWidth = false;
                                            for (int col2 = cell.Position.Column; col2 < cell.Position.Column + colSpan; col2++)
                                            {
                                                if (maxColWidth[col2] == 0)
                                                {
                                                    hasZeroColWidth = true;
                                                }
                                            }
                                            if (hasZeroColWidth)
                                            {
                                                // Cop out: just size equally
                                                for (int col2 = cell.Position.Column; col2 < cell.Position.Column + colSpan; col2++)
                                                {
                                                    minColWidth[col2] = maxColWidth[col2] = cellWidth / colSpan;
                                                }
                                            }
                                            else
                                            {
                                                // Increase size of columns proportionatly
                                                nfloat colWidthUpsizeRation = cellWidth / spanWidth;
                                                for (int col2 = cell.Position.Column; col2 < cell.Position.Column + colSpan; col2++)
                                                {
                                                    maxColWidth[col2] *= colWidthUpsizeRation;
                                                    minColWidth[col2] = maxColWidth[col2];
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    LogLine("minColWidth:");
                    for (int col = 0; col < this.ColumnDefinitions.Count(); col++)
                    {
                        LogLine($"   col {col}: {minColWidth[col]}");
                    }

                    totalWidth = maxColWidth.Sum(w => (float)w);
                    int numCols = maxColWidth.Count(w => w > 0);
                    totalWidth += (numCols - 1) * Spacing + Padding.Left + Padding.Right;
                }
                else
                {
                    totalWidth = grid.Frame.Width;
                }

                // Track available col width available for percent-based sized roiws
                // Add width of fixed size cols
                nfloat fixedWidth = this.ColumnDefinitions.Where((d) => d.Size > 1).Select(d => d.Size).Sum();
                nfloat remaining = totalWidth - fixedWidth;

                // Add width of auto sized columns
                for (int column = 0; column < this.ColumnDefinitions.Count(); column++)
                {
                    var definition = this.ColumnDefinitions.ElementAt(column);
                    if (definition.Size == -1)
                    {
                        var autoSizedCells = Cells.Where(c => c.Position.Column == column &&
                            c.Position.ColumnSpan == 1 && c.IncludeInAutoWidthSizeCalcs);
                        if (autoSizedCells.Any())
                        {
                            nfloat colWidth = autoSizedCells.Max(c => c.View.Frame.Width + c.Position.Margin.Width());
                            colWidth = NMath.Max(minColWidth[column], colWidth);
                            absoluteColumnWidth[column] = colWidth;
                            remaining -= colWidth;
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
                        nfloat colWidth = definition.Size > 1 ? definition.Size : definition.Size * remaining;
                        absoluteColumnWidth[column] = NMath.Max(minColWidth[column], colWidth);
                    }
                }

                LogLine($"totalWidth={totalWidth}");

                 return new Tuple<nfloat, nfloat[]>(totalWidth, absoluteColumnWidth);
			}

            /// <summary>
            /// Calculates grid height and individual row heights.
            /// </summary>
			public Tuple<nfloat,nfloat[]> CalculateAbsoluteRowHeight(Grid grid)
			{
                bool debug = grid.GetType().ToString().Contains("LunarTimesGridView");
                var absoluteRowHeight = new nfloat[this.RowDefinitions.Count()];

                // Calculate full height of grid
                nfloat totalHeight;

                var minRowHeight = new nfloat[this.RowDefinitions.Count()];

                if (grid.AutoHeight)
                {
                    LogLine($"CalculateAbsoluteRowHeight AutoHeight");

                    var maxRowHeight = new nfloat[this.RowDefinitions.Count()];

                    int maxRowSpan = grid.CurrentLayout.cells.Max(c => c.Position.RowSpan);

                    for (int rowSpan = 1; rowSpan <= maxRowSpan; rowSpan++)
                    {
                        for (int col = 0; col < this.ColumnDefinitions.Count(); col++)
                        {
                            for (int row = 0; row < this.RowDefinitions.Count(); row++)
                            {
                                var rowDef = this.RowDefinitions[row];
                                var cell = FindCell(col, row);
                                if (cell == null) continue;
                                if (cell.IncludeInAutoHeightSizeCalcs && cell.Position.RowSpan == rowSpan)
                                {
                                    nfloat cellHeight = rowDef.SizeType == SizeType.Fixed
                                        ? rowDef.Size
                                        : cell.View.Frame.Height;
                                    cellHeight += cell.Position.Margin.Height();
                                    LogLine($"   {cell.Position} cellHeight={cellHeight}");

                                    if (rowSpan == 1)
                                    {
                                        maxRowHeight[row] = NMath.Max(maxRowHeight[row], cellHeight);
                                    }
                                    else
                                    {
                                        // Need to ensure the other rows we span have enough
                                        // Height to account for this spanned column. 
                                        nfloat spanHeight = 0;
                                        for (int row2 = cell.Position.Row; row2 < cell.Position.Row + rowSpan; row2++)
                                        {
                                            spanHeight += maxRowHeight[row2];
                                        }
                                        if (cellHeight > spanHeight)
                                        {
                                            bool hasZeroRowHeight = false;
                                            for (int row2 = cell.Position.Row; row2 < cell.Position.Row + rowSpan; row2++)
                                            {
                                                if (maxRowHeight[row2] == 0)
                                                {
                                                    hasZeroRowHeight = true;
                                                }
                                            }
                                            if (hasZeroRowHeight)
                                            {
                                                // Cop out: just size equally
                                                for (int row2 = cell.Position.Row; row2 < cell.Position.Row + rowSpan; row2++)
                                                {
                                                    minRowHeight[row2] = maxRowHeight[row2] = cellHeight / rowSpan;
                                                }
                                            }
                                            else
                                            {
                                                // Increase size of columns proportionatly
                                                nfloat colHeightUpsizeRation = cellHeight / spanHeight;
                                                for (int row2 = cell.Position.Row; row2 < cell.Position.Row + rowSpan; row2++)
                                                {
                                                    maxRowHeight[row2] *= colHeightUpsizeRation;
                                                    minRowHeight[row2] = maxRowHeight[row2];
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    LogLine("minRowHeight:");
                    for (int row = 0; row < this.RowDefinitions.Count(); row++)
                    {
                        LogLine($"   row {row}: {minRowHeight[row]}");
                    }

                    totalHeight = maxRowHeight.Sum(h => (float)h);
                    int numRows = maxRowHeight.Count(h => h > 0);
                    totalHeight += (numRows - 1) * Spacing + Padding.Top + Padding.Bottom;
                }
                else
                {
                    totalHeight = grid.Frame.Height;
                }

                // Track available row Height available for percent-based sized roiws
                // Add Height of fixed size cols
                nfloat fixedHeight = this.RowDefinitions.Where((d) => d.Size > 1).Select(d => d.Size).Sum();
                nfloat remaining = totalHeight - fixedHeight;

                // Add Height of auto sized columns
                for (int row = 0; row < this.RowDefinitions.Count(); row++)
                {
                    var definition = this.RowDefinitions.ElementAt(row);
                    if (definition.Size == -1)
                    {
                        var autoSizedCells = Cells.Where(c => c.Position.Row == row &&
                            c.Position.RowSpan == 1 && c.IncludeInAutoHeightSizeCalcs);
                        if (autoSizedCells.Any())
                        {
                            nfloat rowHeight = autoSizedCells.Max(c => c.View.Frame.Height + c.Position.Margin.Height());
                            rowHeight = NMath.Max(minRowHeight[row], rowHeight);
                            absoluteRowHeight[row] = rowHeight;
                            remaining -= rowHeight;
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
                        nfloat rowHeight = definition.Size > 1 ? definition.Size : definition.Size * remaining;
                        absoluteRowHeight[row] = NMath.Max(minRowHeight[row], rowHeight);
                    }
                }

                LogLine($"totalHeight={totalHeight}");

                return new Tuple<nfloat, nfloat[]>(totalHeight, absoluteRowHeight);
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
