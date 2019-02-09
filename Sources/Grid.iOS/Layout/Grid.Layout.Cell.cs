using UIKit;

namespace GridView
{
    using CoreGraphics;

    public partial class Grid : UIView
	{
        public partial class Layout
		{
			public class Cell
			{
				public Cell(UIView view, Position position)
				{
					this.View = view;
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
            }
		}
	}
}
