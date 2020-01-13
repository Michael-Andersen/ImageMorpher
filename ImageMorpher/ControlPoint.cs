using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ImageMorpher
{
	[Serializable]
	public class ControlPoint : IComparable
	{
		public static double TOLERANCE = 4;

		public ControlPoint(Point o)
		{
			Point = o;
		}

		public Point Point { get; set; }


		public int CompareTo(object obj)
		{
			ControlPoint p = (ControlPoint)obj;
			if (Math.Abs(p.Point.X - this.Point.X) < TOLERANCE
				&& Math.Abs(p.Point.Y - this.Point.Y) < TOLERANCE)
			{
				return 0;
			}
			if (p.Point.X - this.Point.X < 0)
			{
				return -1;
			} else
			{
				return 1;
			}
		}
	}
}
