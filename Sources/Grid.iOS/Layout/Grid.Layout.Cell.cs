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
                /// True when the cell will be inspected to determine column/row with if 
                /// size spec is -1 or when AutoWidth or AutoHeight is true.
                /// </summary>
                public bool IncludeInAutoSizeCalcs => View.Hidden == false || Position.CollapseHidden == false;

                public override string ToString()
                {
                    return $"Position={Position}: View.Frame={View?.Frame} View.Hidden={View?.Hidden}";
                }
            }
		}
	}
}
