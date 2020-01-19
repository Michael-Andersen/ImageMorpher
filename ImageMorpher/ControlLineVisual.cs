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
	public class ControlLineVisual : ControlLine
	{
		[NonSerialized]
		public ImageViewer viewer;

		[NonSerialized]
		private Line drawnLine;

		[NonSerialized]
		private Ellipse drawnStart;

		[NonSerialized]
		private Ellipse drawnEnd;

		[NonSerialized]
		private Ellipse drawnMiddle;

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


		public enum Colour
		{
			Red, Green, Blue, Orange, Black, White, Brown, Purple, Gray
		}

		static ControlLineVisual()
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



		public ControlLineVisual(ImageViewer iv, ControlPoint o)
		{
			initVisuals();
			setThickness();
			setColours();
			viewer = iv;
			addToCanvas(viewer.canvas);
			Point pixelPoint = iv.GridToPixel(o.Point);
			drawStart(o);
			setStart(viewer.GridToPixel(o.Point));
		}

		private void initVisuals()
		{
			drawnLine = new Line();
			drawnStart = new Ellipse();
			drawnEnd = new Ellipse();
			drawnMiddle = new Ellipse();
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

		private void setColours()
		{
			drawnStart.Fill = colourDict[START_COLOUR];
			drawnLine.Fill = colourDict[LINE_COLOUR];
			drawnLine.Stroke = colourDict[LINE_COLOUR];
			drawnEnd.Fill = colourDict[END_COLOUR];
			drawnMiddle.Fill = colourDict[MIDDLE_COLOUR];
		}

		private void addToCanvas(Canvas canvas)
		{
			canvas.Children.Add(drawnLine);
			canvas.Children.Add(drawnStart);
			canvas.Children.Add(drawnEnd);
			canvas.Children.Add(drawnMiddle);
		}

		public void drawLine(ImageViewer iv)
		{
			initVisuals();
			setThickness();
			setColours();
			viewer = iv;
			addToCanvas(viewer.canvas);
			drawStart(Start);
			drawEnd(End);
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

		public void removeFromCanvas()
		{
			viewer.canvas.Children.Remove(drawnLine);
			viewer.canvas.Children.Remove(drawnStart);
			viewer.canvas.Children.Remove(drawnMiddle);
			viewer.canvas.Children.Remove(drawnEnd);
			viewer.canvas.UpdateLayout();
		}

		public void drawStart(ControlPoint o)
		{
			Start = o;
			drawnLine.X1 = o.Point.X;
			drawnLine.Y1 = o.Point.Y;
			Canvas.SetLeft(drawnStart, o.Point.X - 2.5);
			Canvas.SetTop(drawnStart, o.Point.Y - 2.5);
		}

		public void drawEnd(ControlPoint o, bool newMiddle = true)
		{
			End = o;
			Canvas.SetLeft(drawnEnd, o.Point.X - 2.5);
			Canvas.SetTop(drawnEnd, o.Point.Y - 2.5);
			drawMiddle(newMiddle);
			drawnLine.X2 = o.Point.X;
			drawnLine.Y2 = o.Point.Y;
		}

		public void drawMiddle(bool newMiddle = true)
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
			Point endPoint = new Point(End.Point.X + xtranslation, End.Point.Y + ytranslation);
			drawStart(new ControlPoint(startPoint));
			drawEnd(new ControlPoint(endPoint), false);
			middleTranslation(viewer.GridToPixel(startPoint), viewer.GridToPixel(endPoint));
		}
	}
}
