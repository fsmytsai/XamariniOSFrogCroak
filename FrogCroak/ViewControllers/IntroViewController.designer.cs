// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;
using System;

namespace FrogCroak.ViewControllers
{
	[Register ("IntroViewController")]
	partial class IntroViewController
    {
        public IntroViewController(IntPtr p) : base(p)
        {
        }
		[Outlet]
		UIKit.UIPageControl pc_Intro { get; set; }

		[Outlet]
		UIKit.UIScrollView sv_Intro { get; set; }

		[Action ("pcIntrolValueChange:")]
		partial void pcIntrolValueChange (UIKit.UIPageControl sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (pc_Intro != null) {
				pc_Intro.Dispose ();
				pc_Intro = null;
			}

			if (sv_Intro != null) {
				sv_Intro.Dispose ();
				sv_Intro = null;
			}
		}
	}
}
