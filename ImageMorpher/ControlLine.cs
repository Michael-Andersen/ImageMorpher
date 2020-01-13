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
		private ControlPoint start;
		private ControlPoint end;
		private ControlPoint middle;

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

		public ControlPoint Start { get => start; set => start = value; }
		public ControlPoint End { get => end; set => end = value; }
		public ControlPoint Middle { get => middle; set => middle = value; }

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

		public ControlLine(Canvas canvas, ControlPoint o)
		{
			initVisuals();
			setThickness();
			setColours();
			addToCanvas(canvas);
			setStart(o);
			setEnd(o);
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
			setStart(start);
			setEnd(end);
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

		public void setStart(ControlPoint o)
		{
			start = o;
			drawnLine.X1 = o.Point.X;
			drawnLine.Y1 = o.Point.Y;
			Canvas.SetLeft(drawnStart, o.Point.X - 2.5);
			Canvas.SetTop(drawnStart, o.Point.Y - 2.5);
		}

		public void setEnd(ControlPoint o, bool newMiddle=true)
		{
			end = o;
			Canvas.SetLeft(drawnEnd, o.Point.X - 2.5);
			Canvas.SetTop(drawnEnd, o.Point.Y - 2.5);
			setMiddle(newMiddle);
			drawnLine.X2 = o.Point.X;
			drawnLine.Y2 = o.Point.Y;
		}

		public void setMiddle(bool newMiddle=true)
		{
			if (newMiddle)
			{
				Point middlePoint = new Point();
				middlePoint.X = (start.Point.X + end.Point.X) / 2;
				middlePoint.Y = (start.Point.Y + end.Point.Y) / 2;
				middle = new ControlPoint(middlePoint);
			}
			Canvas.SetLeft(drawnMiddle, middle.Point.X - 2.5);
			Canvas.SetTop(drawnMiddle, middle.Point.Y - 2.5);
		}

		public void moveMiddle(ControlPoint o)
		{
			double xtranslation = o.Point.X - middle.Point.X;
			double ytranslation = o.Point.Y - middle.Point.Y;
			middle = o;
			Point startPoint = new Point(start.Point.X + xtranslation, start.Point.Y + ytranslation);
			setStart(new ControlPoint(startPoint));
			Point endPoint = new Point(end.Point.X + xtranslation, end.Point.Y + ytranslation);
			setEnd(new ControlPoint(endPoint), false);
		}
	}
}
