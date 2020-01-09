using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ImageMorpher
{
	class ControlPoint : IComparable
	{
		public Point point;
		private static double tolerance = 4;

		public ControlPoint(Point o)
		{
			point = o;
		}
	
		
		public int CompareTo(object obj)
		{
			ControlPoint p = (ControlPoint)obj;
			if (Math.Abs(p.point.X - this.point.X) < tolerance
				&& Math.Abs(p.point.Y - this.point.Y) < tolerance)
			{
				return 0;
			}
			if (p.point.X - this.point.X < 0)
			{
				return -1;
			} else
			{
				return 1;
			}
		}
	}
}
