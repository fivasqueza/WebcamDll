using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Structure;
using SharpDX.MediaFoundation;
using ZXing;
using ZXing.Common;

namespace WebcamDll
{
    public class Camera : DynamicObject
    {
        private string name;
        private VideoCapture capture;
        private bool camera_active;
        
        public Camera(string _name)
        {
            name = _name;
            camera_active = false;
        }
        public void StartCamera()
        {
            if (camera_active) return;

            int camera_index = GetCameraIndexForPartName(name);

            capture = new VideoCapture(camera_index);
            capture.Start();
            Mat test_result = capture.QueryFrame();

            bool camera_available = test_result != null;
            
            if (!camera_available) throw new Exception("Camera unavalaible or in use by another program.");
            camera_active = true;
            
        }
        public string CheckStatus()
        {
            if (camera_active)
            {
                return "Camera active.";
            }
            else
            {
                int camera_index = GetCameraIndexForPartName(name);

                capture = new VideoCapture(camera_index);
                capture.Start();
                Mat test_result = capture.QueryFrame();
                
                if (test_result != null)
                {
                    capture.Dispose();
                    return "Camera is accesible for use.";
                }
                else
                {
                    return "Camera unavailable for use.";
                }
            }
        }

        public Bitmap TakePicture()
        {
            if (!camera_active) throw new Exception("Camera not currently active.");
            return capture.QueryFrame().ToBitmap();
        }

        public string ReadQRCode()
        {
            DecodingOptions options = new DecodingOptions
            {
                TryHarder = true,
                PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE }
            };
            return ReadCode(options); 
        }
        public string ReadPDF417()
        {
            DecodingOptions options = new DecodingOptions
            {
                TryHarder = true,
                PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.PDF_417 }
            };
            return ReadCode(options);
        }

        private string ReadCode(DecodingOptions options)
        {
            BarcodeReader reader = new BarcodeReader();
            reader.Options = options;
            try
            {
                int n = 0;
                Result result = null;
                while (result == null && n < 100)
                {
                    n++;
                    Bitmap photo = TakePicture();
                    result = reader.Decode(photo);
                }

                return result.Text;
            }
            catch (Exception)
            {
                return "Error";
            }

        }
        public void StopCamera()
        {
            if (camera_active)
            {
                capture.Dispose();
                camera_active = false;
            }
        }

        public bool CompareName(string input_name)
        {
            return string.Equals(name, input_name, System.StringComparison.OrdinalIgnoreCase);
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
        private static int GetCameraIndexForPartName(string partName)
        {
            var cameras = ListOfAttachedCameras();
            for (var i = 0; i < cameras.Count(); i++)
            {
                if (cameras[i].ToLower().Contains(partName.ToLower()))
                {
                    return i;
                }
            }
            return -1;
        }
    }

    
}
