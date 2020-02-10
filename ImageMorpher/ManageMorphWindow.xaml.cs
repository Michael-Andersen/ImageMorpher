using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using System.Windows.Shapes;

namespace ImageMorpher
{
	/// <summary>
	/// Interaction logic for ManageMorphWindow.xaml
	/// </summary>
	public partial class ManageMorphWindow : Window
	{
		public Dictionary<string, Morph> morphDict;
		public Morph toBeDeleted;

		public ManageMorphWindow(Dictionary<string, Morph> mrphDict)
		{
			this.morphDict = mrphDict;
			InitializeComponent();
			morphComboBox.ItemsSource = mrphDict.Keys;
			this.morphDict = mrphDict;
		}

		private void MorphComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (morphComboBox.SelectedIndex > 0 && morphComboBox.SelectedIndex < morphDict.Count)
			{
				toBeDeleted = morphDict[(string)(morphComboBox.SelectedValue)];
			}
			
		}

		private void deleteBtn_Click(object sender, RoutedEventArgs e)
		{
			for (int i = 0; i < toBeDeleted.Frames.Count; i++)
			{
				File.Delete(Morpher.PROJECT_PATH + Morpher.PROJECT_NAME + "\\" + toBeDeleted.MorphName + "_" + i + ".png");
			}
			morphDict.Remove(toBeDeleted.MorphName);
			morphComboBox.ItemsSource = new List<string>();
			morphComboBox.ItemsSource = morphDict.Keys;
			morphComboBox.SelectedIndex = -1;
			UpdateLayout();
		}

		private void ManageMorphs_Closing(object sender, CancelEventArgs e)
		{
			this.DialogResult = true;
		}
	}
}
