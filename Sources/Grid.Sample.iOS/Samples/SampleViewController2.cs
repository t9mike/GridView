using System;
using UIKit;
using CoreGraphics;
using GridView;

namespace GridViewSample.Samples
{
    public class SampleViewController2 : BaseViewController
    {
        public SampleViewController2()
        {
            var time = Make_Box(UIColor.Blue, 200, 50);
            var p = Make_Box(UIColor.Yellow, 30, 15);
            var m = Make_Box(UIColor.Red, 30, 15);

            var layout = new Grid.Layout()
                .WithRows(0.5f, 0.5f)
                .WithColumns(-1, -1)
                + time.At(0, 0).RowSpan(2)
                + p.At(0, 1).Vertically(Grid.Layout.Alignment.Start)
                + m.At(1, 1).Vertically(Grid.Layout.Alignment.End);
            var grid = new Grid(layout);
            grid.Frame = new CGRect(0, 100, 0, 0);
            grid.BackgroundColor = UIColor.DarkGray;
            grid.LayoutSubviews();

            Add(grid);
        }
    }
}
