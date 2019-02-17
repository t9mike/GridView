using UIKit;

namespace GridView
{
    using System;
    using CoreGraphics;

    public partial class Grid : UIView
	{
        public partial class Layout
		{
			public class Cell
			{
				public Cell(UIView view, Position position)
				{
                    this.View = view ?? throw new Exception($"view is null for cell at row {position.Row}, column {position.Column}");
					this.InitialSize = this.View.Bounds.Size;
					this.Position = position;
				}

				public CGSize InitialSize { get; private set; }

				public UIView View { get; private set; }

                public Position Position { get; internal set; }

                /// <summary>
                /// True when the cell's height will be inspected to determine row height if 
                /// size spec is -1.
                /// </summary>
                public bool IncludeInAutoHeightSizeCalcs => View.Hidden == false || !Position.CollapseHidden.HasFlag(Collapse.Height);

                /// <summary>
                /// True when the cell's width will be inspected to determine column width if 
                /// size spec is -1.
                /// </summary>
                public bool IncludeInAutoWidthSizeCalcs => View.Hidden == false || !Position.CollapseHidden.HasFlag(Collapse.Width);

                public override string ToString()
                {
                    return $"Position={Position}: View.Frame={View?.Frame} View.Hidden={View?.Hidden}";
                }
            }
		}
	}
}
