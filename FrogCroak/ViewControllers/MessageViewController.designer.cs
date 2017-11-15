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
	[Register ("MessageViewController")]
	partial class MessageViewController
	{
		[Outlet]
		UIKit.UITableView table_Message { get; set; }

		[Outlet]
		UIKit.UITextField tf_Message { get; set; }

		[Action ("pressSelectImage:")]
		partial void pressSelectImage (UIKit.UIButton sender);

		[Action ("pressSendMessage:")]
		partial void pressSendMessage (UIKit.UIButton sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (tf_Message != null) {
				tf_Message.Dispose ();
				tf_Message = null;
			}

			if (table_Message != null) {
				table_Message.Dispose ();
				table_Message = null;
			}
		}
	}
}
