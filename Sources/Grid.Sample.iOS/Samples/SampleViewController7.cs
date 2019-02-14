using System;
using UIKit;
using CoreGraphics;
using GridView;

namespace GridViewSample.Samples
{
    public class SampleViewController7 : BaseViewController
    {
        public SampleViewController7()
        {
            float width = 200;
            float padding = 10;

            var blue = Make_Box(UIColor.Blue, 0, 100);

            var layout = new Grid.Layout()
                + blue.AddStackRow(width - padding*2).Horizontally(Grid.Layout.Alignment.Stretched)
            ;
            layout.Padding = new Grid.Insets(10);
            var grid = new Grid(layout);
            grid.Frame = new CGRect(100, 100, width, 0);
            grid.BackgroundColor = UIColor.Gray;
            grid.AutoWidth = false;
            grid.AutoHeight = true;

            Add(grid);
        }
    }
}
