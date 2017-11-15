using System;

using UIKit;

namespace FrogCroak.ViewControllers
{
    public partial class AudioViewController : UIViewController
    {
        public AudioViewController() : base("AudioViewController", null)
        {
        }

        protected AudioViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}

