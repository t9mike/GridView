using System;
using UIKit;
using CoreGraphics;
using GridView;

namespace GridViewSample.Samples
{
    public class SampleViewController3 : BaseViewController
    {
        UIView box2;

        public SampleViewController3()
        {
            var box1 = Make_Box(UIColor.Blue, 200, 50);
            box2 = Make_Box(UIColor.Yellow, 100, 50);
            var box3 = Make_Box(UIColor.Red, 200, 50);
            var box4 = Make_Box(UIColor.Green, 200, 50);

            var layout = new Grid.Layout()
                + box1.AddStackRow()
                + box2.AddStackRow().MarginTop(60).MarginLeft(20).MarginBottom(40)
                + box3.AddStackRow()
                + box4.AddStackRow()
            ;
            layout.Spacing = 10;
            var grid = new Grid(layout);
            grid.Frame = new CGRect(0, 100, 0, 0);
            grid.BackgroundColor = UIColor.DarkGray;
            grid.LayoutSubviews();

            Add(grid);
        }


        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            box2.Hidden = true;
        }
    }
}
