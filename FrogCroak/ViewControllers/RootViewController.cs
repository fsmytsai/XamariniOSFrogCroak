using System;
using Foundation;
using UIKit;

namespace FrogCroak.ViewControllers
{
    public partial class RootViewController : UIViewController
    {
        private UIViewController vc_Intro;
        private UIViewController tbc_Home;

        public RootViewController() : base("RootViewController", null)
        {
        }

        protected RootViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            var preferencesRead = NSUserDefaults.StandardUserDefaults;
            if (preferencesRead.BoolForKey("NeverShowIntro"))
            {
                tbc_Home = Storyboard.InstantiateViewController("tbc_Home");
                switchViewController(null, tbc_Home);
            }
            else
            {
                vc_Intro = Storyboard.InstantiateViewController("vc_Intro");
                switchViewController(null, vc_Intro);
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        public void switchViewController(UIViewController fromVC, UIViewController toVC)
        {
            if (fromVC != null)
            {
                fromVC.WillMoveToParentViewController(null); // 通知from即将从父ViewController移除
                fromVC.View.RemoveFromSuperview(); // 移除from的view
                fromVC.RemoveFromParentViewController(); // 移除from的ViewController
            }

            if (toVC != null)
            {
                this.AddChildViewController(toVC); // 添加to的ViewController到父ViewController
                var cgrect = new CoreGraphics.CGRect();
                cgrect.X = 0;
                cgrect.Y = 0;
                cgrect.Width = containerView.Frame.Width;
                cgrect.Height = containerView.Frame.Height;
                toVC.View.Frame = cgrect;
                this.containerView.AddSubview(toVC.View);
                toVC.DidMoveToParentViewController(this); // 通知to已经添加到父ViewController
            }
        }

        public void startCroak()
        {
            var preferencesWrite = NSUserDefaults.StandardUserDefaults;
            preferencesWrite.SetBool(true, "NeverShowIntro");
            tbc_Home = Storyboard.InstantiateViewController("tbc_Home");
            switchViewController(vc_Intro, tbc_Home);
        }
    }
}