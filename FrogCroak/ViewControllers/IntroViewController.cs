using System;
using FrogCroak.MyDelegates;
using UIKit;
using CoreGraphics;

namespace FrogCroak.ViewControllers
{
    public partial class IntroViewController : UIViewController
    {
        public IntroViewController() : base("IntroViewController", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
            sv_Intro.Delegate = new IntroScrollViewDelegate(pc_Intro);
            sv_Intro.ContentSize = new CGSize(View.Frame.Width * 4, View.Frame.Height);
            for (int i = 0; i < 4; i++)
            {
                UIView OneView = new UIView(new CGRect(i * View.Frame.Width, 0, View.Frame.Width, View.Frame.Height));
                UIImageView ImageView = new UIImageView(new CGRect(0, 0, View.Frame.Width, View.Frame.Height));
                ImageView.Image = UIImage.FromBundle($"Page{i + 1}");
                OneView.AddSubview(ImageView);

                if (i == 3)
                {
                    UIButton StartButton = new UIButton(UIButtonType.RoundedRect);


                    StartButton.SetTitle("開始青蛙呱呱", UIControlState.Normal);

                    StartButton.TitleLabel.Font = UIFont.SystemFontOfSize(24);


                    StartButton.TintColor = UIColor.White;


                    StartButton.BackgroundColor = UIColor.FromRGB(0.2314f, 0.3176f, 0.0784f);


                    StartButton.Layer.CornerRadius = 20;


                    StartButton.TouchUpInside += (o, s) =>
                    {
                        Console.WriteLine("button touched");
                    };
                    var Frame = StartButton.Frame;
                    Frame.Width = View.Frame.Width / 2;
                    Frame.Height = 80;
                    StartButton.Frame = Frame;
                    OneView.AddSubview(StartButton);

                    StartButton.Center = View.ConvertPointFromView(View.Center, View.Superview);
                }

                sv_Intro.AddSubview(OneView);
            }
            sv_Intro.PagingEnabled = true;
            sv_Intro.Bounces = false;
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        partial void pcIntrolValueChange(UIPageControl sender)
        {
            var offset = new CoreGraphics.CGPoint(View.Frame.Width * sender.CurrentPage, 0);
            sv_Intro.SetContentOffset(offset, true);
        }
    }
}

