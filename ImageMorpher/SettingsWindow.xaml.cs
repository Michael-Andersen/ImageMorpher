﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{

		public bool OwnerClosing { get; set; } = false;


		public SettingsWindow(ImageViewer srcViewer, ImageViewer destViewer)
		{
			InitializeComponent();
			clSettings.SrcViewer = srcViewer;
			clSettings.DestViewer = destViewer;
		}

		private void Settings_Closing(object sender, CancelEventArgs e)
		{
			if (!OwnerClosing)
			{
				e.Cancel = true;
				Hide();
			}
		}
	}
}
