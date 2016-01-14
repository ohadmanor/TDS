using System.Collections.Generic;
using System.Text;

using System;
using System.Windows.Interop;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

using System.Security.Permissions;

using System.Windows.Input;

using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows;



namespace TDSClient
{
    public class CursorHelper
    {
        private struct IconInfo
        {
          public bool fIcon;
          public int xHotspot;
          public int yHotspot;
          public IntPtr hbmMask;
          public IntPtr hbmColor;
        }

        [DllImport("user32.dll")]
        private static extern SafeIconHandle CreateIconIndirect(
            ref IconInfo icon);

        //private static extern IntPtr CreateIconIndirect(
        //    ref IconInfo icon);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetIconInfo(IntPtr hIcon,
            ref IconInfo pIconInfo);


        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);


        [DllImport("user32.dll")]
        public static extern bool DestroyIcon(IntPtr hIcon);



        public static Cursor InternalCreateCursor(System.Drawing.Bitmap bmp,
            int xHotSpot, int yHotSpot)
        {
          //IconInfo tmp = new IconInfo();
          //GetIconInfo(bmp.GetHicon(), ref tmp);
          //tmp.xHotspot = xHotSpot;
          //tmp.yHotspot = yHotSpot;
          //tmp.fIcon = false;

          //IntPtr ptr = CreateIconIndirect(ref tmp);
          //SafeFileHandle handle = new SafeFileHandle(ptr, true);
          //return CursorInteropHelper.Create(handle);



          var iconInfo = new IconInfo();
          GetIconInfo(bmp.GetHicon(), ref iconInfo);
          iconInfo.xHotspot = xHotSpot;
          iconInfo.yHotspot = yHotSpot;
          iconInfo.fIcon = false;

          SafeIconHandle cursorHandle = CreateIconIndirect(ref iconInfo);
          return CursorInteropHelper.Create(cursorHandle);

        }

        [SecurityPermission(SecurityAction.LinkDemand,UnmanagedCode=true)]
        private class SafeIconHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public SafeIconHandle()
                : base(true)
            {
            }
            protected override bool  ReleaseHandle()
            {
                return DestroyIcon(handle);
           }
          
        }


        public static Cursor CreateCursor(UIElement element, int xHotSpot, int yHotSpot,bool isTransparent)
        {
            try
            {
                element.Measure(new Size(double.PositiveInfinity,
                                double.PositiveInfinity));
                element.Arrange(new Rect(0, 0, element.DesiredSize.Width,
                  element.DesiredSize.Height));

                RenderTargetBitmap rtb =
                  new RenderTargetBitmap((int)element.DesiredSize.Width,
                    (int)element.DesiredSize.Height, 96, 96,
                    PixelFormats.Pbgra32);
                rtb.Render(element);

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));

                MemoryStream ms = new MemoryStream();
                encoder.Save(ms);

                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(ms);



                if (isTransparent)
                {

                    System.Drawing.Color backColor = bmp.GetPixel(1, 1);
                    bmp.MakeTransparent(backColor);
                }



                Cursor cur = InternalCreateCursor(bmp, xHotSpot, yHotSpot);

                ms.Close();
                ms.Dispose();


                //VictorDEBUG
                bmp.Dispose();

                return cur;
            }
            catch
            {
            }
            return null;

        }

        public static Cursor CreateCursor(UIElement element,int xHotSpot, int yHotSpot)
        {
            try
            {
                element.Measure(new Size(double.PositiveInfinity,
                  double.PositiveInfinity));
                element.Arrange(new Rect(0, 0, element.DesiredSize.Width,
                  element.DesiredSize.Height));

                RenderTargetBitmap rtb =
                  new RenderTargetBitmap((int)element.DesiredSize.Width,
                    (int)element.DesiredSize.Height, 96, 96,
                    PixelFormats.Pbgra32);
                rtb.Render(element);

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));

                MemoryStream ms = new MemoryStream();
                encoder.Save(ms);

                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(ms);

                //----------------------------------------            
                //Victor            

                System.Drawing.Color backColor = bmp.GetPixel(1, 1);
                bmp.MakeTransparent(backColor);
                //-----------------------------------------


                Cursor cur = InternalCreateCursor(bmp, xHotSpot, yHotSpot);

                ms.Close();
                ms.Dispose();

                //VictorDEBUG
                bmp.Dispose();

                return cur;
            }
            catch
            {

            }

            return null;
        }

        public static BitmapSource CreateTransparentElement(UIElement element)
        {
            BitmapSource retBitmap = null;

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)100,
                                    (int)50, 96, 96,
                                    PixelFormats.Pbgra32);
            rtb.Render(element);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            MemoryStream ms = new MemoryStream();
            encoder.Save(ms);

            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(ms);
            System.Drawing.Color backColor = bmp.GetPixel(1, 1);
            bmp.MakeTransparent(backColor);

            retBitmap = loadBitmap(bmp);
            return retBitmap;

        }

        private static BitmapSource loadBitmap(System.Drawing.Bitmap source)
        {
            BitmapSource BS = null;
            if (source == null)
                return null;
            //     using(System.Drawing.Bitmap bmp=(System.Drawing.Bitmap)source.Clone())
            {
                IntPtr hBitmap = source.GetHbitmap();
                BS = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty,
                  System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                 DeleteObject(hBitmap);
            }
            return BS;
            //source.Dispose();
        }

          public void CursorTest()
          {
            ////InitializeComponent();

            ////TextBlock tb = new TextBlock();
            ////tb.Text = "{ } Switch On The Code";
            ////tb.FontSize = 10;
            ////tb.Foreground = Brushes.Green;
            
            ////this.Cursor = CursorHelper.CreateCursor(tb, 5, 5);
          }





      }

      
}
