﻿using UIKit;

namespace GridView
{
	public partial class Grid : UIView
	{
		public partial class Layout
		{
            public struct Definition
			{
				public Definition(float size)
				{
					this.Size = size;
				}

				public float Size { get; private set; }

                public SizeType SizeType => Size == -1 ? SizeType.Auto : Size > 1 ? SizeType.Fixed : SizeType.Percentage;
            }
		}
	}
}
