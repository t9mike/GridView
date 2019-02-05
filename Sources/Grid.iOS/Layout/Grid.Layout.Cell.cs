using UIKit;
using System;

namespace GridView
{
	using CoreGraphics;

	public partial class Grid : UIView
	{
        internal enum StackType
        {
            None,
            Vertical,
            Horizontal
        }

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
            }
		}
	}
}
