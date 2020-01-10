using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageMorpher
{
	/// <summary>
	/// Interaction logic for ImageViewer.xaml
	/// </summary>
	public partial class ImageViewer : UserControl
	{
		enum EditState
		{
			Start,
			End,
			Middle,
			NewLine,
			None
		}
		private EditState editState = EditState.None;
		private bool selected = false;
		private String imgFileName;
		private int editIndex = 0;
		private ControlPoint prevEnd;
		private ControlPoint prevMid;
		private ControlPoint prevStart;
		private List<ControlLine> controlLines;
		private SortedDictionary<ControlPoint, List<ControlLine>> controlLineDict;
		private ControlLine currentLine;
		private ImageViewer otherViewer;

		public int EditIndex { get => editIndex; set => editIndex = value; }
		public bool Selected { get => selected; set => selected = value; }
		public List<ControlLine> ControlLines
		{
			get => controlLines;
			set => controlLines = value;
		}
		public SortedDictionary<ControlPoint, List<ControlLine>> ControlLineDict
		{
			get => controlLineDict;
			set => controlLineDict = value;
		}
		public ImageSource ImageSrc
		{
			get => image.Source;
			set => image.Source = value;
		}

		public String ImgFileName
		{
			get => imgFileName;
			set => imgFileName = value;
		}

		public Image getImage()
		{
			return image;
		}

		public ImageViewer()
		{
			InitializeComponent();
			controlLines = new List<ControlLine>();
			controlLineDict = new SortedDictionary<ControlPoint, List<ControlLine>>();
		}

		public void setOtherViewer(ImageViewer other)
		{
			otherViewer = other;
		}

		public void setImage()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			if (openFileDialog.ShowDialog() == true)
			{
				ImageSource imageSource = new BitmapImage(new Uri(openFileDialog.FileName));
				image.Source = imageSource;
				imgFileName = openFileDialog.FileName;
			}
		}

		public void loadProject(List<ControlLine> lines,
			SortedDictionary<ControlPoint, List<ControlLine>> dict, String filename)
		{
			canvas.Children.Clear();
			controlLines = lines;
			controlLineDict = dict;
			imgFileName = filename;
			ImageSource imageSource = new BitmapImage(new Uri(filename));
			image.Source = imageSource;
			foreach (ControlLine cl in controlLines)
			{
				cl.drawLine(canvas);
			}
		}

		private void mouseDown(object sender, MouseButtonEventArgs e)
		{
			if (image.Source == null)
			{
				return;
			}
			if (e.ChangedButton == MouseButton.Right)
			{
				return;
			}
			if (selected)
			{
				clearSelection();
			}
			Point mouseDownPos = e.GetPosition(grid);
			grid.CaptureMouse();
			ControlPoint downControlPoint = new ControlPoint(mouseDownPos);
			if (controlLineDict.ContainsKey(downControlPoint))
			{
				mouseDown_Edit(downControlPoint);
			}
			else
			{
				mouseDown_NewLine(mouseDownPos);
			}
		}

		private void clearSelection()
		{
			deHighlightLine();
			otherViewer.deHighlightLine();
			selected = false;
			otherViewer.Selected = false;
		}

		private void mouseDown_NewLine(Point mouseDownPos)
		{
			createLineStart(mouseDownPos);
			otherViewer.createLineStart(mouseDownPos);
			editState = EditState.NewLine;
		}

		private void mouseDown_Edit(ControlPoint downControlPoint)
		{
			grid.Cursor = Cursors.Arrow;
			selected = true;
			otherViewer.Selected = true;
			ControlLine cl = controlLineDict[downControlPoint][0];
			editIndex = controlLines.IndexOf(cl);
			otherViewer.EditIndex = editIndex;
			otherViewer.highlightLine();
			highlightLine();
			currentLine = cl;
			prevEnd = cl.End;
			prevMid = cl.Middle;
			prevStart = cl.Start;
			if (downControlPoint.CompareTo(cl.Start) == 0)
			{
				editState = EditState.Start;
			}
			else if (downControlPoint.CompareTo(cl.Middle) == 0)
			{
				editState = EditState.Middle;
			}
			else if (downControlPoint.CompareTo(cl.End) == 0)
			{
				editState = EditState.End;
			}
		}

		public void highlightLine()
		{
			controlLines[editIndex].highlight();
		}

		public void deHighlightLine()
		{
			controlLines[editIndex].deHighlight();
		}

		private void createLineStart(Point mouseDownPos)
		{
			currentLine = new ControlLine(canvas, new ControlPoint(mouseDownPos));	
		}

		private void mouseMove(object sender, MouseEventArgs e)
		{
			Point mousePos = e.GetPosition(grid);
			switch (editState)
			{
				case EditState.NewLine:
					createLineDrag(mousePos);
					otherViewer.createLineDrag(mousePos);
					break;
				case EditState.Start:
					currentLine.setStart(new ControlPoint(mousePos));
					currentLine.setMiddle();
					break;
				case EditState.Middle:
					currentLine.moveMiddle(new ControlPoint(mousePos));
					break;
				case EditState.End:
					currentLine.setEnd(new ControlPoint(mousePos));
					break;
				case EditState.None:
					ControlPoint downControlPoint = new ControlPoint(mousePos);
					if (controlLineDict.ContainsKey(downControlPoint))
					{
						grid.Cursor = Cursors.Hand;
					} else
					{
						grid.Cursor = Cursors.Arrow;
					}
					break;
				default:
					break;
			}
		}

		private void createLineDrag(Point mousePos)
		{
			currentLine.setEnd(new ControlPoint(mousePos));
		}

		private void mouseUp(object sender, MouseButtonEventArgs e)
		{
			Point mouseUpPos = e.GetPosition(grid);
			grid.ReleaseMouseCapture();
			switch (editState)
			{
				case EditState.NewLine:
					mouseUp_NewLine(mouseUpPos);
					break;
				case EditState.Start:
					mouseUp_Start(mouseUpPos);
					break;
				case EditState.Middle:
					mouseUp_Middle(mouseUpPos);
					break;
				case EditState.End:
					mouseUp_End(mouseUpPos);
					break;
				default:
					break;
			}
			editState = EditState.None;
		}

		private void mouseUp_NewLine(Point mouseUpPos)
		{
			createLineEnd(mouseUpPos);
			otherViewer.createLineEnd(mouseUpPos);
		}

		private void mouseUp_Start(Point mouseUpPos)
		{
			removeFromDict(prevStart);
			removeFromDict(prevMid);
			currentLine.setStart(new ControlPoint(mouseUpPos));
			currentLine.setMiddle();
			addToDict(currentLine.Start);
			addToDict(currentLine.Middle);
		}

		private void mouseUp_Middle(Point mouseUpPos)
		{
			removeFromDict(prevStart);
			removeFromDict(prevMid);
			removeFromDict(prevEnd);
			currentLine.moveMiddle(new ControlPoint(mouseUpPos));
			addToDict(currentLine.Start);
			addToDict(currentLine.Middle);
			addToDict(currentLine.End);
		}

		private void mouseUp_End(Point mouseUpPos)
		{
			removeFromDict(prevEnd);
			removeFromDict(prevMid);
			currentLine.setEnd(new ControlPoint(mouseUpPos));
			addToDict(currentLine.End);
			addToDict(currentLine.Middle);
		}

		private void createLineEnd(Point mouseUpPos)
		{
			currentLine.setEnd(new ControlPoint(mouseUpPos));
			controlLines.Add(currentLine);
			addToDict(currentLine.Start);
			addToDict(currentLine.End);
			addToDict(currentLine.Middle);
		}

		private void removeLine(object sender, RoutedEventArgs e)
		{
			if (selected)
			{
				destroySelectedLine();
				otherViewer.destroySelectedLine();
			}
		}

		private void removeAllLines(object sender, RoutedEventArgs e)
		{
			destroyAllLines();
			otherViewer.destroyAllLines();
		}

		public void destroySelectedLine()
		{
			currentLine = controlLines[editIndex];
			removeFromDict(currentLine.Start);
			removeFromDict(currentLine.Middle);
			removeFromDict(currentLine.End);
			currentLine.removeFromCanvas(canvas);
			controlLines.Remove(currentLine);
			selected = false;
		}

		public void destroyAllLines()
		{
			controlLineDict.Clear();
			controlLines.Clear();
			canvas.Children.Clear();
		}

		private void addToDict(ControlPoint cp)
		{
			if (controlLineDict.ContainsKey(cp))
			{
				controlLineDict[cp].Add(currentLine);
			}
			else
			{
				List<ControlLine> startList = new List<ControlLine>();
				startList.Add(currentLine);
				controlLineDict.Add(cp, startList);
			}
		}

		private void removeFromDict(ControlPoint cp)
		{
			List<ControlLine> pointList = controlLineDict[cp];
			pointList.Remove(currentLine);
			if (pointList.Count == 0)
			{
				controlLineDict.Remove(cp);
			}
		}
	}
}
