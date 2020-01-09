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
		bool makingLine = false;
		bool editingStart = false;
		bool editingEnd = false;
		bool editingMiddle = false;
		int editIndex = 0;
		Point mouseDownPos;
		ControlPoint prevEnd;
		ControlPoint prevMid;
		ControlPoint prevStart;
		List<ControlLine> controlLines;
		SortedDictionary<ControlPoint, List<ControlLine>> controlLineDict;
		ControlLine currentLine;
		ImageViewer otherViewer;

		public int EditIndex{ get => editIndex; set => editIndex = value; }

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
			}

		//	canvas.Height = image.ActualHeight;
		//	canvas.Width = image.ActualWidth;
		}

		private void mouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Right)
			{
				return;
			}
			if (controlLines.Count > 0)
			{
				deHighlightLine(editIndex);
				otherViewer.deHighlightLine(editIndex);
			}
			mouseDownPos = e.GetPosition(grid);
			ControlPoint downControlPoint = new ControlPoint(mouseDownPos);
			if (controlLineDict.ContainsKey(downControlPoint))
			{
				ControlLine cl = controlLineDict[downControlPoint][0];
				deHighlightLine(editIndex);
				otherViewer.deHighlightLine(editIndex);
				editIndex = controlLines.IndexOf(cl);
				otherViewer.EditIndex = editIndex;
				otherViewer.highlightLine(editIndex);
				highlightLine(editIndex);
				currentLine = cl;
				if (downControlPoint.CompareTo(cl.Middle) == 0) {
					editingMiddle = true;
					prevEnd = cl.End;
					prevMid = downControlPoint;
					prevStart = cl.Start;
					return;
				}
				if (downControlPoint.CompareTo(cl.Start) == 0)
				{
					editingStart = true;
					currentLine = cl;
					prevStart = downControlPoint;
					prevMid = cl.Middle;
					return;
				}
				if (downControlPoint.CompareTo(cl.End) == 0)
				{
					editingEnd = true;
					prevEnd = downControlPoint;
					prevMid = cl.Middle;
					currentLine = cl;
					return;
				}
			}
			grid.CaptureMouse();
			createLineStart(mouseDownPos);
			otherViewer.createLineStart(mouseDownPos);
			makingLine = true;
		}

		public void highlightLine(int index)
		{
			controlLines[index].highlight();
		}

		public void deHighlightLine(int index)
		{
			controlLines[index].deHighlight();
		}

		private void createLineStart(Point mouseDownPos)
		{
			currentLine = new ControlLine(canvas, new ControlPoint(mouseDownPos));	
		}

		private void mouseMove(object sender, MouseEventArgs e)
		{
			Point mousePos = e.GetPosition(grid);
			if (makingLine)
			{
				createLineDrag(mousePos);
				otherViewer.createLineDrag(mousePos);
			}
			if (editingStart)
			{
				currentLine.setStart(new ControlPoint(mousePos));
				currentLine.setMiddle();
			}
			if (editingEnd)
			{
				currentLine.setEnd(new ControlPoint(mousePos));
			}
			if (editingMiddle)
			{
				currentLine.moveMiddle(new ControlPoint(mousePos));
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
			if (makingLine)
			{
				makingLine = false;
				createLineEnd(mouseUpPos);
				otherViewer.createLineEnd(mouseUpPos);
			}
			if (editingEnd)
			{
				List<ControlLine> pointList = controlLineDict[prevEnd];
				pointList.Remove(currentLine);
				if (pointList.Count == 0)
				{
					controlLineDict.Remove(prevEnd);
				}
				pointList = controlLineDict[prevMid];
				pointList.Remove(currentLine);
				if (pointList.Count == 0)
				{
					controlLineDict.Remove(prevMid);
				}
				currentLine.setEnd(new ControlPoint(mouseUpPos));
				if (controlLineDict.ContainsKey(currentLine.End))
				{
					controlLineDict[currentLine.End].Add(currentLine);
				}
				else
				{
					List<ControlLine> startList = new List<ControlLine>();
					startList.Add(currentLine);
					controlLineDict.Add(currentLine.End, startList);
				}
				if (controlLineDict.ContainsKey(currentLine.Middle))
				{
					controlLineDict[currentLine.Middle].Add(currentLine);
				}
				else
				{
					List<ControlLine> startList = new List<ControlLine>();
					startList.Add(currentLine);
					controlLineDict.Add(currentLine.Middle, startList);
				}
				editingEnd = false;
			}

			if (editingStart)
			{
				List<ControlLine> pointList = controlLineDict[prevStart];
				pointList.Remove(currentLine);
				if (pointList.Count == 0)
				{
					controlLineDict.Remove(prevStart);
				}
				pointList = controlLineDict[prevMid];
				pointList.Remove(currentLine);
				if (pointList.Count == 0)
				{
					controlLineDict.Remove(prevMid);
				}
				currentLine.setStart(new ControlPoint(mouseUpPos));
				currentLine.setMiddle();
				if (controlLineDict.ContainsKey(currentLine.Start))
				{
					controlLineDict[currentLine.Start].Add(currentLine);
				}
				else
				{
					List<ControlLine> startList = new List<ControlLine>();
					startList.Add(currentLine);
					controlLineDict.Add(currentLine.Start, startList);
				}
				if (controlLineDict.ContainsKey(currentLine.Middle))
				{
					controlLineDict[currentLine.Middle].Add(currentLine);
				}
				else
				{
					List<ControlLine> startList = new List<ControlLine>();
					startList.Add(currentLine);
					controlLineDict.Add(currentLine.Middle, startList);
				}
				editingStart = false;
			}
			if (editingMiddle)
			{
				List<ControlLine> pointList = controlLineDict[prevStart];
				pointList.Remove(currentLine);
				if (pointList.Count == 0)
				{
					controlLineDict.Remove(prevStart);
				}
				pointList = controlLineDict[prevMid];
				pointList.Remove(currentLine);
				if (pointList.Count == 0)
				{
					controlLineDict.Remove(prevMid);
				}
				pointList = controlLineDict[prevEnd];
				pointList.Remove(currentLine);
				if (pointList.Count == 0)
				{
					controlLineDict.Remove(prevEnd);
				}
				currentLine.moveMiddle(new ControlPoint(mouseUpPos));
				if (controlLineDict.ContainsKey(currentLine.Start))
				{
					controlLineDict[currentLine.Start].Add(currentLine);
				}
				else
				{
					List<ControlLine> startList = new List<ControlLine>();
					startList.Add(currentLine);
					controlLineDict.Add(currentLine.Start, startList);
				}
				if (controlLineDict.ContainsKey(currentLine.Middle))
				{
					controlLineDict[currentLine.Middle].Add(currentLine);
				}
				else
				{
					List<ControlLine> startList = new List<ControlLine>();
					startList.Add(currentLine);
					controlLineDict.Add(currentLine.Middle, startList);
				}
				if (controlLineDict.ContainsKey(currentLine.End))
				{
					controlLineDict[currentLine.End].Add(currentLine);
				}
				else
				{
					List<ControlLine> startList = new List<ControlLine>();
					startList.Add(currentLine);
					controlLineDict.Add(currentLine.End, startList);
				}
				editingMiddle = false;
			}
		}

		private void createLineEnd(Point mouseUpPos)
		{
			currentLine.setEnd(new ControlPoint(mouseUpPos));
			controlLines.Add(currentLine);
			if (controlLineDict.ContainsKey(currentLine.Start))
			{
				controlLineDict[currentLine.Start].Add(currentLine);
			}
			else
			{
				List<ControlLine> startList = new List<ControlLine>();
				startList.Add(currentLine);
				controlLineDict.Add(currentLine.Start, startList);
			}
			if (controlLineDict.ContainsKey(currentLine.End))
			{
				controlLineDict[currentLine.End].Add(currentLine);
			}
			else
			{
				List<ControlLine> startList = new List<ControlLine>();
				startList.Add(currentLine);
				controlLineDict.Add(currentLine.End, startList);
			}
			if (controlLineDict.ContainsKey(currentLine.Middle))
			{
				controlLineDict[currentLine.Middle].Add(currentLine);
			}
			else
			{
				List<ControlLine> startList = new List<ControlLine>();
				startList.Add(currentLine);
				controlLineDict.Add(currentLine.Middle, startList);
			}
		}
		private void removeLine(object sender, RoutedEventArgs e)
		{
			destroySelectedLine();
			otherViewer.destroySelectedLine();
		}

		public void destroySelectedLine()
		{
			currentLine = controlLines[editIndex];
			List<ControlLine> pointList = controlLineDict[currentLine.Start];
			pointList.Remove(currentLine);
			if (pointList.Count == 0)
			{
				controlLineDict.Remove(currentLine.Start);
			}
			pointList = controlLineDict[currentLine.Middle];
			pointList.Remove(currentLine);
			if (pointList.Count == 0)
			{
				controlLineDict.Remove(currentLine.Middle);
			}
			pointList = controlLineDict[currentLine.End];
			pointList.Remove(currentLine);
			if (pointList.Count == 0)
			{
				controlLineDict.Remove(currentLine.End);
			}
			currentLine.removeFromCanvas(canvas);
		}

	}

	



}
