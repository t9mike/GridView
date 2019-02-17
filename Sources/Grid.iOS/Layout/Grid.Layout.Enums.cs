using System;
using UIKit;

namespace GridView
{
    public partial class Grid : UIView
    {
        public partial class Layout
        {
            internal enum StackType
            {
                None,
                Vertical,
                Horizontal
            }

            public enum Alignment
            {
                Stretched,
                Start,
                Center,
                End,
            }

            public enum SizeType
            {
                Auto,
                Percentage,
                Fixed
            }

            [Flags]
            public enum Direction
            {
                Horizontal = 0,
                Vertical = 1,
                Both = Horizontal | Vertical
            }

            [Flags]
            public enum Collapse
            {
                None = 0,

                /// <summary>
                /// When a cell's view is Hidden, the view width will be ignored
                /// when calculating column width when column width is -1.
                /// </summary>
                Width = 1,

                /// <summary>
                /// When a cell's view is Hidden, the view height will be ignored
                /// when calculating row height when row height is -1.
                /// </summary>
                Height = 2,

                /// <summary>
                /// When a cell's view is Hidden, the both the view width and height
                /// will be ignored when calculating column width and/or height when 
                /// the column width or height is -1.
                /// </summary>
                Both = Width | Height
            }
        }
    }
}