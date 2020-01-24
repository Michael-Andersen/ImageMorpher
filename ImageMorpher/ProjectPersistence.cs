using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageMorpher
{
	[Serializable]
	public class ProjectPersistence
	{
		public ControlLineVisual.Colour LineColour { get; set; }
		public ControlLineVisual.Colour StartColour { get; set; }
		public ControlLineVisual.Colour MiddleColour { get; set; }
		public ControlLineVisual.Colour EndColour { get; set; }
		public ControlLineVisual.Colour HighlightColour { get; set; }
		public double Tolerance { get; set; }
		public double LineThickness { get; set; }
		public double Diameter { get; set; }

		public List<ControlLine> SrcControlLines { get; set; }
		public List<ControlLine> DestControlLines { get; set; }
		public SortedDictionary<ControlPoint, List<ControlLine>> SrcControlDict { get; set; }
		public SortedDictionary<ControlPoint, List<ControlLine>> DestControlDict { get; set; }
		public string SrcImageFilename { get; set; }
		public string DestImageFilename { get; set; }
		public string ProjectPath { get; set; }
		public string ProjectName { get; set; }
		public Dictionary<string, int> MorphNames { get; set; }

	} 
}
