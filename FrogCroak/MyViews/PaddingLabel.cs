using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace FrogCroak.MyViews
{
    [Register("PaddingLabel")]
    public class PaddingLabel : UILabel
    {
        protected PaddingLabel(IntPtr handle) : base(handle)
        {
        }

        public override void DrawText(CGRect rect)
        {
            var insets = new UIEdgeInsets(15, 20, 15, 20);
            base.DrawText(UIEdgeInsetsInsetRect(rect, insets));
        }

        CGRect UIEdgeInsetsInsetRect(CGRect rect, UIEdgeInsets insets)
        {
            rect.X += insets.Left;
            rect.Y += insets.Top;
            rect.Width -= (insets.Left + insets.Right);
            rect.Height -= (insets.Top + insets.Bottom);

            return rect;
        }
    }
}
