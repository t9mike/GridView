using System;

using UIKit;
using GridView;
using CoreGraphics;

namespace Grid.Sample.iOS
{
	public partial class ViewController : UIViewController
	{
		protected ViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad(); 

			var red = new UIView { BackgroundColor = UIColor.Red };
			var blue = new UIView { BackgroundColor = UIColor.Blue, Frame = new CoreGraphics.CGRect(0,0,50,50) };
			var cyan = new UIView { BackgroundColor = UIColor.Cyan };
			var yellow = new UIView { BackgroundColor = UIColor.Yellow };

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
                AutoHeight = false
            };

            grid.AddLayout(portrait);
			//grid.AddLayout(landscape, (g) => (g.Frame.Width > g.Frame.Height));

			this.View = grid;
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}
	}
}
