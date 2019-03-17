using UIKit;
using CoreGraphics;

namespace GridView
{
	public partial class Grid : UIView
	{
		public partial class Layout
		{
            public class Position
            {
                public Position(Position other)
                {
                    this.Row = other.Row;
                    this.Column = other.Column;
                    this.RowSpan = other.RowSpan;
                    this.ColumnSpan = other.ColumnSpan;
                    this.Horizontal = other.Horizontal;
                    this.Vertical = other.Vertical;
                    this.NoResize = other.NoResize;
                    this.NoPosition = other.NoPosition;
                    this.Margin = other.Margin;
                    this.Tag = other.Tag;
                    this.StackType = other.StackType;
                    this.StackCellSize = other.StackCellSize;
                    this.CollapseHidden = other.CollapseHidden;
                }

                public Position(int row, int column)
                {
                    this.Row = row;
                    this.Column = column;
                    this.RowSpan = 1;
                    this.ColumnSpan = 1;
                    this.Horizontal = Alignment.Start;
                    this.Vertical = Alignment.Start;
                    this.NoResize = false;
                    this.NoPosition = false;
                    this.Margin = UIEdgeInsets.Zero;
                    this.Tag = null;
                    this.StackType = StackType.None;
                    this.StackCellSize = CGSize.Empty; // N/A unless StackType is not None
                    this.CollapseHidden = Collapse.Both;
                }

                public int Row { get; set; }

                public int Column { get; set; }

                public int RowSpan { get; set; }

                public int ColumnSpan { get; set; }

                public Alignment Vertical { get; set; }

                public Alignment Horizontal { get; set; }

                public bool NoResize { get; set; }

                public bool NoPosition { get; set; }

                public UIEdgeInsets Margin { get; set; }

                public object Tag { get; set; }

                public Collapse CollapseHidden { get; set; }

                // Temporary to support stack creation fluent interface
                internal StackType StackType;
                internal CGSize StackCellSize;

                public override string ToString()
                {
                    string col = Column.ToString();
                    if (ColumnSpan > 1)
                    {
                        col += "-" + (Column + ColumnSpan - 1).ToString();
                    }
                    string row = Row.ToString();
                    if (RowSpan > 1)
                    {
                        row += "-" + (Row + RowSpan - 1).ToString();
                    }
                    return $"Col={col} Row={row}";
                }
            }
        }
	}
}
