using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private SettingsWindow settings;
		private Morpher morph;

		public MainWindow()
		{
			InitializeComponent();
			destViewer.setOtherViewer(srcViewer);
			srcViewer.setOtherViewer(destViewer);
			srcViewer.IsSrc = true;
			destViewer.IsSrc = false;
			settings = new SettingsWindow(srcViewer, destViewer);
		    morph = new Morpher();
			morphViewer.Morph = morph;
			morphViewer.Visibility = Visibility.Collapsed;
		}

		private void SetSrc_Click(object sender, RoutedEventArgs e)
		{
			srcViewer.setImage();
		}

		private void SetDest_Click(object sender, RoutedEventArgs e)
		{
			destViewer.setImage();
		}

		private void SaveProject_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			if (saveFileDialog.ShowDialog() == true)
			{
				Stream SaveFileStream = File.Create(saveFileDialog.FileName);
				BinaryFormatter serializer = new BinaryFormatter();
				ProjectPersistence save = new ProjectPersistence();
				save.DestControlDict = destViewer.ControlLineDict;
				save.DestControlLines = destViewer.ControlLines;
				save.DestImageFilename = destViewer.ImgFileName;
				save.SrcControlDict = srcViewer.ControlLineDict;
				save.SrcControlLines = srcViewer.ControlLines;
				save.SrcImageFilename = srcViewer.ImgFileName;
				save.StartColour = ControlLineVisual.START_COLOUR;
				save.MiddleColour = ControlLineVisual.MIDDLE_COLOUR;
				save.EndColour = ControlLineVisual.END_COLOUR;
				save.LineColour = ControlLineVisual.LINE_COLOUR;
				save.HighlightColour = ControlLineVisual.HIGHLIGHT_COLOUR;
				save.LineThickness = ControlLineVisual.LINE_THICKNESS;
				save.Diameter = ControlLineVisual.DIAMETER;
				save.Tolerance = ControlPoint.TOLERANCE;
				save.FrameSrcs = new List<string>();
				for (int i = 0; i < morph.Frames.Count; i++)
				{
					save.FrameSrcs.Add("c:\\Users\\Mike\\Downloads\\" + morph.UUID + "_" + i + ".png");
				}
				
				serializer.Serialize(SaveFileStream, save);
				SaveFileStream.Close();
			}
		}

		private void OpenProject_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			if (openFileDialog.ShowDialog() == true)
			{
				Stream openFileStream = File.OpenRead(openFileDialog.FileName);
				BinaryFormatter deserializer = new BinaryFormatter();
				//must be temporarily set to 0 to ensure it's lower than saved tolerance due to sorted dictionary
				ControlPoint.TOLERANCE = 0; 
				ProjectPersistence loaded = (ProjectPersistence)deserializer.Deserialize(openFileStream);
				loadSettings(loaded);
				if (loaded.FrameSrcs != null && loaded.FrameSrcs.Count > 0) { 
					for (int i = 0; i < loaded.FrameSrcs.Count; i++)
					{
						BitmapSource bms = new BitmapImage(new Uri(@loaded.FrameSrcs[i]));
						morph.Frames.Add(bms);
					} 
					morph.Morphed = true;
				}
				srcViewer.loadProject(loaded.SrcControlLines, loaded.SrcControlDict, loaded.SrcImageFilename);
				destViewer.loadProject(loaded.DestControlLines, loaded.DestControlDict, 
					loaded.DestImageFilename);
				openFileStream.Close();
			}
		}

		private void loadSettings(ProjectPersistence loaded)
		{
			settings.load(loaded.LineThickness, loaded.Diameter, loaded.Tolerance, loaded.LineColour,
				loaded.StartColour, loaded.MiddleColour, loaded.EndColour, loaded.HighlightColour);
		}

		private void Morph_Click(object sender, RoutedEventArgs e)
		{
			morph.SrcLines = srcViewer.ControlLines;
			morph.DestLines = destViewer.ControlLines;
			morph.setSrc((BitmapSource)srcViewer.ImageSrc);
			morph.setDest((BitmapSource)destViewer.ImageSrc);
			if (!morph.Morphed)
			{
				morph.startThreads();
			}
			srcViewer.Visibility = Visibility.Hidden;
			destViewer.Visibility = Visibility.Hidden;
			morphViewer.Visibility = Visibility.Visible;
			morphViewer.Src = srcViewer.ImageSrc;
			morphViewer.Dest = destViewer.ImageSrc;
			morphViewer.setImageSrc(morphViewer.Src);
			modeItem.Header = "Change Control Lines";
			modeItem.Click -= Morph_Click;
			modeItem.Click += ChangeControlLines_Click;
			UpdateLayout();
		}

		private void ChangeControlLines_Click(object sender, RoutedEventArgs e)
		{
			srcViewer.Visibility = Visibility.Visible;
			destViewer.Visibility = Visibility.Visible;
			morphViewer.Visibility = Visibility.Hidden;
			modeItem.Header = "Create Morph";
			modeItem.Click += Morph_Click;
			modeItem.Click -= ChangeControlLines_Click;
		}

		private void Settings_Click(object sender, RoutedEventArgs e)
		{
			settings.Show();
		}

		private void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			settings.OwnerClosing = true;
			settings.Close();
		}
	}
}
