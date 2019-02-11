using System;
using UIKit;
using CoreGraphics;
using GridView;

namespace GridViewSample.Samples
{
    public class SampleViewController5 : BaseViewController
    {
        public SampleViewController5()
        {
            var blue = Make_Box(UIColor.Blue, 50, 20);
            var yellow = Make_Box(UIColor.Yellow, 0, 50);
            var red = Make_Box(UIColor.Red, 0, 25);
            var green = Make_Box(UIColor.Green, 0, 100);
            var orange = Make_Box(UIColor.Orange, 25, 25);

            var layout = new Grid.Layout()
                .WithColumns(0.25f, 0.25f, 0.5f)
                .WithRows(-1, -1)
                + blue.At(0, 0).ColSpan(2).Tag("blue")
                + yellow.At(0, 2).AlignStretch(Grid.Layout.Direction.Horizontal).Tag("yellow")
                + red.At(1, 0).AlignStretch(Grid.Layout.Direction.Horizontal).Tag("red")
                + green.At(1, 1).AlignStretch(Grid.Layout.Direction.Horizontal).Tag("green")
                + orange.At(1, 2).Tag("orange")
            ;
            var grid = new Grid(layout);
            grid.AutoWidth = false; // Cannot be used w/ %-based column spec
            grid.AutoHeight = true;
            grid.Frame = new CGRect(0, 100, View.Frame.Width, 0); // No need to specify height: calculated/set
            grid.BackgroundColor = UIColor.DarkGray;
            grid.LayoutSubviews();

            Add(grid);
        }
    }
}
