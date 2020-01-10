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
		private List<ControlLine> srcControlLines;
		private List<ControlLine> destControlLines;
		private SortedDictionary<ControlPoint, List<ControlLine>> srcControlDict;
		private SortedDictionary<ControlPoint, List<ControlLine>> destControlDict;
		private String srcImageFilename;
		private String destImageFilename;

		public List<ControlLine> SrcControlLines { get => srcControlLines;
			set => srcControlLines= value; }
		public List<ControlLine> DestControlLines
		{
			get => destControlLines;
			set => destControlLines = value;
		}
		public SortedDictionary<ControlPoint, List<ControlLine>> SrcControlDict
		{
			get => srcControlDict;
			set => srcControlDict = value;
		}
		public SortedDictionary<ControlPoint, List<ControlLine>> DestControlDict
		{
			get => destControlDict;
			set => destControlDict = value;
		}
		public String SrcImageFilename
		{
			get => srcImageFilename;
			set => srcImageFilename = value;
		}
		public String DestImageFilename
		{
			get => destImageFilename;
			set => destImageFilename = value;
		}

	} 
}
