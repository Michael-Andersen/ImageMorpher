using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		private Morpher morpher;
		private NewDialog newProjectDialog;
		private NewDialog newMorphDialog;
		

		public MainWindow()
		{
			InitializeComponent();
			destViewer.setOtherViewer(srcViewer);
			srcViewer.setOtherViewer(destViewer);
			srcViewer.IsSrc = true;
			destViewer.IsSrc = false;
			settings = new SettingsWindow(srcViewer, destViewer);
			newProjectDialog = new NewDialog();
			newProjectDialog.OkEvent += (sender, args) =>
			{
				Morpher.PROJECT_NAME = newProjectDialog.box.Text;
				using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
				{
					System.Windows.Forms.DialogResult result = dialog.ShowDialog();
					Morpher.PROJECT_PATH = dialog.SelectedPath + "\\";
					Directory.CreateDirectory(Morpher.PROJECT_PATH + Morpher.PROJECT_NAME);
				}
				newProjectDialog.Hide();
			};
			newMorphDialog = new NewDialog();
			newMorphDialog.OkEvent += (sender, args) =>
			{
				newMorphDialog.MorphName = newMorphDialog.box.Text;
				newMorphDialog.Hide();
			};
			morpher = new Morpher();
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

		private void NewProject_Click(object sender, RoutedEventArgs e)
		{
			newProjectDialog.Show();
			
		}

			private void SaveProject_Click(object sender, RoutedEventArgs e)
		{
			save();
		}

		private void save()
		{
			Stream SaveFileStream = File.Create(Morpher.PROJECT_PATH + Morpher.PROJECT_NAME 
				+ "\\" + Morpher.PROJECT_NAME);
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
			save.ProjectName = Morpher.PROJECT_NAME;
			save.ProjectPath = Morpher.PROJECT_PATH;
			save.MorphNames = new Dictionary<string, int>();
			foreach (String key in morphViewer.morphDict.Keys)
			{
				save.MorphNames.Add(key, morphViewer.morphDict[key].Frames.Count);
			}
			serializer.Serialize(SaveFileStream, save);
			SaveFileStream.Close();
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
				morphViewer.morphDict.Clear();
				if (loaded.MorphNames.Count > 0) {
					foreach (string key in loaded.MorphNames.Keys)
					{
						Morph morph = new Morph();
						morph.Frames = new List<BitmapSource>();
						for (int j = 0; j < loaded.MorphNames[key]; j++) {
							BitmapSource bms = Morpher.LoadImage(loaded.ProjectPath + loaded.ProjectName + "\\" + key
								+ "_" + j + ".png");
							morph.Frames.Add(bms);
						}
						morph.MorphName = key;
						morphViewer.morphDict.Add(key, morph);
						newMorphDialog.MorphName = key;
					}
					
				}
				Morpher.PROJECT_NAME = loaded.ProjectName;
				Morpher.PROJECT_PATH = loaded.ProjectPath;
				srcViewer.loadProject(loaded.SrcControlLines, loaded.SrcControlDict, loaded.SrcImageFilename);
				destViewer.loadProject(loaded.DestControlLines, loaded.DestControlDict, 
					loaded.DestImageFilename);
				openFileStream.Close();
			}
		}

		private void loadSettings(ProjectPersistence loaded)
		{
			settings.clSettings.load(loaded.LineThickness, loaded.Diameter, loaded.Tolerance, loaded.LineColour,
				loaded.StartColour, loaded.MiddleColour, loaded.EndColour, loaded.HighlightColour);
		}

		private void ManageMorphs_Click(object sender, RoutedEventArgs e)
		{
			ManageMorphWindow mpw = new ManageMorphWindow(morphViewer.morphDict);
			mpw.ShowDialog();
			save();
			morphViewer.updateMorphs();
		}

		private void NewMorph_Click(object sender, RoutedEventArgs e)
		{
			if (!(bool)settings.performSettings.benchmarkBox.IsChecked)
			{
				Morph morph = new Morph();
				newMorphDialog.ShowDialog();
				morphViewer.morphDict.Add(newMorphDialog.MorphName, morph);
				morph.MorphName = newMorphDialog.MorphName;
				newMorph(morph);
				MessageBoxResult mbr = MessageBox.Show("Morph Ready!", "",
						MessageBoxButton.OK, MessageBoxImage.Information);
			} else
			{
				Dictionary<string, TimeSpan> times = new Dictionary<string, TimeSpan>();
				Stopwatch sw = new Stopwatch();
				string[] benchMarkKeys = new string[8] { "1 Thread", "2 Threads", "3 Threads",
				"4 Threads", "5 Threads", "6 Threads", "7 Threads", "8 Threads"};
				Morph morph = new Morph();
				newMorphDialog.ShowDialog();
				morphViewer.morphDict.Add(newMorphDialog.MorphName, morph);
				morph.MorphName = newMorphDialog.MorphName;
				if ((bool)settings.performSettings.thread1Box.IsChecked)
				{
					Morpher.NumThreads = 1;
					sw.Start();
					newMorph(morph);
					sw.Stop();
					times.Add(benchMarkKeys[0], sw.Elapsed);
					morph.MorphName = "BenchMark";
				}
				if ((bool)settings.performSettings.thread2Box.IsChecked)
				{
					Morpher.NumThreads = 2;
					sw.Start();
					newMorph(morph);
					sw.Stop();
					times.Add(benchMarkKeys[1], sw.Elapsed);
					for (int i = 0; i < morph.Frames.Count; i++)
					{
						File.Delete(Morpher.PROJECT_PATH + Morpher.PROJECT_NAME + "\\" + morph.MorphName + "_" + i + ".png");
					}
				}
				if ((bool)settings.performSettings.thread3Box.IsChecked)
				{
					Morpher.NumThreads = 3;
					sw.Start();
					newMorph(morph);
					sw.Stop();
					times.Add(benchMarkKeys[2], sw.Elapsed);
					for (int i = 0; i < morph.Frames.Count; i++)
					{
						File.Delete(Morpher.PROJECT_PATH + Morpher.PROJECT_NAME + "\\" + morph.MorphName + "_" + i + ".png");
					}
				}
				if ((bool)settings.performSettings.thread4Box.IsChecked)
				{
					Morpher.NumThreads = 4;
					sw.Start();
					newMorph(morph);
					sw.Stop();
					times.Add(benchMarkKeys[3], sw.Elapsed);
					for (int i = 0; i < morph.Frames.Count; i++)
					{
						File.Delete(Morpher.PROJECT_PATH + Morpher.PROJECT_NAME + "\\" + morph.MorphName + "_" + i + ".png");
					}
				}
				if ((bool)settings.performSettings.thread5Box.IsChecked)
				{
					Morpher.NumThreads = 5;
					sw.Start();
					newMorph(morph);
					sw.Stop();
					times.Add(benchMarkKeys[4], sw.Elapsed);
					for (int i = 0; i < morph.Frames.Count; i++)
					{
						File.Delete(Morpher.PROJECT_PATH + Morpher.PROJECT_NAME + "\\" + morph.MorphName + "_" + i + ".png");
					}
				}
				if ((bool)settings.performSettings.thread6Box.IsChecked)
				{
					Morpher.NumThreads = 6;
					sw.Start();
					newMorph(morph);
					sw.Stop();
					times.Add(benchMarkKeys[5], sw.Elapsed);
					for (int i = 0; i < morph.Frames.Count; i++)
					{
						File.Delete(Morpher.PROJECT_PATH + Morpher.PROJECT_NAME + "\\" + morph.MorphName + "_" + i + ".png");
					}
				}
				if ((bool)settings.performSettings.thread7Box.IsChecked)
				{
					Morpher.NumThreads = 7;
					sw.Start();
					newMorph(morph);
					sw.Stop();
					times.Add(benchMarkKeys[6], sw.Elapsed);
					for (int i = 0; i < morph.Frames.Count; i++)
					{
						File.Delete(Morpher.PROJECT_PATH + Morpher.PROJECT_NAME + "\\" + morph.MorphName + "_" + i + ".png");
					}
				}
				if ((bool)settings.performSettings.thread8Box.IsChecked)
				{
					Morpher.NumThreads = 8;
					sw.Start();
					newMorph(morph);
					sw.Stop();
					times.Add(benchMarkKeys[7], sw.Elapsed);
					for (int i = 0; i < morph.Frames.Count; i++)
					{
						File.Delete(Morpher.PROJECT_PATH + Morpher.PROJECT_NAME + "\\" + morph.MorphName + "_" + i + ".png");
					}
				}
				MessageBoxResult mbr = MessageBox.Show("Morph Ready!", "",
						MessageBoxButton.OK, MessageBoxImage.Information);
				foreach (string key in times.Keys)
				{
					double ratio = (double)times[key].Ticks / (double)times[benchMarkKeys[0]].Ticks;
					MessageBoxResult mbr2 = MessageBox.Show(key + " / 1 Thread: " + ratio, "",
						MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
		}

		private void newMorph(Morph mrph)
		{
			morpher.Mrph = mrph;
			morpher.SrcLines = srcViewer.ControlLines;
			morpher.DestLines = destViewer.ControlLines;
			morpher.setSrc((BitmapSource)srcViewer.ImageSrc);
			morpher.setDest((BitmapSource)destViewer.ImageSrc);
			morpher.startThreads();
			save();
		}
		
		private void ViewMorph_Click(object sender, RoutedEventArgs e )
		{
			if (morphViewer.morphDict.Count != 0)
			{
				srcViewer.Visibility = Visibility.Hidden;
				destViewer.Visibility = Visibility.Hidden;
				morphViewer.updateMorphs();
				morphViewer.Visibility = Visibility.Visible;
				morphViewer.Mrph = morphViewer.morphDict[newMorphDialog.MorphName];
				morphViewer.Src = srcViewer.ImageSrc;
				morphViewer.Dest = destViewer.ImageSrc;
				morphViewer.setImageSrc(morphViewer.Src);
				modeItem.Header = "Change Control Lines";
				modeItem.Click -= ViewMorph_Click;
				modeItem.Click += ChangeControlLines_Click;
				UpdateLayout();
			}
		}

		private void ChangeControlLines_Click(object sender, RoutedEventArgs e)
		{
			srcViewer.Visibility = Visibility.Visible;
			destViewer.Visibility = Visibility.Visible;
			morphViewer.Visibility = Visibility.Hidden;
			modeItem.Header = "View Morph";
			modeItem.Click += ViewMorph_Click;
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
			newProjectDialog.Close();
			newMorphDialog.Close();
		}
	}
}
