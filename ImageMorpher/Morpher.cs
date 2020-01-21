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
using Point = System.Windows.Point;

namespace ImageMorpher
{

	public class Morpher
	{
		Bitmap src;
		Bitmap dest;
		public List<BitmapSource> Frames { set; get; }
		public List<ControlLine> SrcLines { get; set; }
		public List<ControlLine> DestLines { get; set; }
		public static double A_VALUE = 0.01;
		public static double B_VALUE = 2;
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
			double xStartChange = pair.StartPixel.X - cl.StartPixel.X;
			double yStartChange = pair.StartPixel.Y - cl.StartPixel.Y;
			double xEndChange = pair.EndPixel.X - cl.EndPixel.X;
			double yEndChange = pair.EndPixel.Y - cl.EndPixel.Y;
			double xStartfactor = frameNum * xStartChange / (NumFrames + 1.0);
			double yStartfactor = frameNum * yStartChange / (NumFrames + 1.0);
			double xEndfactor = frameNum * xEndChange / (NumFrames + 1.0);
			double yEndfactor = frameNum * yEndChange / (NumFrames + 1.0);
			int startPX = (int)(cl.StartPixel.X + xStartfactor);
			int startPY = (int)(cl.StartPixel.Y + yStartfactor);
			int endPX = (int)(cl.EndPixel.X + xEndfactor);
			int endPY = (int)(cl.EndPixel.Y + yEndfactor);
			return new ControlLine(new Point(startPX, startPY), new Point(endPX, endPY));
		}

		private Point createNewCoords(ControlLine dest, double distance, double fracLen)
		{
			Point coords = dest.StartPixel + fracLen * dest.Vect - distance * (dest.Norm/ dest.Norm.Length);
			return coords;
		}

		public Bitmap createFrame(BitmapSource bms, int frameNum)
		{
			Bitmap bm = bitmapfromSource(bms);
			for (int i = 0; i < src.Width; i++)
			{
				for (int j = 0; j < src.Height; j++)
				{
					Point p = new Point(i, j);
					Point srcCoords = getFrameCoords(SrcLines, p, frameNum);
					Point destCoords = getFrameCoords(DestLines, p, NumFrames + 1 - frameNum);
					Color srcColor = src.GetPixel((int)srcCoords.X, (int)srcCoords.Y);
					Color destColor = dest.GetPixel((int)destCoords.X, (int)destCoords.Y);
					int red = (int)(srcColor.R * ((NumFrames + 1.0 - frameNum) / (NumFrames + 1.0))
						+ destColor.R * ((frameNum) / (NumFrames + 1.0))); 
					int green = (int)(srcColor.G * ((NumFrames + 1.0 - frameNum) / (NumFrames + 1.0))
						+ destColor.G * ((frameNum) / (NumFrames + 1.0))); 
					int blue = (int)(srcColor.B * ((NumFrames + 1.0 - frameNum) / (NumFrames + 1.0))
						+ destColor.B * ((frameNum) / (NumFrames + 1.0))); 
					int alpha = 255;
					bm.SetPixel(i, j, Color.FromArgb(alpha, red, green, blue));
				}
			}
			return bm;
		}

		private Point getFrameCoords(List<ControlLine> lines, Point p, int frameNum)
		{
			double totalweights = 0;
			Vector avgDelta = new Vector(0, 0);
			for (int k = 0; k < SrcLines.Count; k++)
			{
				ControlLine destLine = makeDestLine(lines[k], frameNum);
				double dist = destLine.distance(p);
				double fractLen = destLine.fracLength(p);
				Point coords = createNewCoords(lines[k], dist, fractLen);
				double weightDist =  getWeightDist(destLine, p, fractLen, dist);
				double weight = Math.Pow(1 / (A_VALUE + weightDist), B_VALUE);
				totalweights += weight;
				avgDelta += (weight * (coords - p));
			}
			 return ensurePixel(p + avgDelta / totalweights);
		}

		private Point ensurePixel(Point p)
		{
			if (p.X < 0)
			{
				p.X = 0;
			} else if (p.X >= src.Width)
			{
				p.X = src.Width - 1;
			}
			if (p.Y < 0)
			{
				p.Y = 0;
			}
			else if (p.Y >= src.Height)
			{
				p.Y = src.Height - 1;
			}
			return p;
		}
		
		private double getWeightDist(ControlLine line, Point p, double fracLen, double dist)
		{
			if (fracLen >=0 && fracLen <= 1)
			{
				return Math.Abs(dist);
			} else if (fracLen < 0)
			{
				return line.startDistance(p);
			} else
			{
				return line.endDistance(p);
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
