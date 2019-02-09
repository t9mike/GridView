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
        }
    }
}