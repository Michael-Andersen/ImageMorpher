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

		public ControlPoint Start { get => start; set => start = value; }
		public ControlPoint End { get => end; set => end = value; }
		public ControlPoint Middle { get => middle; set => middle = value; }

		public ControlLine(Canvas canvas, ControlPoint o)
		{	
			drawnLine = new Line();
			drawnStart = new Ellipse();
			drawnEnd = new Ellipse();
			drawnMiddle = new Ellipse();
			drawnStart.Width = 5;
			drawnStart.Height = 5;
			drawnStart.Fill = Brushes.Green;
			setStart(o);
			setEnd(o);
			drawnLine.Fill = Brushes.Black;
			drawnLine.Stroke = Brushes.Black;
			drawnLine.StrokeThickness = 1;
			drawnEnd.Width = 5;
			drawnEnd.Height = 5;
			drawnEnd.Fill = Brushes.Red;
			drawnMiddle.Width = 5;
			drawnMiddle.Height = 5;
			drawnMiddle.Fill = Brushes.Blue;
			canvas.Children.Add(drawnLine);
			canvas.Children.Add(drawnStart);
			canvas.Children.Add(drawnEnd);
			canvas.Children.Add(drawnMiddle);

		}

		public void drawLine(Canvas canvas)
		{
			drawnLine = new Line();
			drawnLine.Fill = Brushes.Black;
			drawnLine.Stroke = Brushes.Black;
			drawnLine.StrokeThickness = 1;
			drawnLine.X1 = start.Point.X;
			drawnLine.Y1 = start.Point.Y;
			drawnStart = new Ellipse();
			drawnEnd = new Ellipse();
			drawnMiddle = new Ellipse();
			drawnStart.Width = 5;
			drawnStart.Height = 5;
			drawnStart.Fill = Brushes.Green;
			drawnEnd.Width = 5;
			drawnEnd.Height = 5;
			drawnEnd.Fill = Brushes.Red;
			drawnMiddle.Width = 5;
			drawnMiddle.Height = 5;
			drawnMiddle.Fill = Brushes.Blue;
			Canvas.SetLeft(drawnStart, start.Point.X - 2.5);
			Canvas.SetTop(drawnStart, start.Point.Y - 2.5);
			drawnLine.X2 = end.Point.X;
			drawnLine.Y2 = end.Point.Y;
			Canvas.SetLeft(drawnEnd, end.Point.X - 2.5);
			Canvas.SetTop(drawnEnd, end.Point.Y - 2.5);
			Canvas.SetLeft(drawnMiddle, middle.Point.X - 2.5);
			Canvas.SetTop(drawnMiddle, middle.Point.Y - 2.5);
			canvas.Children.Add(drawnLine);
			canvas.Children.Add(drawnStart);
			canvas.Children.Add(drawnEnd);
			canvas.Children.Add(drawnMiddle);
		}

		public void highlight()
		{
			drawnLine.Fill = Brushes.Purple;
			drawnLine.Stroke = Brushes.Purple;
			drawnMiddle.Fill = Brushes.Purple;
		}

		public void deHighlight()
		{
			drawnLine.Fill = Brushes.Black;
			drawnLine.Stroke = Brushes.Black;
			drawnMiddle.Fill = Brushes.Blue;
		}
		
		public void addToCanvas(Canvas canvas)
		{
			canvas.Children.Add(drawnLine);
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

		public void setEnd(ControlPoint o)
		{
			end = o;
			drawnEnd.Width = 5;
			drawnEnd.Height = 5;
			drawnEnd.Fill = Brushes.Red;
			Canvas.SetLeft(drawnEnd, o.Point.X - 2.5);
			Canvas.SetTop(drawnEnd, o.Point.Y - 2.5);
			setMiddle();
			drawnLine.X2 = o.Point.X;
			drawnLine.Y2 = o.Point.Y;
		}

		public void setMiddle()
		{
			drawnMiddle.Width = 5;
			drawnMiddle.Height = 5;
			drawnMiddle.Fill = Brushes.Blue;
			Point middlePoint = new Point();
			middlePoint.X = (start.Point.X + end.Point.X) / 2;
			middlePoint.Y = (start.Point.Y + end.Point.Y) / 2;
			middle = new ControlPoint(middlePoint);
			Canvas.SetLeft(drawnMiddle, middle.Point.X - 2.5);
			Canvas.SetTop(drawnMiddle, middle.Point.Y - 2.5);
		}

		public void moveMiddle(ControlPoint o)
		{
			double xtranslation = o.Point.X - middle.Point.X;
			double ytranslation = o.Point.Y - middle.Point.Y;
			middle = o;
			Point startPoint = new Point(start.Point.X + xtranslation, start.Point.Y + ytranslation);
			start = new ControlPoint(startPoint);
			Point endPoint = new Point(end.Point.X + xtranslation, end.Point.Y + ytranslation);
			end = new ControlPoint(endPoint);
			drawnLine.X1 = start.Point.X;
			drawnLine.Y1 = start.Point.Y;
			Canvas.SetLeft(drawnStart, start.Point.X - 2.5);
			Canvas.SetTop(drawnStart, start.Point.Y - 2.5);
			drawnLine.X2 = end.Point.X;
			drawnLine.Y2 = end.Point.Y;
			Canvas.SetLeft(drawnEnd, end.Point.X - 2.5);
			Canvas.SetTop(drawnEnd, end.Point.Y - 2.5);
			Canvas.SetLeft(drawnMiddle, middle.Point.X - 2.5);
			Canvas.SetTop(drawnMiddle, middle.Point.Y - 2.5);
		}
	}
}
