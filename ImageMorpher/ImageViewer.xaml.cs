using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
		private ControlPoint prevEnd;
		private ControlPoint prevMid;
		private ControlPoint prevStart;
		private ControlLine currentLine;
		private ImageViewer otherViewer;

		public int EditIndex { get; set; } = 0;
		public bool Selected { get; set; } = false;
		public bool IsSrc { get; set; }
		public List<ControlLine> ControlLines { get; set; }
		public SortedDictionary<ControlPoint, List<ControlLine>> ControlLineDict { get; set; }
		public ImageSource ImageSrc
		{
			get => image.Source;
			set => image.Source = value;
		}

		public string ImgFileName { get; set; }

		public Point GridToPixel(Point p)
		{
			Point imagePoint = grid.TranslatePoint(p, image);
			double pixelX = (int)(imagePoint.X * ((BitmapSource)image.Source).PixelWidth / image.ActualWidth);
			double pixelY = (int)(imagePoint.Y * ((BitmapSource)image.Source).PixelHeight / image.ActualHeight);
			return new Point(pixelX, pixelY);
		}

		public Point PixelToGrid(Point p)
		{
			double imageX = (int)(p.X * image.ActualWidth / ((BitmapSource)image.Source).PixelWidth);
			double imageY = (int)(p.Y * image.ActualHeight / ((BitmapSource)image.Source).PixelHeight);
			return image.TranslatePoint(new Point(imageX, imageY), grid);
		}


		public Image getImage()
		{
			return image;
		}

		public ImageViewer()
		{
			InitializeComponent();
			ControlLines = new List<ControlLine>();
			ControlLineDict = new SortedDictionary<ControlPoint, List<ControlLine>>();
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
				string prevFile = ImgFileName;
				ImageSource prevSrc = image.Source;
				image.Source = imageSource;
				ImgFileName = openFileDialog.FileName;
				if (otherViewer.ImageSrc != null && 
					(((BitmapSource)image.Source).PixelHeight != ((BitmapSource)otherViewer.ImageSrc).PixelHeight
					|| ((BitmapSource)image.Source).PixelWidth != ((BitmapSource)otherViewer.ImageSrc).PixelWidth))
				{
					MessageBoxResult mbr = MessageBox.Show("The image you are adding has different dimensions than the other image." +
						"Would you like the images to be automatically cropped to the same size? " +
						"If not, then the other image will be removed.", "", 
						MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
					if (mbr == MessageBoxResult.Yes)
					{
						crop();
					}
					else if (mbr == MessageBoxResult.No)
					{
						otherViewer.ImageSrc = null;
						otherViewer.ImgFileName = null;
					}
					else
					{
						image.Source = prevSrc;
						ImgFileName = prevFile;
					}
				}
			}
		}

		private void crop()
		{
			if (((BitmapSource)image.Source).PixelHeight > ((BitmapSource)otherViewer.ImageSrc).PixelHeight)
			{
				cropHeight();
			}
			else if (((BitmapSource)image.Source).PixelHeight < ((BitmapSource)otherViewer.ImageSrc).PixelHeight)
			{
				otherViewer.cropHeight();
			}
			if (((BitmapSource)image.Source).PixelWidth > ((BitmapSource)otherViewer.ImageSrc).PixelWidth)
			{
				cropWidth();
			}
			else if (((BitmapSource)image.Source).PixelWidth < ((BitmapSource)otherViewer.ImageSrc).PixelWidth)
			{
				otherViewer.cropWidth();
			}
		}

		public void cropHeight()
		{
			int Ycoord = (int)((((BitmapSource)image.Source).PixelHeight
				- ((BitmapSource)otherViewer.ImageSrc).PixelHeight) / 2);
			int height = (int)(((BitmapSource)otherViewer.ImageSrc).PixelHeight);
			int Xcoord = 0;
			int width = (int)((BitmapSource)image.Source).PixelWidth;
			CroppedBitmap cb = new CroppedBitmap(
							(BitmapSource)image.Source,
									new Int32Rect(Xcoord, Ycoord, width, height));
			image.Source = cb;
		}

		public void cropWidth()
		{
			int Ycoord = 0;
			int height = (int)((BitmapSource)image.Source).PixelHeight;
			int Xcoord = (int)((((BitmapSource)image.Source).PixelWidth 
				- ((BitmapSource)otherViewer.ImageSrc).PixelWidth) / 2);
			int width = (int)(((BitmapSource)otherViewer.ImageSrc).PixelWidth);
			CroppedBitmap cb = new CroppedBitmap(
					(BitmapSource)image.Source,
							new Int32Rect(Xcoord, Ycoord, width, height));
			image.Source = cb;
		}
		public void loadProject(List<ControlLine> lines,
			SortedDictionary<ControlPoint, List<ControlLine>> dict, String filename)
		{
			canvas.Children.Clear();
			ControlLines = lines;
			ControlLineDict = dict;
			ImgFileName = filename;
			ImageSource imageSource = new BitmapImage(new Uri(filename));
			image.Source = imageSource;
			if (otherViewer.ImageSrc != null &&
					(((BitmapSource)image.Source).PixelHeight != ((BitmapSource)otherViewer.ImageSrc).PixelHeight
					|| ((BitmapSource)image.Source).PixelWidth != ((BitmapSource)otherViewer.ImageSrc).PixelWidth))
			{
				crop();
			}
			UpdateLayout();
			foreach (ControlLine cl in ControlLines)
			{
				reDrawLine(cl);
			}
		}

		private void mouseDown(object sender, MouseButtonEventArgs e)
		{
			if (image.Source == null || otherViewer.image.Source == null)
			{
				return;
			}
			if (e.ChangedButton == MouseButton.Right)
			{
				return;
			}
			if (Selected)
			{
				clearSelection();
			}
			Point mouseDownPos = e.GetPosition(grid);
			ControlPoint downControlPoint = new ControlPoint(mouseDownPos);
			if (ControlLineDict.ContainsKey(downControlPoint))
			{
				mouseDown_Edit(downControlPoint);
			}
			else
			{
				Point pixelPoint = GridToPixel(mouseDownPos);
				mouseDown_NewLine(mouseDownPos);
			}
		}

		private void clearSelection()
		{
			deHighlightLine();
			otherViewer.deHighlightLine();
			Selected = false;
			otherViewer.Selected = false;
		}

		private void mouseDown_NewLine(Point mouseDownPos)
		{
			createLineStart(mouseDownPos);
			currentLine.Pair = otherViewer.createLineStart(mouseDownPos);
			otherViewer.currentLine.Pair = currentLine;
			editState = EditState.NewLine;
		}

		private void mouseDown_Edit(ControlPoint downControlPoint)
		{
			grid.Cursor = Cursors.Arrow;
			Selected = true;
			otherViewer.Selected = true;
			ControlLine cl = ControlLineDict[downControlPoint][0];
			EditIndex = ControlLines.IndexOf(cl);
			otherViewer.EditIndex = EditIndex;
			otherViewer.highlightLine();
			highlightLine();
			currentLine = cl;
			prevEnd = ((ControlLineVisual)cl).End;
			prevMid = ((ControlLineVisual)cl).Middle;
			prevStart = ((ControlLineVisual)cl).Start;
			if (downControlPoint.CompareTo(((ControlLineVisual)cl).Start) == 0)
			{
				editState = EditState.Start;
			}
			else if (downControlPoint.CompareTo(((ControlLineVisual)cl).Middle) == 0)
			{
				editState = EditState.Middle;
			}
			else if (downControlPoint.CompareTo(((ControlLineVisual)cl).End) == 0)
			{
				editState = EditState.End;
			}
		}

		public void highlightLine()
		{
			((ControlLineVisual)ControlLines[EditIndex]).highlight();
		}

		public void deHighlightLine()
		{
			((ControlLineVisual)ControlLines[EditIndex]).deHighlight();
		}

		public void updateVisualSettings()
		{
			canvas.Children.Clear();
			foreach (ControlLine cl in ControlLines)
			{
				((ControlLineVisual)cl).drawLine(this);
			}
		}

		private ControlLine createLineStart(Point mouseDownPos)
		{

			currentLine = new ControlLineVisual(this, new ControlPoint(mouseDownPos));
			currentLine.IsSrc = IsSrc;
			return currentLine;
		}

		private void mouseMove(object sender, MouseEventArgs e)
		{
			if (image.Source == null || otherViewer.image.Source == null)
			{
				return;
			}
			Point mousePos = e.GetPosition(grid);
			Point pixelPos = GridToPixel(mousePos);
			switch (editState)
			{
				case EditState.NewLine:
					createLineDrag(mousePos);
					otherViewer.createLineDrag(mousePos);
					break;
				case EditState.Start:
					((ControlLineVisual)currentLine).drawStart(new ControlPoint(mousePos));
					((ControlLineVisual)currentLine).drawMiddle();
					break;
				case EditState.Middle:
					((ControlLineVisual)currentLine).moveMiddle(new ControlPoint(mousePos));
					break;
				case EditState.End:
					((ControlLineVisual)currentLine).drawEnd(new ControlPoint(mousePos));
					break;
				case EditState.None:
					ControlPoint downControlPoint = new ControlPoint(mousePos);
					if (ControlLineDict.ContainsKey(downControlPoint))
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
			((ControlLineVisual)currentLine).drawEnd(new ControlPoint(mousePos));
		}

		private void mouseUp(object sender, MouseButtonEventArgs e)
		{
			if (image.Source == null || otherViewer.image.Source == null)
			{
				return;
			}
			Point mouseUpPos = e.GetPosition(grid);
			Point pixelPos = GridToPixel(mouseUpPos);
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

		private void mouseUp_NewLine(Point mousePos)
		{
			createLineEnd(mousePos);
			otherViewer.createLineEnd(mousePos);
		}

		private void mouseUp_Start(Point mousePos)
		{
			removeFromDict(prevStart);
			removeFromDict(prevMid);
			((ControlLineVisual)currentLine).drawStart(new ControlPoint(mousePos));
			currentLine.setStart(GridToPixel(mousePos));
			((ControlLineVisual)currentLine).drawMiddle();
			addToDict(((ControlLineVisual)currentLine).Start);
			addToDict(((ControlLineVisual)currentLine).Middle);
		}

		private void mouseUp_Middle(Point mousePos)
		{
			removeFromDict(prevStart);
			removeFromDict(prevMid);
			removeFromDict(prevEnd);
			((ControlLineVisual)currentLine).moveMiddle(new ControlPoint(mousePos));
			addToDict(((ControlLineVisual)currentLine).Start);
			addToDict(((ControlLineVisual)currentLine).Middle);
			addToDict(((ControlLineVisual)currentLine).End);
		}

		private void mouseUp_End(Point mousePos)
		{
			removeFromDict(prevEnd);
			removeFromDict(prevMid);
			((ControlLineVisual)currentLine).drawEnd(new ControlPoint(mousePos));
			((ControlLineVisual)currentLine).setEnd(GridToPixel(mousePos));
			addToDict(((ControlLineVisual)currentLine).End);
			addToDict(((ControlLineVisual)currentLine).Middle);
		}

		private void createLineEnd(Point mousePos)
		{
			((ControlLineVisual)currentLine).drawEnd(new ControlPoint(mousePos));
			((ControlLineVisual)currentLine).setEnd(GridToPixel(mousePos));
			ControlLines.Add(currentLine);
			addToDict(((ControlLineVisual)currentLine).Start);
			addToDict(((ControlLineVisual)currentLine).End);
			addToDict(((ControlLineVisual)currentLine).Middle);
		}

		private void removeLine(object sender, RoutedEventArgs e)
		{
			if (Selected)
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
			currentLine = ControlLines[EditIndex];
			removeFromDict(((ControlLineVisual)currentLine).Start);
			removeFromDict(((ControlLineVisual)currentLine).Middle);
			removeFromDict(((ControlLineVisual)currentLine).End);
			((ControlLineVisual)currentLine).removeFromCanvas();
			ControlLines.Remove(currentLine);
			Selected = false;
		}

		public void destroyAllLines()
		{
			ControlLineDict.Clear();
			ControlLines.Clear();
			canvas.Children.Clear();
			Selected = false;
		}

		private void addToDict(ControlPoint cp)
		{
			if (ControlLineDict.ContainsKey(cp))
			{
				ControlLineDict[cp].Add(currentLine);
			}
			else
			{
				List<ControlLine> startList = new List<ControlLine>();
				startList.Add(currentLine);
				ControlLineDict.Add(cp, startList);
			}
		}

		private void removeFromDict(ControlPoint cp)
		{
			if (ControlLineDict.ContainsKey(cp))
			{
				List<ControlLine> pointList = ControlLineDict[cp];
				pointList.Remove(currentLine);
				if (pointList.Count == 0)
				{
					ControlLineDict.Remove(cp);
				}
			}
		}
		
		private void SizeChangedEventHandler(Object sender, SizeChangedEventArgs e)
		{
			canvas.Children.Clear();
			foreach (ControlLine cl in ControlLines)
			{
				reDrawLine(cl);
			}
		}

		private void reDrawLine(ControlLine cl)
		{
			currentLine = cl;
			removeFromDict(((ControlLineVisual)cl).Start);
			removeFromDict(((ControlLineVisual)cl).Middle);
			removeFromDict(((ControlLineVisual)cl).End);
			((ControlLineVisual)cl).Start = new ControlPoint(PixelToGrid(((ControlLineVisual)cl).StartPixel));
			((ControlLineVisual)cl).End = new ControlPoint(PixelToGrid(((ControlLineVisual)cl).EndPixel));
			((ControlLineVisual)cl).drawLine(this);
			addToDict(((ControlLineVisual)cl).Start);
			addToDict(((ControlLineVisual)cl).Middle);
			addToDict(((ControlLineVisual)cl).End);
		}

	}
}
