using System;
using CoreGraphics;
using UIKit;

namespace GridViewSample.Samples
{
    public class BaseViewController : UIViewController
    {
        public BaseViewController()
        {
        }

        protected UIView Make_Box(UIColor bg_color)
        {
            return Make_Box(bg_color, 0, 0);
        }

        protected UIView Make_Box(UIColor bg_color, nfloat w, nfloat h)
        {
            var view = new UIView();
            view.Frame = new CGRect(0, 0, w, h);
            view.BackgroundColor = bg_color.ColorWithAlpha(0.5f);
            return view;
        }
    }
}
