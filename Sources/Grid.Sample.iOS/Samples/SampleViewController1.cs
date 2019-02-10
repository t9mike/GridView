using System;

using UIKit;
using GridView;
using CoreGraphics;

namespace GridViewSample.Samples
{
	public partial class SampleViewController1 : BaseViewController
	{
		public SampleViewController1()
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad(); 

			var red = Make_Box(UIColor.Red);
			var blue = Make_Box(UIColor.Blue, 50,50);
			var cyan = Make_Box(UIColor.Cyan);
			var yellow = Make_Box(UIColor.Yellow);

			var portrait = new GridView.Grid.Layout() 
									 { 
										Spacing = 10, 
										Padding = new GridView.Grid.Insets(10,32,10,10) 
									 }
									 .WithRows(0.75f, 0.25f, 200f)
									 .WithColumns(0.75f, 0.25f)
									 + red.At(0, 0).Span(2, 1).AlignStretch()
				  			         + blue.At(2, 0).Span(1, 2).AlignStretch()
                                     + cyan.At(0, 1).AlignStretch()
                                     + yellow.At(1,1).AlignStretch();

			var landscape = new GridView.Grid.Layout() 
									 { 
										Spacing = 20, 
										Padding = new GridView.Grid.Insets(20), 
									 }
									 .WithRows(1.00f)
									 .WithColumns(0.50f, 0.25f, 0.25f)
									 + red.At(0, 0).AlignStretch()
                                     + blue.At(0, 1).Vertically(GridView.Grid.Layout.Alignment.End).Horizontally(GridView.Grid.Layout.Alignment.Center)
									 + cyan.At(0, 2).AlignStretch();

            var grid = new GridView.Grid()
            {
                AutoWidth = false,
                AutoHeight = false,
                BackgroundColor = UIColor.White
            };

            grid.AddLayout(portrait);
			grid.AddLayout(landscape, (g) => (g.Frame.Width > g.Frame.Height));

			this.View = grid;
		}
	}
}
