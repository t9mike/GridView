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
            /// Finds a cell by col, row index. Retuns null if there is none.
            /// </summary>
            public Cell FindCell(int col, int row)
            {
                return this.Cells.FirstOrDefault(c => c.Position.Column == col && c.Position.Row == row);
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
				var absoluteColumnWidth = new nfloat[this.ColumnDefinitions.Count()];

                // Calculate full height of grid
                nfloat totalWidth;

                if (grid.AutoWidth)
                {
                    LogLine($"CalculateAbsoluteRowHeight AutoWidth");

                    nfloat maxWidth = 0;
                    bool canAutoSize = false;

                    for (int row = 0; row < this.RowDefinitions.Count(); row++)
                    {
                        // Tracks the # of cols -- adding spans -- that have counted to the
                        // width calc for this row. If there were 3 cols defined for the
                        // grid and (0, 1) has ColSpan=2 and is auto sized and (0, 2) 
                        // has absolute size spec, this value will be 3 
                        int numGridColsCounted = 0;

                        // In the example above, this value will be 1: (0,1) and (0,2) = 2.  
                        int numCellCols = 0;

                        nfloat widthForRow = 0;
                        for (int col = 0; col < this.ColumnDefinitions.Count(); col++)
                        {
                            var colDef = this.ColumnDefinitions[col];
                            var cell = FindCell(col, row);
                            if (cell?.IncludeInAutoSizeCalcs == true)
                            {
                                nfloat cellWidth = colDef.SizeType == SizeType.Fixed
                                    ? colDef.Size
                                    : cell.View.Frame.Width;
                                LogLine($"{   cell.Position} cellWidth={cellWidth}");
                                cellWidth += cell.Position.Margin.Width();
                                widthForRow += cellWidth;
                                numGridColsCounted += cell.Position.ColumnSpan;
                                numCellCols += 1;
                            }
                        }

                        if (numGridColsCounted >= this.ColumnDefinitions.Count())
                        {
                            canAutoSize = true;
                            widthForRow += (numCellCols - 1) * Spacing;
                            LogLine($"   row={row}: widthForrow={widthForRow}");
                            maxWidth = NMath.Max(maxWidth, widthForRow);
                        }
                    }

                    if (!canAutoSize)
                    {
                        // This should not be possible given algo above
                        throw new Exception("your grid's col and cell defintions do not support AutoWidth");
                    }
                    totalWidth = maxWidth;
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

                return new Tuple<nfloat, nfloat[]>(totalWidth, absoluteColumnWidth);
			}

            /// <summary>
            /// Calculates grid height and individual row heights.
            /// </summary>
			public Tuple<nfloat,nfloat[]> CalculateAbsoluteRowHeight(Grid grid)
			{
				var absoluteRowHeight = new nfloat[this.RowDefinitions.Count()];

                // Calculate full height of grid
                nfloat totalHeight;

                if (grid.AutoHeight)
                {
                    LogLine($"CalculateAbsoluteRowHeight AutoHeight");
                    nfloat maxHeight = 0;

                    for (int col = 0; col < this.ColumnDefinitions.Count(); col++)
                    {
                        // Tracks the # of rows -- adding spans -- that have counted to the
                        // height calc for this colun. If there were 3 rows defined for the
                        // grid and (0, 1) has rowSpan=2 and is auto sized and (0, 2) 
                        // has absolute size spec, this value will be 3 
                        int numGridRowsCounted = 0;       

                        // In the example above, this value will be 1: (0,1) and (0,2) = 2.  
                        int numCellRows = 0;

                        nfloat heightForCol = 0;
                        for (int row = 0; row < this.RowDefinitions.Count(); row++)
                        {
                            var rowDef = this.RowDefinitions[row];
                            var cell = FindCell(col, row);
                            if (cell?.IncludeInAutoSizeCalcs == true)
                            {
                                nfloat cellHeight = rowDef.SizeType == SizeType.Fixed
                                    ? rowDef.Size
                                    : cell.View.Frame.Height;
                                cellHeight += cell.Position.Margin.Height();
                                LogLine($"   {cell.Position} cellHeight={cellHeight}");
                                heightForCol += cellHeight;
                                numGridRowsCounted += cell.Position.RowSpan;
                                numCellRows += 1;
                            }
                        }

                        heightForCol += (numCellRows - 1) * Spacing;
                        LogLine($"   col={col}: heightForCol={heightForCol}");
                        maxHeight = NMath.Max(maxHeight, heightForCol);
                    }

                    totalHeight = maxHeight;
                }
                else
                {
                    totalHeight = grid.Frame.Height;
                }

                // Track available row height available for percent-based sized roiws
                // Add height of fixed size rows
                nfloat fixedHeight = this.RowDefinitions.Where((d) => d.Size > 1).Select(d => d.Size).Sum();
                nfloat remaining = totalHeight - fixedHeight;

                // Define height of auto sized rows
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

                // Define size of fixed and pct sized rows
				for (int row = 0; row < this.RowDefinitions.Count(); row++)
				{
					var definition = this.RowDefinitions.ElementAt(row);
                    if (definition.Size != -1)
                    {
                        absoluteRowHeight[row] = definition.Size > 1 ? definition.Size : definition.Size * remaining;
                    }
                }

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
