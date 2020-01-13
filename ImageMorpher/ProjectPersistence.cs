using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ImageMorpher
{
	[Serializable]
	public class ProjectPersistence
	{
		public ControlLine.Colour LineColour { get; set; }
		public ControlLine.Colour StartColour { get; set; }
		public ControlLine.Colour MiddleColour { get; set; }
		public ControlLine.Colour EndColour { get; set; }
		public ControlLine.Colour HighlightColour { get; set; }
		public double Tolerance { get; set; }
		public double LineThickness { get; set; }
		public double Diameter { get; set; }

		public List<ControlLine> SrcControlLines { get; set; }
		public List<ControlLine> DestControlLines { get; set; }
		public SortedDictionary<ControlPoint, List<ControlLine>> SrcControlDict { get; set; }
		public SortedDictionary<ControlPoint, List<ControlLine>> DestControlDict { get; set; }
		public string SrcImageFilename { get; set; }
		public string DestImageFilename { get; set; }

	} 
}
