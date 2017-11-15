using System;
using Foundation;
using FrogCroakCL.Models;
using UIKit;
using FrogCroakCL.Services;
using System.Collections.Generic;
using FrogCroak.MyViews;
using CoreGraphics;
using FrogCroak.MyClassLibrary;
using System.Threading.Tasks;
using System.Net;

namespace FrogCroak.ViewControllers
{
    public partial class MessageViewController : UIViewController
    {
        private bool isKeyboardShown = false;
        private float keyboardHeight = 0;

        public ChatMessageTableViewSource chatMessageTableViewSource;

        public MessageViewController() : base("MessageViewController", null)
        {
        }

        protected MessageViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
            setPushFrameAndTapCloseKB();
            chatMessageTableViewSource = new ChatMessageTableViewSource(View.Frame.Width * 0.7f, table_Message);
            table_Message.Source = chatMessageTableViewSource;
            tf_Message.Delegate = new ChatMessageTextFieldDelegate(this);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        private void setPushFrameAndTapCloseKB()
        {
            //監控開啟關閉鍵盤
            NSNotificationCenter.DefaultCenter.AddObserver((NSString)"UIKeyboardWillShowNotification", (NSNotification note) =>
            {
                if (isKeyboardShown)
                {
                    return;
                }

                var keyboardAnimationDetail = note.UserInfo;
                var keyboardFrameValue = (NSValue)keyboardAnimationDetail[UIKeyboard.FrameBeginUserInfoKey];
                var keyboardFrame = keyboardFrameValue.CGRectValue;

                if (keyboardFrame.Height != 0)
                {
                    keyboardHeight = (float)keyboardFrame.Height;
                }

                double duration = (double)(NSNumber)keyboardAnimationDetail[UIKeyboard.AnimationDurationUserInfoKey];

                UIView.Animate(duration, () =>
                {
                    var Frame = ParentViewController.View.Frame;
                    Frame.Y = Frame.Y - keyboardHeight;
                    ParentViewController.View.Frame = Frame;
                });

                isKeyboardShown = true;
            });

            NSNotificationCenter.DefaultCenter.AddObserver((NSString)"UIKeyboardWillHideNotification", (NSNotification note) =>
            {
                if (!isKeyboardShown)
                {
                    return;
                }

                var keyboardAnimationDetail = note.UserInfo;

                double duration = (double)(NSNumber)keyboardAnimationDetail[UIKeyboard.AnimationDurationUserInfoKey];

                UIView.Animate(duration, () =>
                {
                    var Frame = ParentViewController.View.Frame;
                    Frame.Y = Frame.Y + keyboardHeight;
                    ParentViewController.View.Frame = Frame;
                });

                isKeyboardShown = false;
            });

            //監控點擊畫面
            var singleFinger = new UITapGestureRecognizer((UITapGestureRecognizer obj) =>
            {
                tf_Message.ResignFirstResponder();
            });

            //點1下觸發
            singleFinger.NumberOfTapsRequired = 1;

            //一根手指觸發
            singleFinger.NumberOfTouchesRequired = 1;

            //為視圖加入監聽手勢
            this.View.AddGestureRecognizer(singleFinger);
        }

        partial void pressSendMessage(UIButton sender)
        {
            chatMessageTableViewSource.CreateData(tf_Message, this);
        }

        partial void pressSelectImage(UIButton sender)
        {
            var ImagePicker = new UIImagePickerController();
            ImagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
            ImagePicker.FinishedPickingMedia += (mSender, e) =>
            {
                switch (e.Info[UIImagePickerController.MediaType].ToString())
                {
                    case "public.video":
                        DismissViewController(true, null);
                        return;
                }
                var SelectedImage = (UIImage)e.Info[UIImagePickerController.OriginalImage];

                var DocUrl = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                var FileName = $"{DateTime.Now.GetHashCode().ToString()}.jpg";
                var FilePath = System.IO.Path.Combine(DocUrl, FileName);

                nfloat Quality = 1.0f;
                var ImgData = SelectedImage.AsJPEG(Quality);
                while (ImgData.Length / 1024 > 250 && Quality > 0.1)
                {
                    Quality -= 0.1f;
                    ImgData = SelectedImage.AsJPEG(Quality);
                }

                NSError Err = null;
                if (ImgData.Save(FilePath, false, out Err))
                {
                    DismissViewController(true, null);
                    chatMessageTableViewSource.CreateImageData(FileName, this);
                }
                else
                {
                    SharedService.ShowErrorDialog("儲存圖片失敗", this);
                }

            };

            ImagePicker.Canceled += (mSender, e) =>
            {
                DismissViewController(true, null);
            };

            PresentViewController(ImagePicker, true, null);
            //NavigationController.PresentModalViewController(ImagePicker, true);
        }

    }

    public class ChatMessageTableViewSource : UITableViewSource
    {
        private UITableView table_Message;
        private nfloat contentMaxWidth = 0;

        private List<ChatMessage> chatMessageList;
        public ChatMessageService chatMessageService;

        private NSCache imageCache = new NSCache();

        private Dictionary<string, nfloat> rawValueDictionary;

        private bool isFirstLoad = true;
        private bool isLoading = false;
        private bool isFinishLoad = false;

        private int nowPage = 0;

        public ChatMessageTableViewSource(nfloat ContentMaxWidth, UITableView Table_Message)
        {
            rawValueDictionary = new Dictionary<string, nfloat>();
            chatMessageService = new ChatMessageService();
            chatMessageList = new List<ChatMessage>();
            contentMaxWidth = ContentMaxWidth;
            table_Message = Table_Message;
            ReadData();
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell cell;

            if (chatMessageList[indexPath.Row].Type == 1)
            {
                cell = tableView.DequeueReusableCell("MyImage", indexPath);

                var iv_MyImageView = (UIImageView)cell.ContentView.Subviews[0];
                iv_MyImageView.TranslatesAutoresizingMaskIntoConstraints = true;


                var DocUrl = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                var FilePath = (NSString)System.IO.Path.Combine(DocUrl, chatMessageList[indexPath.Row].Message);
                try
                {
                    //從快取取出
                    var Image = (UIImage)imageCache.ObjectForKey(FilePath);
                    if (Image == null)
                    {
                        var ImageData = NSData.FromFile(FilePath);
                        Image = new UIImage(ImageData);
                        //存入快取
                        imageCache.SetObjectforKey(Image, FilePath);
                    }
                    nfloat ih;
                    rawValueDictionary.TryGetValue($"{chatMessageList[indexPath.Row].Message}ih", out ih);
                    if (ih == 0)
                    {
                        if (Image.Size.Width > contentMaxWidth)
                            rawValueDictionary[$"{chatMessageList[indexPath.Row].Message}ih"] = Image.Size.Height / (Image.Size.Width / contentMaxWidth);
                        else
                            rawValueDictionary[$"{chatMessageList[indexPath.Row].Message}ih"] = Image.Size.Height * (contentMaxWidth / Image.Size.Width);
                    }
                    var IVFrame = iv_MyImageView.Frame;
                    IVFrame.Width = contentMaxWidth;
                    IVFrame.Height = rawValueDictionary[$"{chatMessageList[indexPath.Row].Message}ih"];
                    IVFrame.X = tableView.Frame.Width - contentMaxWidth - 10;
                    iv_MyImageView.Frame = IVFrame;

                    iv_MyImageView.Image = Image;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                return cell;
            }

            PaddingLabel l_Message;

            if (chatMessageList[indexPath.Row].Isme)
            {
                cell = tableView.DequeueReusableCell("Right", indexPath);
                l_Message = (PaddingLabel)cell.ContentView.Subviews[0];
            }
            else
            {
                cell = tableView.DequeueReusableCell("Left", indexPath);
                l_Message = (PaddingLabel)cell.ContentView.Subviews[2];
            }

            l_Message.TranslatesAutoresizingMaskIntoConstraints = true;
            l_Message.Text = chatMessageList[indexPath.Row].Message;

            nfloat lh;
            rawValueDictionary.TryGetValue($"{chatMessageList[indexPath.Row].Message}lh", out lh);
            if (lh == 0)
            {
                NSString tempText = (NSString)l_Message.Text;
                var rect = tempText.GetBoundingRect(
                    new CGSize(contentMaxWidth - 40, 1000),
                    NSStringDrawingOptions.UsesLineFragmentOrigin,
                    new UIStringAttributes()
                    {
                        Font = l_Message.Font
                    },
                    null);


                rawValueDictionary[$"{chatMessageList[indexPath.Row].Message}lh"] = rect.Height + 40;
                rawValueDictionary[$"{chatMessageList[indexPath.Row].Message}lw"] = rect.Width + 42;


                if (chatMessageList[indexPath.Row].Isme)
                    rawValueDictionary[$"{chatMessageList[indexPath.Row].Message}lx"] = tableView.Frame.Width - rect.Width - 52;
                else
                    rawValueDictionary[$"{chatMessageList[indexPath.Row].Message}lx"] = cell.ContentView.Subviews[0].Frame.Width + 20;
            }
            var Frame = l_Message.Frame;
            Frame.Height = rawValueDictionary[$"{chatMessageList[indexPath.Row].Message}lh"];
            Frame.Width = rawValueDictionary[$"{chatMessageList[indexPath.Row].Message}lw"];
            Frame.X = rawValueDictionary[$"{chatMessageList[indexPath.Row].Message}lx"];
            l_Message.Frame = Frame;

            l_Message.Layer.CornerRadius = 20;
            l_Message.Layer.MasksToBounds = true;


            if (!isLoading && !isFinishLoad && indexPath.Row > chatMessageList.Count * 0.7)
            {
                isLoading = true;
                ReadData();
            }

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return chatMessageList.Count;
        }

        private void ReadData()
        {
            List<ChatMessage> ChatMessageList = chatMessageService.GetChatMessageList(nowPage);
            nowPage++;
            chatMessageList.AddRange(ChatMessageList);
            if (ChatMessageList.Count < 15)
                isFinishLoad = true;

            if (isFirstLoad)
                isFirstLoad = false;
            else
            {
                table_Message.ReloadData();
                isLoading = false;
            }
        }

        public async void CreateData(UITextField tf_ChatMessage, UIViewController ViewController)
        {
            string Message = tf_ChatMessage.Text;
            if (Message.Trim() != "")
            {
                tf_ChatMessage.ResignFirstResponder();
                tf_ChatMessage.Text = "";

                var chatMessage = new ChatMessage();
                chatMessage.Message = Message;
                chatMessage.Isme = true;
                chatMessage.Type = 0;

                bool IsSuccess = chatMessageService.CreateChatMessage(chatMessage);
                if (IsSuccess)
                {
                    chatMessageList.Insert(0, chatMessage);
                    UpdateTableViewRow();
                }
                else
                {
                    SharedService.ShowErrorDialog("新增資料失敗", ViewController);
                    return;
                }

                AllRequestResult result = null;
                await Task.Run(async () =>
                {
                    result = await chatMessageService.CreateMessage(chatMessage.Message);
                });
                if (result.IsSuccess)
                {
                    string ResMessage = (string)result.Result;
                    ResMessage = ResMessage.Replace("\\n", "\n");
                    var ResChatMessage = new ChatMessage();
                    ResChatMessage.Message = ResMessage;
                    ResChatMessage.Isme = false;
                    ResChatMessage.Type = 0;

                    IsSuccess = chatMessageService.CreateChatMessage(ResChatMessage);

                    if (IsSuccess)
                    {
                        chatMessageList.Insert(0, ResChatMessage);
                        UpdateTableViewRow();
                    }
                    else
                    {
                        SharedService.ShowErrorDialog("新增資料失敗", ViewController);
                        return;
                    }
                }
                else
                {
                    SharedService.WebExceptionHandler((WebException)result.Result, ViewController);
                }
            }
        }

        public async void CreateImageData(string FileName, UIViewController ViewController)
        {
            var chatMessage = new ChatMessage();
            chatMessage.Message = FileName;
            chatMessage.Isme = true;
            chatMessage.Type = 1;

            bool IsSuccess = chatMessageService.CreateChatMessage(chatMessage);

            if (IsSuccess)
            {
                chatMessageList.Insert(0, chatMessage);
                UpdateTableViewRow();

                AllRequestResult result = null;
                var DocUrl = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                await Task.Run(async () =>
                {
                    result = await chatMessageService.UploadImage(DocUrl + "/" + FileName, "jpeg");
                });

                if (result.IsSuccess)
                {
                    chatMessage = new ChatMessage();
                    chatMessage.Message = (string)result.Result;
                    chatMessage.Isme = false;
                    chatMessage.Type = 0;

                    IsSuccess = chatMessageService.CreateChatMessage(chatMessage);

                    if (IsSuccess)
                    {
                        chatMessageList.Insert(0, chatMessage);
                        UpdateTableViewRow();
                    }
                    else
                    {
                        SharedService.ShowErrorDialog("新增資料失敗", ViewController);
                        return;
                    }
                }
                else
                {
                    SharedService.WebExceptionHandler((WebException)result.Result, ViewController);
                }
            }
            else
            {
                SharedService.ShowErrorDialog("新增資料失敗", ViewController);
                return;
            }
        }

        private void UpdateTableViewRow()
        {
            table_Message.BeginUpdates();
            table_Message.InsertRows(new[] { NSIndexPath.FromRowSection(0, 0) }, UITableViewRowAnimation.Automatic);
            table_Message.EndUpdates();

            table_Message.ScrollToRow(NSIndexPath.FromRowSection(0, 0), UITableViewScrollPosition.Top, true);
        }
    }

    public class ChatMessageTextFieldDelegate : UITextFieldDelegate
    {
        private MessageViewController messageViewController;

        public ChatMessageTextFieldDelegate(MessageViewController messageViewController)
        {
            this.messageViewController = messageViewController;
        }

        public override bool ShouldReturn(UITextField textField)
        {
            messageViewController.chatMessageTableViewSource.CreateData(textField, messageViewController);
            return true;
        }
    }
}