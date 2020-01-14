using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using System.Windows.Interop;
using System.Windows;
using System.Drawing.Imaging;

namespace ImageMorpher
{
	
	public class Morpher
	{
		Bitmap src;
		Bitmap dest;
		List<Bitmap> frames;
		public List<ControlLine> SrcLines { get; set; }
		public List<ControlLine> DestLines { get; set; }
		public double A_VALUE = 0.001;
		public double B_VALUE = 2;

		[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);

		public Morpher()
		{
			frames = new List<Bitmap>();
		}

		public int NumFrames { get; set; } = 1;
		public Bitmap bitmapfromSource(BitmapSource bms)
		{
			using (MemoryStream outStream = new MemoryStream())
			{
				BitmapEncoder enc = new BmpBitmapEncoder();
				enc.Frames.Add(BitmapFrame.Create(bms));
				enc.Save(outStream);
				System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);
				return new Bitmap(bitmap);
			}
		}

		public void setSrc(BitmapSource bms)
		{
			src = bitmapfromSource(bms);
		}

		public void setDest(BitmapSource bms)
		{
			dest = bitmapfromSource(bms);
		}

		private ControlLine makeDestLine(ControlLine cl, int frameNum)
		{
			ControlLine pair = cl.Pair;
			double xStartChange = pair.StartPixelX - cl.StartPixelX;
			double yStartChange = pair.StartPixelY - cl.StartPixelY;
			double xEndChange = pair.EndPixelX - cl.EndPixelX;
			double yEndChange = pair.EndPixelY - cl.EndPixelY;
			double xStartfactor = frameNum * xStartChange / (NumFrames + 1);
			double yStartfactor = frameNum * yStartChange / (NumFrames + 1);
			double xEndfactor = frameNum * xEndChange / (NumFrames + 1);
			double yEndfactor = frameNum * yEndChange / (NumFrames + 1);
			int startPX = (int)(cl.StartPixelX + xStartfactor);
			int startPY = (int)(cl.StartPixelY + yStartfactor);
			int endPX = (int)(cl.EndPixelX + xEndfactor);
			int endPY = (int)(cl.EndPixelY + yEndfactor);
			return new ControlLine(startPX, startPY, endPX, endPY);
		}

		private int[] createNewCoords(ControlLine dest, double distance, double fracLen)
		{
			int xCoord = (int)(dest.StartPixelX + fracLen * dest.VectX - distance * dest.NormX / dest.NormLength);
			int yCoord = (int)(dest.StartPixelY + fracLen * dest.VectY - distance * dest.NormY / dest.NormLength);
			if (xCoord < 0)
			{
				xCoord = 0;
			}
			if (xCoord >= src.Width)
			{
				xCoord = src.Width - 1;
			}
			if (yCoord < 0)
			{
				yCoord = 0;
			}
			if (yCoord >= src.Height)
			{
				yCoord = src.Height - 1;
			}
			int[] coords = new int[2];
			coords[0] = xCoord;
			coords[1] = yCoord;
			return coords;
		}

		public Bitmap createFrame(BitmapSource bms)
		{
			Bitmap bm = bitmapfromSource(bms);
			int[] xCoords = new int[SrcLines.Count];
			int[] yCoords = new int[SrcLines.Count];
			int[] xDeltas = new int[SrcLines.Count];
			int[] yDeltas = new int[SrcLines.Count];
			int[] weights = new int[SrcLines.Count];
			for (int i = 0; i < src.Width; i++)
			{
				for (int j = 0; j < src.Height; j++)
				{
					int[] srcCoords = getFrameCoords(SrcLines, i, j);
					int[] destCoords = getFrameCoords(DestLines, i, j);
					Color srcColor = src.GetPixel(srcCoords[0], srcCoords[1]);
					Color destColor = dest.GetPixel(destCoords[0], destCoords[1]);
					int red = (int)(srcColor.R * 0.5 + destColor.R * 0.5); //0.5 cause only one frame
					int green = (int) (srcColor.G * 0.5 + destColor.G * 0.5); //0.5 cause only one frame
					int blue = (int) (srcColor.B * 0.5 + destColor.B * 0.5); //0.5 cause only one frame
					int alpha = (int)(srcColor.A * 0.5 + destColor.A * 0.5); //0.5 cause only one frame
					bm.SetPixel(i, j, Color.FromArgb(alpha, red, green, blue));
				}
			}
			return bm;
		}

		private int[] getFrameCoords(List<ControlLine> lines, int i, int j)
		{

			double totalweights = 0;
			double avgXDelta = 0;
			double avgYDelta = 0;
			for (int k = 0; k < SrcLines.Count; k++)
			{
				ControlLine destLine = makeDestLine(lines[k], 1); // 1 is frame number
				double dist = ControlLine.distance(destLine, i, j);
				double fractLen = ControlLine.fracLength(destLine, i, j);
				int[] coords = createNewCoords(lines[0], dist, fractLen);
				double weight = Math.Pow(1 / A_VALUE + Math.Abs(dist), B_VALUE);
				totalweights += weight;
				avgXDelta += weight * (coords[0] - i);
				avgYDelta += Math.Pow(1 / A_VALUE + Math.Abs(dist), B_VALUE) * (coords[1] - j);
			}
			int[] frameCoords = new int[2];
			frameCoords[0] = i + (int)(avgXDelta / totalweights);
			frameCoords[1] = j + (int)(avgYDelta / totalweights);
			return frameCoords;
		}

		public BitmapImage convertToSource(Bitmap bm)
		{
			using (var memory = new MemoryStream())
			{
				bm.Save(memory, ImageFormat.Png); 
				memory.Position = 0;
				var bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.StreamSource = memory;
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.EndInit();
				bitmapImage.Freeze();

				return bitmapImage;
			}
		}

		public BitmapImage getFrame(BitmapSource bms)
		{
			Bitmap frame = createFrame(bms);
			return convertToSource(frame);
		}

	}
}
