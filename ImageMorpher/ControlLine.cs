using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ImageMorpher
{
	
	[Serializable]
    public class ControlLine
	{
		[NonSerialized]
		private Line drawnLine;
		[NonSerialized]
		private Ellipse drawnStart;
		[NonSerialized]
		private Ellipse drawnEnd;
		[NonSerialized]
		private Ellipse drawnMiddle;


		public enum Colour
		{
			Red, Green, Blue, Orange, Black, White, Brown, Purple, Gray
		}

		public static double DIAMETER = 5;
		public static double LINE_THICKNESS = 1;
		public static Colour START_COLOUR = Colour.Green;
		public static Colour MIDDLE_COLOUR = Colour.Blue;
		public static Colour END_COLOUR = Colour.Red;
		public static Colour HIGHLIGHT_COLOUR = Colour.Purple;
		public static Colour LINE_COLOUR = Colour.Black;
		private static Dictionary<Colour, SolidColorBrush> colourDict
			= new Dictionary<Colour, SolidColorBrush>();

		public ControlPoint Start { get; set; }
		public ControlPoint End { get; set; }
		public ControlPoint Middle { get; set; }

		public bool IsSrc { get; set; }
		public ControlLine Pair { get; set; }
		public double Length { get; set; }
		public double LengthSq { get; set; }
		public double VectX { get; set; }
		public double VectY { get; set; }
		public double NormX { get; set; }
		public double NormY { get; set; }
		public double NormLength { get; set; }
		public double StartPixelX { get; set; }
		public double StartPixelY { get; set; }
		public double EndPixelX { get; set; }
		public double EndPixelY { get; set; }

	static ControlLine()
		{
			colourDict.Add(Colour.Red, Brushes.Red);
			colourDict.Add(Colour.Green, Brushes.Green);
			colourDict.Add(Colour.Gray, Brushes.Gray);
			colourDict.Add(Colour.Blue, Brushes.Blue);
			colourDict.Add(Colour.Black, Brushes.Black);
			colourDict.Add(Colour.Brown, Brushes.Brown);
			colourDict.Add(Colour.White, Brushes.White);
			colourDict.Add(Colour.Orange, Brushes.Orange);
			colourDict.Add(Colour.Purple, Brushes.Purple);
		}

		public static double dotProduct(ControlLine a, double X, double Y)
		{
			return a.VectX * X + a.VectY * Y;
		}

		public static double dotProduct(ControlLine a, ControlLine b)
		{
			return a.VectX * b.VectX + a.VectY * b.VectY;
		}

		public static double proj(ControlLine vector, double X, double Y)
		{
			return dotProduct(vector, X, Y) / Math.Pow(X * X + Y * Y, 0.5);
		}

		public static double proj(ControlLine vector, ControlLine on)
		{
			return dotProduct(vector, on) / on.Length;
		}

		public static double calcLenSq(ControlLine vector)
		{
			return vector.VectX * vector.VectX + vector.VectY * vector.VectY;
		}

		public static double calcLen(ControlLine vector)
		{
			return Math.Pow(calcLenSq(vector), 0.5);
		}

		public static ControlLine xP(ControlLine vector, double x, double y)
		{
			ControlLine xP = new ControlLine();
			xP.VectX = vector.StartPixelX - x;
			xP.VectY = vector.StartPixelY - y;
			return xP;
		}

		public static ControlLine pX(ControlLine vector, double x, double y)
		{
			ControlLine pX = new ControlLine();
			pX.VectX = -1 * (vector.StartPixelX - x);
			pX.VectY = -1 * (vector.StartPixelY - y);
			return pX;
		}

		public static double distance(ControlLine vector, double x, double y)
		{
			return proj(xP(vector, x, y), vector.NormX, vector.NormY);
		}

		public static double fracLength(ControlLine vector, double x, double y)
		{
			return dotProduct(vector, pX(vector, x, y)) / vector.LengthSq;
		}

		public ControlLine(Canvas canvas, ControlPoint o, double pixelX, double pixelY)
		{
			initVisuals();
			setThickness();
			setColours();
			addToCanvas(canvas);
			setStart(o, pixelX, pixelY);
			setEnd(o, pixelX, pixelY);
		}

		public ControlLine()
		{

		}


		public ControlLine(double startPX, double startPY, double endPX, double endPY)
		{
			StartPixelX = startPX;
			StartPixelY = startPY;
			EndPixelX = endPX;
			EndPixelY = endPY;
			setVector();
		}

		private void addToCanvas(Canvas canvas)
		{
			canvas.Children.Add(drawnLine);
			canvas.Children.Add(drawnStart);
			canvas.Children.Add(drawnEnd);
			canvas.Children.Add(drawnMiddle);
		}
		private void setColours()
		{
			drawnStart.Fill = colourDict[START_COLOUR];
			drawnLine.Fill = colourDict[LINE_COLOUR];
			drawnLine.Stroke = colourDict[LINE_COLOUR];
			drawnEnd.Fill = colourDict[END_COLOUR];
			drawnMiddle.Fill = colourDict[MIDDLE_COLOUR];
		}

		private void setThickness()
		{
			drawnStart.Width = DIAMETER;
			drawnStart.Height = DIAMETER;
			drawnLine.StrokeThickness = LINE_THICKNESS;
			drawnEnd.Width = DIAMETER;
			drawnEnd.Height = DIAMETER;
			drawnMiddle.Width = DIAMETER;
			drawnMiddle.Height = DIAMETER;
		}
		private void initVisuals()
		{
			drawnLine = new Line();
			drawnStart = new Ellipse();
			drawnEnd = new Ellipse();
			drawnMiddle = new Ellipse();
		}
		public void drawLine(Canvas canvas)
		{
			initVisuals();
			setThickness();
			setColours();
			addToCanvas(canvas);
			setStart(Start);
			setEnd(End, -1, -1);
		}

		public void highlight()
		{
			drawnLine.Fill = colourDict[HIGHLIGHT_COLOUR];
			drawnLine.Stroke = colourDict[HIGHLIGHT_COLOUR];
			drawnMiddle.Fill = colourDict[HIGHLIGHT_COLOUR];
		}

		public void deHighlight()
		{
			drawnLine.Fill = colourDict[LINE_COLOUR];
			drawnLine.Stroke = colourDict[LINE_COLOUR];
			drawnMiddle.Fill = colourDict[MIDDLE_COLOUR];
		}

		public void removeFromCanvas(Canvas canvas)
		{
			canvas.Children.Remove(drawnLine);
			canvas.Children.Remove(drawnStart);
			canvas.Children.Remove(drawnMiddle);
			canvas.Children.Remove(drawnEnd);
			canvas.UpdateLayout();
		}

		public void setStart(ControlPoint o, double pixelX = -1, double pixelY = -1)
		{
			Start = o;
			drawnLine.X1 = o.Point.X;
			drawnLine.Y1 = o.Point.Y;
			Canvas.SetLeft(drawnStart, o.Point.X - 2.5);
			Canvas.SetTop(drawnStart, o.Point.Y - 2.5);
			if (pixelX >= 0)
			{ 
				StartPixelX = pixelX;
				StartPixelY = pixelY;
				setVector();
			}
		}

		public void setEnd(ControlPoint o, double xPixel, double yPixel, bool newMiddle=true)
		{
			End = o;
			Canvas.SetLeft(drawnEnd, o.Point.X - 2.5);
			Canvas.SetTop(drawnEnd, o.Point.Y - 2.5);
			setMiddle(newMiddle);
			drawnLine.X2 = o.Point.X;
			drawnLine.Y2 = o.Point.Y;
			if (xPixel >= 0)
			{
				EndPixelX = xPixel;
				EndPixelY = yPixel;
				setVector();
			}
		}
		private void setVector()
		{
			VectX = EndPixelX - StartPixelX;
			VectY = EndPixelY - StartPixelY;
			LengthSq = calcLenSq(this);
			Length = calcLen(this);
			NormX = -1 * VectY;
			NormY = VectX;
			NormLength = Math.Pow(NormX * NormX + NormY * NormY, 0.5);
		}
		public void setMiddle(bool newMiddle=true)
		{
			if (newMiddle)
			{
				Point middlePoint = new Point();
				middlePoint.X = (Start.Point.X + End.Point.X) / 2;
				middlePoint.Y = (Start.Point.Y + End.Point.Y) / 2;
				Middle = new ControlPoint(middlePoint);
			}
			Canvas.SetLeft(drawnMiddle, Middle.Point.X - 2.5);
			Canvas.SetTop(drawnMiddle, Middle.Point.Y - 2.5);
		}

		public void moveMiddle(ControlPoint o)
		{
			double xtranslation = o.Point.X - Middle.Point.X;
			double ytranslation = o.Point.Y - Middle.Point.Y;
			Middle = o;
			Point startPoint = new Point(Start.Point.X + xtranslation, Start.Point.Y + ytranslation);
			setStart(new ControlPoint(startPoint), StartPixelX + xtranslation, StartPixelY + ytranslation);
			Point endPoint = new Point(End.Point.X + xtranslation, End.Point.Y + ytranslation);
			setEnd(new ControlPoint(endPoint), EndPixelX + xtranslation, EndPixelY + ytranslation, false);
		}
	}
}
