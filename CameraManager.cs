using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using SharpDX.MediaFoundation;
using ZXing;
using ZXing.Common;

namespace WebcamDll
{
    public class CameraManager : DynamicObject
    {
        private List<Camera> cameras;
        public CameraManager()
        {
            cameras = new List<Camera>();
            RegisterCameras();
        }
        public string Decode(Image img)
        {
            DecodingOptions options = new DecodingOptions
            {
                TryHarder = true,
                PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE, BarcodeFormat.PDF_417 }
            };
            BarcodeReader reader = new BarcodeReader();
            reader.Options = options;
            string return_value = "";
            try
            {
                return_value = reader.Decode(new Bitmap(img)).Text;
            } catch (Exception e)
            {
                Console.WriteLine("Error decoding image.");
            }

            return return_value;
        }
        public void RegisterCameras()
        {
            var camera_names = ListOfAttachedCameras();
            for (int i = 0; i < camera_names.Count(); i++)
            {
                cameras.Add(new Camera(camera_names[i]));
            }
        }
        public void StartCamera(string name)
        {
            var camera = cameras.Find(i => i.CompareName(name));
            camera.StartCamera();
        }
        public void TakePicture(string name)
        {
            var camera = cameras.Find(i => i.CompareName(name));
            camera.TakePicture().Save("Imagen.jpg");
        }

        public void StopCamera(string name)
        {
            var camera = cameras.Find(i => i.CompareName(name));
            camera.StopCamera();
        }
        public string CheckCamera(string name)
        {
            var camera = cameras.Find(i => i.CompareName(name));
            return camera.CheckStatus();
        }
        public string ReadQRCode(string name)
        {
            var camera = cameras.Find(i => i.CompareName(name));
            return camera.ReadQRCode();
        }
        public string ReadPDF417(string name)
        {
            var camera = cameras.Find(i => i.CompareName(name));
            return camera.ReadPDF417();
        }
        public static string[] ListOfAttachedCameras()
        {
            var cameras = new List<string>();
            var attributes = new MediaAttributes(1);
            attributes.Set(CaptureDeviceAttributeKeys.SourceType.Guid, CaptureDeviceAttributeKeys.SourceTypeVideoCapture.Guid);
            var devices = MediaFactory.EnumDeviceSources(attributes);
            for (var i = 0; i < devices.Count(); i++)
            {
                var friendlyName = devices[i].Get(CaptureDeviceAttributeKeys.FriendlyName);
                cameras.Add(friendlyName);
            }
            return cameras.ToArray();
        }
    }
}
