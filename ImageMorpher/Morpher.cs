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
		public List<BitmapSource> Frames { set; get; }
		public List<ControlLine> SrcLines { get; set; }
		public List<ControlLine> DestLines { get; set; }
		public double A_VALUE = 0.001;
		public double B_VALUE = 2;
		public bool Morphed = false;
		public string UUID;
		public int index = 0;
		public Morpher()
		{
			Frames = new List<BitmapSource>();
			UUID = Guid.NewGuid().ToString();
		}

		public static int NumFrames { get; set; } = 1;
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
			double xStartfactor = frameNum * xStartChange / (NumFrames + 1.0);
			double yStartfactor = frameNum * yStartChange / (NumFrames + 1.0);
			double xEndfactor = frameNum * xEndChange / (NumFrames + 1.0);
			double yEndfactor = frameNum * yEndChange / (NumFrames + 1.0);
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

		public Bitmap createFrame(BitmapSource bms, int frameNum)
		{
			Bitmap bm = bitmapfromSource(bms);
			for (int i = 0; i < src.Width; i++)
			{
				for (int j = 0; j < src.Height; j++)
				{
					int[] srcCoords = getFrameCoords(SrcLines, i, j, frameNum);
					int[] destCoords = getFrameCoords(DestLines, i, j, NumFrames + 1 - frameNum);
					Color srcColor = src.GetPixel(srcCoords[0], srcCoords[1]);
					Color destColor = dest.GetPixel(destCoords[0], destCoords[1]);
					int red = (int)(srcColor.R * ((NumFrames + 1.0 - frameNum) / (NumFrames + 1.0))
						+ destColor.R * ((frameNum) / (NumFrames + 1.0))); //0.5 cause only one frame
					int green = (int)(srcColor.G * ((NumFrames + 1.0 - frameNum) / (NumFrames + 1.0))
						+ destColor.G * ((frameNum) / (NumFrames + 1.0))); //0.5 cause only one frame
					int blue = (int)(srcColor.B * ((NumFrames + 1.0 - frameNum) / (NumFrames + 1.0))
						+ destColor.B * ((frameNum) / (NumFrames + 1.0))); //0.5 cause only one frame
					int alpha = 255; //0.5 cause only one frame
					bm.SetPixel(i, j, Color.FromArgb(alpha, red, green, blue));
				}
			}
			return bm;
		}

		private int[] getFrameCoords(List<ControlLine> lines, int i, int j, int frameNum)
		{

			double totalweights = 0;
			double avgXDelta = 0;
			double avgYDelta = 0;
			for (int k = 0; k < SrcLines.Count; k++)
			{
				ControlLine destLine = makeDestLine(lines[k], frameNum); // 1 is frame number
				double dist = ControlLine.distance(destLine, i, j);
				double fractLen = ControlLine.fracLength(destLine, i, j);
				int[] coords = createNewCoords(lines[k], dist, fractLen);
				double weightDist =  getWeightDist2(destLine, i, j, fractLen, dist);
				double weight = Math.Pow(1 / (A_VALUE + Math.Abs(weightDist)), B_VALUE);
				totalweights += weight;
				avgXDelta += weight * (coords[0] - i);
				avgYDelta += weight * (coords[1] - j);
			}
			int[] frameCoords = new int[2];
			frameCoords[0] = i + (int)(avgXDelta / totalweights);
			frameCoords[1] = j + (int)(avgYDelta / totalweights);
			return frameCoords;
		}

		private double getWeightDist(ControlLine line, int i, int j, double dist)
		{
			if (line.NormX != 0) {
				double slope = line.NormY / line.NormX;
				double startIntersect = line.StartPixelY - slope * line.StartPixelX;
				double endIntersect = line.EndPixelY - slope * line.EndPixelX;
				double startPX = (j - startIntersect) / slope;
				double startPY = i * slope + startIntersect;
				double endPX = (j - endIntersect) / slope;
				double endPY = i * slope + endIntersect;
				double highX;
				double lowX;
				if (startPX > endPX)
				{
					highX = startPX;
					lowX = endPX;
				} else
				{
					highX = endPX;
					lowX = startPX;
				}
				double highY;
				double lowY;
				if (startPY > endPY)
				{
					highY = startPY;
					lowY = endPY;
				}
				else
				{
					highY = endPY;
					lowY = startPY;
				}
				if ((i >= lowX && i <= highX) || (j >= lowY && j <= highY)) {
					return dist;
				} else
				{
					double startDist = (i - line.StartPixelX) * (i - line.StartPixelX)
						+ (i - line.StartPixelX) * (i - line.StartPixelX);
					double endDist = (i - line.StartPixelX) * (i - line.StartPixelX)
						+ (i - line.StartPixelX) * (i - line.StartPixelX);
					return Math.Min(startDist, endDist);
				}
			}
			else
			{
				double highX;
				double lowX;
				if (line.StartPixelX > line.EndPixelX)
				{
					highX = line.StartPixelX;
					lowX = line.EndPixelX;
				}
				else
				{
					highX = line.EndPixelX;
					lowX = line.StartPixelX;
				}
				if (i >= lowX && i <= highX)
				{
					return dist;
				}
				else
				{
					double startDist = (i - line.StartPixelX) * (i - line.StartPixelX)
						+ (i - line.StartPixelX) * (i - line.StartPixelX);
					double endDist = (i - line.StartPixelX) * (i - line.StartPixelX)
						+ (i - line.StartPixelX) * (i - line.StartPixelX);
					return Math.Min(startDist, endDist);
				}
			}

		}
		
		public double getWeightDist2(ControlLine line, int i, int j, double fracLen, double dist)
		{
			if (fracLen >=0 && fracLen <= 1)
			{
				return dist;
			} else if (fracLen < 0)
			{
				return ControlLine.startDistance(line, i, j);
			} else
			{
				return ControlLine.endDistance(line, i, j);
			}
		}

		public void convertToSource(Bitmap bm)
		{
				string path = "c:\\Users\\Mike\\Downloads\\" + UUID + "_" + index + ".png";
				index++;
				bm.Save(path, ImageFormat.Png);
				Frames.Add(new BitmapImage(new Uri(path)));
		}

		public void setFrames(BitmapSource bms)
		{
			for (int i = 1; i <= NumFrames; i++)
			{
				Bitmap frame = createFrame(bms, i);
				convertToSource(frame);
			}
			Morphed = true;
		}

		public void getFrame(BitmapSource bms)
		{
			Bitmap frame = createFrame(bms, 1);
			convertToSource(frame);
		}

	}
}
