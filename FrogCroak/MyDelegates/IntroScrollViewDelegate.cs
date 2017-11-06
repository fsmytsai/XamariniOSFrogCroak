using System;
using UIKit;

namespace FrogCroak.MyDelegates
{
    public class IntroScrollViewDelegate : UIScrollViewDelegate
    {
        private UIPageControl pc_Intro;
        public IntroScrollViewDelegate(UIPageControl pc_Intro)
        {
            this.pc_Intro = pc_Intro;
        }

        public override void DecelerationEnded(UIScrollView scrollView)
        {
            //base.DecelerationEnded(scrollView);
            int CurrentPageNum = (int)(Math.Round(scrollView.ContentOffset.X / scrollView.Frame.Width));
            pc_Intro.CurrentPage = CurrentPageNum;
        }
    }
}
