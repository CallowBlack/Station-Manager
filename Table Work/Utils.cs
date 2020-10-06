using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace StationManager
{
    class Utils
    {
        public static byte[] GetImageBytes(BitmapImage image)
        {
            byte[] data;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }
            return data;
        }

        public static void SaveImage(string filepath, BitmapImage image)
        {
            File.WriteAllBytes(filepath, GetImageBytes(image));
        }

        public static BitmapImage LoadImage(string filepath)
        {
            return LoadImage(File.ReadAllBytes(filepath));
        }

        public static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public static void SetAllProperties(object obj, dynamic properties)
        {
            foreach (var property in obj.GetType().GetProperties())
            {
                var value = property.GetType().GetProperty(property.Name).GetValue(properties, null);
                if (value != null)
                {
                    property.SetValue(obj, value);
                }
            }
        }
    }
}
