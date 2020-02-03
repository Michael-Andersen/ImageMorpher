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
using System.Threading;
using System.Runtime.InteropServices;

namespace ImageMorpher
{

	public class Morpher
	{
		[DllImport("AssemblyMorphing.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void Free_Pointer(IntPtr ptr);
		[DllImport("AssemblyMorphing.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void SetSize(int ht, int sd, int lsz);
		[DllImport("AssemblyMorphing.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr CreateFrame(IntPtr src, IntPtr dest, IntPtr lines, int frameNum, int numFrames);

		Bitmap src;
		Bitmap dest;
		public List<ControlLine> SrcLines { get; set; }
		public List<ControlLine> DestLines { get; set; }
		public static double A_VALUE = 0.01;
		public static double B_VALUE = 2;
		public static string PROJECT_NAME;
		public static string PROJECT_PATH;
		public static int NumThreads = 4;
		public static bool SSE = true;
		public Morph Mrph { get; set; }
		public bool Morphed = false;
		public string UUID;
		public int index = 0;
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
			int startPX = (int)Math.Round(cl.StartPixel.X + xStartfactor);
			int startPY = (int)Math.Round(cl.StartPixel.Y + yStartfactor);
			int endPX = (int)Math.Round(cl.EndPixel.X + xEndfactor);
			int endPY = (int)Math.Round(cl.EndPixel.Y + yEndfactor);
			return new ControlLine(new Point(startPX, startPY), new Point(endPX, endPY));
		}

		private Point createNewCoords(ControlLine dest, double distance, double fracLen)
		{
			Point coords = dest.StartPixel + fracLen * dest.Vect - distance * (dest.Norm/ dest.Norm.Length);
			return coords;
		}

		public Bitmap createFrame(int frameNum, Bitmap srcC, Bitmap destC)
		{
			Bitmap bm = new Bitmap(srcC.Width, srcC.Height);
			for (int i = 0; i < srcC.Width; i++)
			{
				for (int j = 0; j < srcC.Height; j++)
				{
					Point p = new Point(i, j);
					Point srcCoords = getFrameCoords(SrcLines, p, frameNum, srcC);
					Point destCoords = getFrameCoords(DestLines, p, NumFrames + 1 - frameNum, srcC);
					Color srcColor = srcC.GetPixel((int)Math.Round(srcCoords.X), (int)Math.Round(srcCoords.Y));
					Color destColor = destC.GetPixel((int)Math.Round(destCoords.X), (int)Math.Round(destCoords.Y));
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

		private Point getFrameCoords(List<ControlLine> lines, Point p, int frameNum, Bitmap srcC)
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
			 return ensurePixel(p + avgDelta / totalweights, srcC);
		}

		private Point ensurePixel(Point p, Bitmap srcC)
		{
			if (p.X < 0)
			{
				p.X = 0;
			} else if (p.X >= srcC.Width - 1)
			{
				p.X = srcC.Width - 1;
			}
			if (p.Y < 0)
			{
				p.Y = 0;
			}
			else if (p.Y >= srcC.Height - 1)
			{
				p.Y = srcC.Height - 1;
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

		public void convertToSource(Bitmap bm, int frameNum)
		{
				string path = PROJECT_PATH + PROJECT_NAME + "\\" + Mrph.MorphName + "_" + (frameNum - 1) + ".png";
				bm.Save(path, ImageFormat.Png);
				BitmapImage bmi = LoadImage(path);
				bmi.Freeze();
				Mrph.Frames[frameNum - 1] = bmi;
		}

		public void startThreads()
		{
			Mrph.Frames = new List<BitmapSource>(new BitmapSource[NumFrames]);
			Thread[] threadArr = new Thread[NumThreads];
			int NperThread = NumFrames / NumThreads;
			for (int i = 0; i < NumThreads; i++)
			{
				int start = NperThread * i;
				int amount = NperThread;
				if (i == (NumThreads - 1))
				{
					amount += (NumFrames - NperThread * NumThreads);

				}
				Bitmap srcC = new Bitmap(src);
				Bitmap destC = new Bitmap(dest);
				if (SSE)
				{
					threadArr[i] = new Thread(() => setFrames_SSE(start, amount, srcC, destC)); 
				} else
				{
					threadArr[i] = new Thread(() => setFrames(start, amount, srcC, destC));
				}
				}
			for (int i = 0; i < NumThreads; i++)
			{
				threadArr[i].Start();
			}
			for (int i = 0; i < threadArr.Length; i++)
			{
				threadArr[i].Join();
			}
			Morphed = true;
		}

		public static BitmapImage LoadImage(string filename)
		{
			BitmapImage myRetVal = null;
			if (filename != null)
			{
				BitmapImage image = new BitmapImage();
				try
				{
					using (FileStream stream = File.OpenRead(filename))
					{
						image.BeginInit();
						image.CacheOption = BitmapCacheOption.OnLoad;
						image.StreamSource = stream;
						image.EndInit();
					}
				} catch (FileNotFoundException e)
				{

				}
				myRetVal = image;
			}
			return myRetVal;
		}

		public void setFrames_SSE(int start, int amount, Bitmap srcC, Bitmap destC) {

			byte[] bytedataS;
			byte[] bytedataD;
			for (int i = start; i < start + amount; i++)
			{
				float[] lines = new float[40 + SrcLines.Count * 56];
				lines[0] = 0;
				lines[1] = 2;
				lines[2] = 0;
				lines[3] = 2;
				lines[4] = -1;
				lines[5] = -1 * (srcC.Width);
				lines[6] = -1;
				lines[7] = -1 * (srcC.Width);
				lines[8] = (float)A_VALUE;
				lines[9] = (float)A_VALUE;
				lines[10] = (float)A_VALUE;
				lines[11] = (float)A_VALUE;
				lines[12] = 0;
				lines[13] = 0;
				lines[14] = 0;
				lines[15] = 0;
				lines[16] = 0;
				lines[17] = 0;
				lines[18] = 0;
				lines[19] = 0;
				lines[20] = srcC.Height - 1;
				lines[21] = srcC.Width - 1;
				lines[22] = srcC.Height - 1;
				lines[23] = srcC.Width - 1;
				lines[24] = 4;
				lines[25] = 4;
				lines[26] = 4;
				lines[27] = 4;
				lines[28] = srcC.Width;
				lines[29] = srcC.Width;
				lines[30] = srcC.Width;
				lines[31] = srcC.Width;
				lines[32] = 1;
				lines[33] = 1;
				lines[34] = 1;
				lines[35] = 1;
				lines[36] = 0;
				lines[37] = -1;
				lines[38] = 0;
				lines[39] = -1;
				int z = 0;
				SetSize(srcC.Height, srcC.Width * 4, lines.Length);
				int CONST_AMT = 40;
				for (int k = 0; k < SrcLines.Count; k++ )
				{
					ControlLine l = SrcLines[k];
					lines[CONST_AMT + z] = (float)l.StartPixel.Y;
					z++;
					lines[CONST_AMT + z] = (float)l.StartPixel.X;
					z++;
					lines[CONST_AMT + z] = (float)l.StartPixel.Y;
					z++;
					lines[CONST_AMT + z] = (float)l.StartPixel.X;
					z++;
					lines[CONST_AMT + z] = (float)l.EndPixel.Y;
					z++;
					lines[CONST_AMT + z] = (float)l.EndPixel.X;
					z++;
					lines[CONST_AMT + z] = (float)l.EndPixel.Y;
					z++;
					lines[CONST_AMT + z] = (float)l.EndPixel.X;
					z++;
					lines[CONST_AMT + z] = (float)l.Vect.Y;
					z++;
					lines[CONST_AMT + z] = (float)l.Vect.X;
					z++;
					lines[CONST_AMT + z] = (float)l.Vect.Y;
					z++;
					lines[CONST_AMT + z] = (float)l.Vect.X;
					z++;
					lines[CONST_AMT + z] = (float)(  l.Norm.Y/l.Norm.Length);
					z++;
					lines[CONST_AMT + z] = (float)( l.Norm.X / l.Norm.Length);
					z++;
					lines[CONST_AMT + z] = (float)( l.Norm.Y / l.Norm.Length);
					z++;
					lines[CONST_AMT + z] = (float)(l.Norm.X / l.Norm.Length);
					z++;
					l = makeDestLine(l, i + 1);
					lines[CONST_AMT + z] = (float)l.StartPixel.Y;
					z++;
					lines[CONST_AMT + z] = (float)l.StartPixel.X;
					z++;
					lines[CONST_AMT + z] = (float)l.StartPixel.Y;
					z++;
					lines[CONST_AMT + z] = (float)l.StartPixel.X;
					z++;
					lines[CONST_AMT + z] = (float)l.EndPixel.Y;
					z++;
					lines[CONST_AMT + z] = (float)l.EndPixel.X;
					z++;
					lines[CONST_AMT + z] = (float)l.EndPixel.Y;
					z++;
					lines[CONST_AMT + z] = (float)l.EndPixel.X;
					z++;
					lines[CONST_AMT + z] = (float)l.Vect.Y;
					z++;
					lines[CONST_AMT + z] = (float)l.Vect.X;
					z++;
					lines[CONST_AMT + z] = (float)l.Vect.Y;
					z++;
					lines[CONST_AMT + z] = (float)l.Vect.X;
					z++;
					lines[CONST_AMT + z] = (float)(l.Norm.Y);
					z++;
					lines[CONST_AMT + z] = (float)(l.Norm.X);
					z++;
					lines[CONST_AMT + z] = (float)( l.Norm.Y);
					z++;
					lines[CONST_AMT + z] = (float)(l.Norm.X);
					z++;
					lines[CONST_AMT + z] = (float)l.Vect.Length;
					z++;
					lines[CONST_AMT + z] = (float)l.Vect.Length;
					z++;
					lines[CONST_AMT + z] = (float)l.Vect.Length;
					z++;
					lines[CONST_AMT + z] = (float)l.Vect.Length;
					z++;
					lines[CONST_AMT + z] = (float)l.Vect.LengthSquared;
					z++;
					lines[CONST_AMT + z] = (float)l.Vect.LengthSquared;
					z++;
					lines[CONST_AMT + z] = (float)l.Vect.LengthSquared;
					z++;
					lines[CONST_AMT + z] = (float)l.Vect.LengthSquared;
					z++;
					l = SrcLines[k].Pair;
					lines[CONST_AMT + z] = (float)l.StartPixel.Y;
					z++;
					lines[CONST_AMT + z] = (float)l.StartPixel.X;
					z++;
					lines[CONST_AMT + z] = (float)l.StartPixel.Y;
					z++;
					lines[CONST_AMT + z] = (float)l.StartPixel.X;
					z++;
					lines[CONST_AMT + z] = (float)l.EndPixel.Y;
					z++;
					lines[CONST_AMT + z] = (float)l.EndPixel.X;
					z++;
					lines[CONST_AMT + z] = (float)l.EndPixel.Y;
					z++;
					lines[CONST_AMT + z] = (float)l.EndPixel.X;
					z++;
					lines[CONST_AMT + z] = (float)l.Vect.Y;
					z++;
					lines[CONST_AMT + z] = (float)l.Vect.X;
					z++;
					lines[CONST_AMT + z] = (float)l.Vect.Y;
					z++;
					lines[CONST_AMT + z] = (float)l.Vect.X;
					z++;
					lines[CONST_AMT + z] = (float)(l.Norm.Y / l.Norm.Length);
					z++;
					lines[CONST_AMT + z] = (float)(l.Norm.X / l.Norm.Length);
					z++;
					lines[CONST_AMT + z] = (float)(l.Norm.Y / l.Norm.Length);
					z++;
					lines[CONST_AMT + z] = (float)(l.Norm.X / l.Norm.Length);
					z++;
				}
				IntPtr linesPtr = Marshal.AllocHGlobal(Marshal.SizeOf(lines[0]) * lines.Length);
				Marshal.Copy(lines, 0, linesPtr, lines.Length);
				BitmapData bmpdataS = null;
				BitmapData bmpdataD = null;
				IntPtr Sptr;
				IntPtr Dptr;
				try
				{
					bmpdataS = srcC.LockBits(new Rectangle(0, 0, srcC.Width, srcC.Height), ImageLockMode.ReadOnly, srcC.PixelFormat);
					bmpdataD = destC.LockBits(new Rectangle(0, 0, srcC.Width, srcC.Height), ImageLockMode.ReadOnly, destC.PixelFormat);
					int numbytes = bmpdataS.Stride * srcC.Height;
					bytedataS = new byte[numbytes];
					bytedataD = new byte[numbytes];
					Sptr = bmpdataS.Scan0;
					Dptr = bmpdataD.Scan0;
					Marshal.Copy(Sptr, bytedataS, 0, numbytes);
					Marshal.Copy(Dptr, bytedataD, 0, numbytes);
					Sptr = Marshal.AllocHGlobal(numbytes * Marshal.SizeOf(bytedataS[0]));
					Dptr = Marshal.AllocHGlobal(numbytes * Marshal.SizeOf(bytedataD[0]));
					Marshal.Copy(bytedataS, 0, Sptr, numbytes);
					Marshal.Copy(bytedataD, 0, Dptr, numbytes);
				}
				finally
				{
					if (bmpdataS != null)
						srcC.UnlockBits(bmpdataS);
					if (bmpdataD != null)
						destC.UnlockBits(bmpdataD);
				}
				IntPtr frm = CreateFrame(Sptr, Dptr, linesPtr, i+1, NumFrames);
				Bitmap frame = new Bitmap(srcC.Width, srcC.Height, (srcC.Width * 4), srcC.PixelFormat, frm);
				convertToSource(frame, i + 1);
				Free_Pointer(frm);
				Marshal.FreeHGlobal(Sptr);
				Marshal.FreeHGlobal(Dptr);
				Marshal.FreeHGlobal(linesPtr);
			}
		}

		public void setFrames(int start, int amount, Bitmap srcC, Bitmap destC)
		{
			for (int i = start; i < start + amount; i++)
			{
				Bitmap frame = createFrame(i + 1, srcC, destC);
				convertToSource(frame, i + 1);
			}
		}
	}
}
