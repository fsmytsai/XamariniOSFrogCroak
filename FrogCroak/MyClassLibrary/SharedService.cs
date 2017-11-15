using System;
using System.IO;
using System.Net;
using UIKit;

namespace FrogCroak.MyClassLibrary
{
    public class SharedService
    {

        public static void ShowErrorDialog(string ErrorMessage, UIViewController ViewController)
        {
            var OkAction = UIAlertAction.Create("OK", UIAlertActionStyle.Default, (UIAlertAction obj) =>
            {
                ViewController.DismissViewController(true, null);
            });

            var ErrorAlert = UIAlertController.Create("錯誤", ErrorMessage, UIAlertControllerStyle.Alert);
            ErrorAlert.AddAction((OkAction));
            ViewController.PresentViewController(ErrorAlert, true, null);
        }

        public static void WebExceptionHandler(WebException exception, UIViewController ViewController)
        {
            if (exception.Status == WebExceptionStatus.ProtocolError && exception.Response != null)
            {
                var response = (HttpWebResponse)exception.Response;
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        var content = reader.ReadToEnd();
                        ShowErrorDialog(content, ViewController);
                    }
                }
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    ShowErrorDialog("伺服器錯誤，請聯絡開發人員", ViewController);
                }
            }
            else
            {
                ShowErrorDialog("請檢察網路連線", ViewController);
            }
        }
    }
}
