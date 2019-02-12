using System;
using UIKit;
using CoreGraphics;
using GridView;

namespace GridViewSample.Samples
{
    public class SampleViewController6 : BaseViewController
    {
        public SampleViewController6()
        {
            var blue = Make_Box(UIColor.Blue, 200, 100);
            var yellow = Make_Box(UIColor.Yellow, 100, 50);

            var layout = new Grid.Layout()
                + blue.AddStackRow()
                + yellow.AddStackRow().Horizontally(Grid.Layout.Alignment.Center)
            ;
            layout.Padding = new Grid.Insets(10, 25, 5, 50);
            var innerGrid = new Grid(layout);
            innerGrid.BackgroundColor = UIColor.LightGray;

            var frame = new CGRect(0, 200, View.Frame.Width, View.Frame.Width);
            var outerGrid = Grid.CreateAutoCenterGrid(innerGrid, frame);
            outerGrid.BackgroundColor = UIColor.DarkGray;

            Add(outerGrid);
        }
    }
}
