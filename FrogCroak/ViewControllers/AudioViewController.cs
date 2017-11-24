using System;
using System.Net;
using System.Threading.Tasks;
using AVFoundation;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using FrogCroak.MyClassLibrary;
using FrogCroakCL.Models;
using FrogCroakCL.Services;
using UIKit;

namespace FrogCroak.ViewControllers
{
    public partial class AudioViewController : UIViewController
    {
        private AVAudioRecorder audioRecorder;
        private bool isRecording = false;

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

        partial void Record(UIButton sender)
        {
            if (AVAudioSession.SharedInstance().RecordPermission == AVAudioSessionRecordPermission.Undetermined)
            {
                AVAudioSession.SharedInstance().RequestRecordPermission((bool granted) =>
                {
                    if (granted)
                        DispatchQueue.MainQueue.DispatchAsync(() =>
                        {
                            StartRecord();
                        });
                    else
                        SharedService.ShowErrorDialog("無法取得錄音權限，請至設定內修改隱私權限", this);
                });
            }
            else if (AVAudioSession.SharedInstance().RecordPermission == AVAudioSessionRecordPermission.Granted)
                StartRecord();
            else
                SharedService.ShowErrorDialog("無法取得錄音權限，請至設定內修改隱私權限", this);

        }

        private void StartRecord()
        {
            if (!isRecording)
            {
                PerformSegue("showPopover", null);
                l_Result.Text = "結果";
                iv_ResultFrogImage.Image = null;
                var DocUrl = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string FileName = "Frog.wav";
                string FilePath = System.IO.Path.Combine(DocUrl, FileName);
                var url = new NSUrl(FilePath);

                NSError error = AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.PlayAndRecord);
                if (error != null)
                    Console.WriteLine(error.LocalizedDescription);

                var settings = new AudioSettings()
                {
                    Format = AudioToolbox.AudioFormatType.LinearPCM,
                    NumberChannels = 2,
                    EncoderAudioQualityForVBR = AVAudioQuality.Max,
                    SampleRate = 44100f,
                    LinearPcmBitDepth = 16,
                    LinearPcmBigEndian = true,
                    LinearPcmFloat = true
                };

                audioRecorder = AVAudioRecorder.Create(url, settings, out error);
                if (error != null)
                {
                    audioRecorder = null;
                    Console.WriteLine(error.LocalizedDescription);
                }
                else
                {
                    audioRecorder.Delegate = new MyAVAudioRecorderDelegate(this);
                    audioRecorder.PrepareToRecord();
                    audioRecorder.Record();
                    bt_Record.SetImage(UIImage.FromBundle("recordingbtn"), UIControlState.Normal);
                    isRecording = true;
                }

            }
        }

        public void StopRecord()
        {
            audioRecorder.Stop();
            bt_Record.SetImage(UIImage.FromBundle("recordbtn"), UIControlState.Normal);
            isRecording = false;
            var audioSession = AVAudioSession.SharedInstance();

            NSError error = audioSession.SetCategory(AVAudioSessionCategory.Playback);
            error = audioSession.SetActive(false);

            if (error != null)
                Console.WriteLine(error.LocalizedDescription);
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, Foundation.NSObject sender)
        {
            base.PrepareForSegue(segue, sender);
            if (segue.Identifier == "showPopover")
            {
                var vc = segue.DestinationViewController;
                vc.PreferredContentSize = new CGSize(175, 35);
                vc.View.BackgroundColor = UIColor.FromRGB(0.2314f, 0.3176f, 0.0784f);
                var controller = vc.PopoverPresentationController;
                if (controller != null)
                {
                    controller.BackgroundColor = UIColor.FromRGB(0.2314f, 0.3176f, 0.0784f);
                    controller.Delegate = new MyUIPopoverPresentationControllerDelegate(this);
                }
            }
        }

        public async void UploadWav(string AbsolutePath)
        {

            AllRequestResult result = null;
            await Task.Run(async () =>
            {
                result = await new SoundRecognitionService().SoundRecognition(AbsolutePath);
            });
            if (result.IsSuccess)
            {
                string FrogName = (string)result.Result;
                l_Result.Text = FrogName;
                if (FrogName == "台北樹蛙")
                    iv_ResultFrogImage.Image = UIImage.FromBundle("tptreefrog");
                else if (FrogName == "面天樹蛙")
                    iv_ResultFrogImage.Image = UIImage.FromBundle("facefrog");
                else if (FrogName == "澤蛙")
                    iv_ResultFrogImage.Image = UIImage.FromBundle("waterfrog");
                else if (FrogName == "小雨蛙")
                    iv_ResultFrogImage.Image = UIImage.FromBundle("rainfrog");
                else if (FrogName == "艾氏樹蛙")
                    iv_ResultFrogImage.Image = UIImage.FromBundle("aistreefrog");
                else if (FrogName == "拉都希氏赤蛙")
                    iv_ResultFrogImage.Image = UIImage.FromBundle("ladofrog");
                else if (FrogName == "虎皮蛙")
                    iv_ResultFrogImage.Image = UIImage.FromBundle("lionskinfrog");
                else if (FrogName == "豎琴蛙")
                    iv_ResultFrogImage.Image = UIImage.FromBundle("harpfrog");
                else if (FrogName == "布氏樹蛙")
                    iv_ResultFrogImage.Image = UIImage.FromBundle("boostreefrog");
                else if (FrogName == "貢德氏赤蛙")
                    iv_ResultFrogImage.Image = UIImage.FromBundle("gondesfrog");
                else
                    iv_ResultFrogImage.Image = UIImage.FromBundle("people");
            }
            else
            {
                SharedService.WebExceptionHandler((WebException)result.Result, this);
            }

        }
    }

    class MyUIPopoverPresentationControllerDelegate : UIPopoverPresentationControllerDelegate
    {
        private AudioViewController ViewController;
        public MyUIPopoverPresentationControllerDelegate(AudioViewController ViewController)
        {
            this.ViewController = ViewController;
        }

        public override UIModalPresentationStyle GetAdaptivePresentationStyle(UIPresentationController forPresentationController)
        {
            return UIModalPresentationStyle.None;
        }

        public override void DidDismissPopover(UIPopoverPresentationController popoverPresentationController)
        {
            ViewController.StopRecord();
        }
    }

    class MyAVAudioRecorderDelegate : AVAudioRecorderDelegate
    {
        private AudioViewController ViewController;
        public MyAVAudioRecorderDelegate(AudioViewController ViewController)
        {
            this.ViewController = ViewController;
        }

        public override void FinishedRecording(AVAudioRecorder recorder, bool flag)
        {
            ViewController.UploadWav(recorder.Url.AbsoluteString);
        }
    }
}

