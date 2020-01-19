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

		public bool IsSrc { get; set; }
		public ControlLine Pair { get; set; }
		public Vector Vect { get; set; }
		public Vector Norm { get; set; }
		public Point StartPixel { get; set; }
		public Point EndPixel { get; set; }

		public static double proj(Vector vector, Vector on)
		{
			return vector * on / on.Length;
		}

		public Vector xP(Point x)
		{
			return StartPixel - x;
		}

		public Vector pX(Point x)
		{
			return x - StartPixel;
		}

		public double startDistance(Point x)
		{
			return pX(x).Length;
		}
		public double endDistance(Point x)
		{
			return (x - EndPixel).Length;
		}

		public double distance(Point x)
		{
			return proj(xP(x), Norm);
		}

		public double fracLength(Point x)
		{
			return (pX(x)*Vect) / Vect.LengthSquared;
		}

		public ControlLine()
		{

		}


		public ControlLine(Point start, Point end)
		{
			StartPixel = start;
			EndPixel = end;
			setVector();
		}

		public void setStart(Point p) {
			StartPixel = p;
			setVector();
		}

		public void setEnd(Point p)
		{
			EndPixel = p;
			setVector();
		}
		private void setVector()
		{
			Vect = EndPixel - StartPixel;
			if (Vect.X == 0 && Vect.Y == 0)
			{
				Vect = new Vector(0.001, 0.001);
			}
			Norm = new Vector(-Vect.Y, Vect.X);
			
		}

		public void middleTranslation(Point start, Point end)
		{
			setStart(start);
			setEnd(end);
		}
	}
}
