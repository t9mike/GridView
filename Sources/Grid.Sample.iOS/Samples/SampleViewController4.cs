using System;
using UIKit;
using CoreGraphics;
using GridView;

namespace GridViewSample.Samples
{
    public class SampleViewController4 : BaseViewController
    {
        public SampleViewController4()
        {
            var box1 = Make_Box(UIColor.Blue, 75, 20);
            var box2 = Make_Box(UIColor.Yellow, 50, 50);
            var box3 = Make_Box(UIColor.Red, 25, 50);
            var box4 = Make_Box(UIColor.Green, 25, 50);
            var box5 = Make_Box(UIColor.Orange, 25, 25);

            var layout = new Grid.Layout()
                .WithColumns(-1, -1, -1)
                .WithRows(-1, -1)
                + box1.At(0, 0).ColSpan(2)
                + box2.At(0, 2)
                + box3.At(1, 0)
                + box4.At(1, 1)
                + box5.At(1, 2)
            ;
            var grid = new Grid(layout);
            grid.Frame = new CGRect(0, 100, 0, 0);
            grid.BackgroundColor = UIColor.DarkGray;
            grid.LayoutSubviews();

            Add(grid);
        }
    }
}
