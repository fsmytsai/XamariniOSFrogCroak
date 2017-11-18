using System;
using System.Net;
using System.Threading.Tasks;
using CoreLocation;
using Foundation;
using FrogCroak.MyClassLibrary;
using FrogCroakCL.Models;
using FrogCroakCL.Services;
using MapKit;
using UIKit;
using static FrogCroakCL.Models.MyMarkers;

namespace FrogCroak.ViewControllers
{
    public partial class MapViewController : UIViewController
    {
        private MKMapView map;

        public MapViewController() : base("MapViewController", null)
        {
        }

        protected MapViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
            map = new MKMapView(UIScreen.MainScreen.Bounds);

            var coordinate = new CLLocationCoordinate2D(23.674764, 120.92);
            var span = new MKCoordinateSpan(3, 3);
            var region = new MKCoordinateRegion(coordinate, span);
            map.SetRegion(region, true);
            map.ZoomEnabled = false;
            map.ScrollEnabled = false;
            map.Delegate = new MyMapDelegate(this);
            View = map;

            GetMyMarkers();
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        private async void GetMyMarkers()
        {
            AllRequestResult result = null;
            await Task.Run(() =>
            {
                result = new MarkerService().GetMyMarkerList();
            });
            if (result.IsSuccess)
            {
                MyMarkers myMarkers = (MyMarkers)result.Result;
                MKPointAnnotation annotation;
                foreach (MyMarker myMarker in myMarkers.MarkerList)
                {
                    annotation = new MKPointAnnotation()
                    {
                        Title = myMarker.Title,
                        Subtitle = myMarker.Content.Replace("\\n", "\n"),
                        Coordinate = new CLLocationCoordinate2D(myMarker.Latitude, myMarker.Longitude)
                    };
                    this.map.AddAnnotation(annotation);
                }
            }
            else
            {
                SharedService.WebExceptionHandler((WebException)result.Result, this);
            }
        }



    }

    class MyMapDelegate : MKMapViewDelegate
    {
        private UIViewController ViewController;

        public MyMapDelegate(UIViewController ViewController)
        {
            this.ViewController = ViewController;
        }

        public override MKAnnotationView GetViewForAnnotation(MKMapView mapView, IMKAnnotation annotation)
        {
            string Identifier = "MyPin";
            var result = mapView.DequeueReusableAnnotation(Identifier);
            if (result == null)
            {
                result = new MKAnnotationView(annotation, Identifier);
            }
            else
            {
                result.Annotation = annotation;
            }
            result.CanShowCallout = true;
            result.Image = UIImage.FromBundle("normal");
            result.RightCalloutAccessoryView = UIButton.FromType(UIButtonType.DetailDisclosure);
            return result;
        }

        public override void CalloutAccessoryControlTapped(MKMapView mapView, MKAnnotationView view, UIControl control)
        {
            var OkAction = UIAlertAction.Create("OK", UIAlertActionStyle.Default, (UIAlertAction obj) =>
            {
                ViewController.DismissViewController(true, null);
            });
            var ErrorAlert = UIAlertController.Create(view.Annotation.GetTitle(), view.Annotation.GetSubtitle(), UIAlertControllerStyle.Alert);
            ErrorAlert.AddAction((OkAction));
            ViewController.PresentViewController(ErrorAlert, true, null);
        }
    }
}