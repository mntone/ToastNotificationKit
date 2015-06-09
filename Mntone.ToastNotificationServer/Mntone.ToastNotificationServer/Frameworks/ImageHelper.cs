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
				var src = (int*)source.BackBuffer;
				var ptr = (int*)destination.BackBuffer;
				for (var i = 0; i < resultHeight; ++i)
				{
					for (var j = 0; j < resultWidth; ++j)
					{
						var srcPixel = src + sourceWidth * (sourceY + (int)(i * hr)) + (sourceX + (int)(j * wr));
						*ptr++ = *srcPixel;
					}
				}
			}
			destination.Unlock();
			destination.Freeze();
			return destination;
		}
	}
}