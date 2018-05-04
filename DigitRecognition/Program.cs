using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DigitRecognition
{
    class Program
    {
        [DllImport("HWRDLL.dll", EntryPoint = "GetPictureWord", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetPictureWord(string imageFilePath, string outputFilePath);

        [DllImport("HWRDLL.dll", EntryPoint = "DigitRecognition", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public static extern int DigitRecognition(string imageFilePath, string templateFilePath);
        static void Main(string[] args)
        {

            if (args.Length == 1)
            {
                try
                {
                    var val = GetPicNumber(args[0]);
                    Console.WriteLine(val.Result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
            else if (args.Length == 2 && args[1] == "Local")
            {
                try
                {
                    var val = GetPicNumberByLocal(args[0]);
                    Console.WriteLine(val);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
            else if (args.Length == 2 && args[1] == "LocalList")
            {
                try
                {
                    var list = args[0].Split(',').ToList();
                    var val = GetPicNumberByLocalList(list);
                    Console.WriteLine(string.Join(",", val));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static async Task<string> GetPicNumber(string picUrl)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var picPath = $@"{path}Upload\Pic\{DateTime.Now:yyyy/MM/dd}";
            if (!Directory.Exists(picPath))
                Directory.CreateDirectory(picPath);

            string inPicPath = $@"{picPath}\{Guid.NewGuid()}.jpg";
            var val = "";
            try
            {
                using (var myWebClient = new WebClient())
                {
                    await myWebClient.DownloadFileTaskAsync(new Uri(picUrl), inPicPath);
                }

                var picSavePath = $@"{picPath}\{Guid.NewGuid()}.bmp";
                using (var bitmap = new Bitmap(inPicPath))
                {
                    // 指定8位格式，即256色
                    BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
                    Bitmap bitmap2 = new Bitmap(bitmap.Width, bitmap.Height, data.Stride, PixelFormat.Format8bppIndexed, data.Scan0);
                    bitmap2.Save(picSavePath, ImageFormat.Bmp);
                    bitmap.UnlockBits(data);
                }

                var outSavePath = $@"{picPath}\{Guid.NewGuid()}";
                if (!Directory.Exists(outSavePath))
                    Directory.CreateDirectory(outSavePath);
                foreach (var file in Directory.GetFiles(outSavePath, "*.bmp"))
                {
                    File.Delete(file);
                }

                GetPictureWord(picSavePath, outSavePath);

                var dataPath = $@"{path}Data/template.dat";
                foreach (var img in Directory.GetFiles(outSavePath, "*.bmp"))
                {
                    var picNumber = DigitRecognition(img, dataPath);
                    if (picNumber < 0)
                        throw new Exception("识别错误！");
                    val += picNumber.ToString();
                }
                Directory.Delete(outSavePath, true);
                File.Delete(inPicPath);
                File.Delete(picSavePath);
            }
            catch (Exception ex)
            {
            }
            return val;
        }

        private static string GetPicNumberByLocal(string inPicPath)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var picPath = $@"{path}Upload\Pic\{DateTime.Now:yyyy/MM/dd}";
            if (!Directory.Exists(picPath))
                Directory.CreateDirectory(picPath);
            var val = "";
            try
            {
                var picSavePath = $@"{picPath}\{Guid.NewGuid()}.bmp";
                using (Bitmap bitmap = new Bitmap(inPicPath))
                {
                    // 指定8位格式，即256色
                    BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
                    Bitmap bitmap2 = new Bitmap(bitmap.Width, bitmap.Height, data.Stride, PixelFormat.Format8bppIndexed, data.Scan0);
                    bitmap2.Save(picSavePath, ImageFormat.Bmp);
                    bitmap.UnlockBits(data);
                }

                var outSavePath = $@"{picPath}\{Guid.NewGuid()}";
                if (!Directory.Exists(outSavePath))
                    Directory.CreateDirectory(outSavePath);
                else
                {
                    foreach (var file in Directory.GetFiles(outSavePath, "*.bmp"))
                    {
                        File.Delete(file);
                    }
                }
                GetPictureWord(picSavePath, outSavePath);

                var dataPath = $@"{path}Data/template.dat";
                foreach (var img in Directory.GetFiles(outSavePath, "*.bmp"))
                {
                    var picNumber = DigitRecognition(img, dataPath);
                    if (picNumber < 0)
                        throw new Exception("识别错误！");
                    val += picNumber.ToString();
                }
                Directory.Delete(outSavePath, true);

                File.Delete(picSavePath);
            }
            catch (Exception ex)
            {
            }
            return val;
        }

        private static List<string> GetPicNumberByLocalList(List<string> picPathList)
        {
            var list = new List<string>();
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var picPath = $@"{path}Upload\Pic\{DateTime.Now:yyyy/MM/dd}";
            if (!Directory.Exists(picPath))
                Directory.CreateDirectory(picPath);

            foreach (var item in picPathList.Where(p => p != "").ToList())
            {
                var val = "";
                var picSavePath = $@"{picPath}\{Guid.NewGuid()}.bmp";
                try
                {
                    using (Bitmap bitmap = new Bitmap(item))
                    {
                        // 指定8位格式，即256色
                        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
                        Bitmap bitmap2 = new Bitmap(bitmap.Width, bitmap.Height, data.Stride, PixelFormat.Format8bppIndexed, data.Scan0);
                        bitmap2.Save(picSavePath, ImageFormat.Bmp);
                        bitmap.UnlockBits(data);
                    }
                    var outSavePath = $@"{picPath}\{Guid.NewGuid()}";
                    if (!Directory.Exists(outSavePath))
                        Directory.CreateDirectory(outSavePath);
                    else
                    {
                        foreach (var file in Directory.GetFiles(outSavePath, "*.bmp"))
                        {
                            File.Delete(file);
                        }
                    }
                    GetPictureWord(picSavePath, outSavePath);
                    var dataPath = $@"{path}Data/template.dat";
                    foreach (var img in Directory.GetFiles(outSavePath, "*.bmp"))
                    {
                        var picNumber = DigitRecognition(img, dataPath);
                        if (picNumber < 0)
                            throw new Exception("识别错误！");
                        val += picNumber.ToString();
                    }
                    Directory.Delete(outSavePath, true);
                    File.Delete(picSavePath);
                }
                catch
                {
                }
                list.Add(val);
            }
            return list;
        }
    }
}
