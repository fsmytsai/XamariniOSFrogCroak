// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace FrogCroak.ViewControllers
{
	[Register ("AudioViewController")]
	partial class AudioViewController
	{
		[Outlet]
		UIKit.UIImageView iv_ResultFrogImage { get; set; }

		[Outlet]
		UIKit.UILabel l_Result { get; set; }

		[Action ("Record:")]
		partial void Record (UIKit.UIButton sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (iv_ResultFrogImage != null) {
				iv_ResultFrogImage.Dispose ();
				iv_ResultFrogImage = null;
			}

			if (l_Result != null) {
				l_Result.Dispose ();
				l_Result = null;
			}
		}
	}
}
