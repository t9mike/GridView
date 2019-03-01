﻿using UIKit;

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
                /// <summary>
                /// 
                /// </summary>
                /// <param name="view">OK for view to be null: it will be skipped in layout. This 
                /// provides a way to easily skip a row/column in certain situations without
                /// constructing the underlying UIView or have a separate layout.</param>
                /// <param name="position">Position.</param>
				public Cell(UIView view, Position position)
				{
                    this.View = view;
					this.InitialSize = this.View == null ? CGSize.Empty : this.View.Bounds.Size;
					this.Position = position;
				}

				public CGSize InitialSize { get; private set; }

				public UIView View { get; private set; }

                public Position Position { get; internal set; }

                /// <summary>
                /// True when the cell's height will be inspected to determine row height if 
                /// size spec is -1.
                /// </summary>
                public bool IncludeInAutoHeightSizeCalcs => 
                    (View?.Hidden == false && View?.Alpha > 0) || !Position.CollapseHidden.HasFlag(Collapse.Height);

                /// <summary>
                /// True when the cell's width will be inspected to determine column width if 
                /// size spec is -1.
                /// </summary>
                public bool IncludeInAutoWidthSizeCalcs => 
                    (View?.Hidden == false && View?.Alpha > 0) || !Position.CollapseHidden.HasFlag(Collapse.Width);

                public override string ToString()
                {
                    return $"Position={Position}: View.Frame={View?.Frame} View.Hidden={View?.Hidden}";
                }
            }
		}
	}
}
