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

		private static double DIAMETER = 5;
		private static double LINE_THICKNESS = 1;
		private static SolidColorBrush START_BRUSH = Brushes.Green;
		private static SolidColorBrush MIDDLE_BRUSH = Brushes.Blue;
		private static SolidColorBrush END_BRUSH = Brushes.Red;
		private static SolidColorBrush HIGHLIGHT_BRUSH = Brushes.Purple;
		private static SolidColorBrush LINE_BRUSH = Brushes.Black;

		public ControlPoint Start { get => start; set => start = value; }
		public ControlPoint End { get => end; set => end = value; }
		public ControlPoint Middle { get => middle; set => middle = value; }

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
			drawnStart.Fill = START_BRUSH;
			drawnLine.Fill = LINE_BRUSH;
			drawnLine.Stroke = LINE_BRUSH;
			drawnEnd.Fill = END_BRUSH;
			drawnMiddle.Fill = MIDDLE_BRUSH;
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
			drawnLine.Fill = HIGHLIGHT_BRUSH;
			drawnLine.Stroke = HIGHLIGHT_BRUSH;
			drawnMiddle.Fill = HIGHLIGHT_BRUSH;
		}

		public void deHighlight()
		{
			drawnLine.Fill = LINE_BRUSH;
			drawnLine.Stroke = LINE_BRUSH;
			drawnMiddle.Fill = MIDDLE_BRUSH;
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
