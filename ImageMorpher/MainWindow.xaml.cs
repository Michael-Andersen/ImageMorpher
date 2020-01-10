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
		public MainWindow()
		{
			InitializeComponent();
			destViewer.setOtherViewer(srcViewer);
			srcViewer.setOtherViewer(destViewer);
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
				ProjectPersistence loaded = (ProjectPersistence)deserializer.Deserialize(openFileStream);
				srcViewer.loadProject(loaded.SrcControlLines, loaded.SrcControlDict, loaded.SrcImageFilename);
				destViewer.loadProject(loaded.DestControlLines, loaded.DestControlDict, 
					loaded.DestImageFilename);
				openFileStream.Close();
			}
		}

		private void Morph_Click(object sender, RoutedEventArgs e)
		{
			srcViewer.Visibility = Visibility.Hidden;
			destViewer.Visibility = Visibility.Hidden;
			morphViewer.Visibility = Visibility.Visible;
			morphViewer.setImageSrc(srcViewer.getImage().Source);
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
	}
}
