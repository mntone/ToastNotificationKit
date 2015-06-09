using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Mntone.ToastNotificationServer.Frameworks
{
	public static class ImageHelper
	{
		public static WriteableBitmap CropAndResize(BitmapSource source, Rect sourceRect, Size expectedSize)
		{
			return CropAndResize(new WriteableBitmap(source), sourceRect, expectedSize);
		}

		public static WriteableBitmap CropAndResize(WriteableBitmap source, Rect sourceRect, Size expectedSize)
		{
			var sourceX = (int)sourceRect.X;
			var sourceY = (int)sourceRect.Y;
			var sourceWidth = source.PixelWidth;
			var sourceStride = 4 * sourceWidth;
			var sourceHeight = source.PixelHeight;
			if (sourceRect.Right > sourceWidth || sourceRect.Bottom > sourceHeight)
			{
				throw new ArgumentOutOfRangeException();
			}

			var resultWidth = (int)expectedSize.Width;
			var resultHeight = (int)expectedSize.Height;
			var wr = sourceRect.Width / expectedSize.Width;
			var hr = sourceRect.Height / expectedSize.Height;

			var destination = new WriteableBitmap(resultWidth, resultHeight, 96, 96, PixelFormats.Bgra32, null);
			destination.Lock();
			unsafe
			{
				byte* src = (byte*)source.BackBuffer;
				byte* ptr = (byte*)destination.BackBuffer;
				for (var i = 0; i < resultHeight; ++i)
				{
					for (var j = 0; j < resultWidth; ++j)
					{
						byte* srcPixel = src + sourceStride * (sourceY + (int)(i * hr)) + 4 * (sourceX + (int)(j * wr));
						*ptr++ = srcPixel[0];
						*ptr++ = srcPixel[1];
						*ptr++ = srcPixel[2];
						*ptr++ = srcPixel[3];
					}
				}
			}
			destination.Unlock();
			destination.Freeze();
			return destination;
		}
	}
}