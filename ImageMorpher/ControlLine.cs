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
	

    class ControlLine
	{
		private Line drawnLine;
		private Ellipse drawnStart;
		private Ellipse drawnEnd;
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
			drawnLine.X1 = o.point.X;
			drawnLine.Y1 = o.point.Y;
			Canvas.SetLeft(drawnStart, o.point.X - 2.5);
			Canvas.SetTop(drawnStart, o.point.Y - 2.5);
		}

		public void setEnd(ControlPoint o)
		{
			end = o;
			drawnEnd.Width = 5;
			drawnEnd.Height = 5;
			drawnEnd.Fill = Brushes.Red;
			Canvas.SetLeft(drawnEnd, o.point.X - 2.5);
			Canvas.SetTop(drawnEnd, o.point.Y - 2.5);
			setMiddle();
			drawnLine.X2 = o.point.X;
			drawnLine.Y2 = o.point.Y;
		}

		public void setMiddle()
		{
			drawnMiddle.Width = 5;
			drawnMiddle.Height = 5;
			drawnMiddle.Fill = Brushes.Blue;
			Point middlePoint = new Point();
			middlePoint.X = (start.point.X + end.point.X) / 2;
			middlePoint.Y = (start.point.Y + end.point.Y) / 2;
			middle = new ControlPoint(middlePoint);
			Canvas.SetLeft(drawnMiddle, middle.point.X - 2.5);
			Canvas.SetTop(drawnMiddle, middle.point.Y - 2.5);
		}
	}
}
