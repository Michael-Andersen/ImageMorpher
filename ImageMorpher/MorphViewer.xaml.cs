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
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageMorpher
{
	/// <summary>
	/// Interaction logic for MorphViewer.xaml
	/// </summary>
	public partial class MorphViewer : UserControl
	{
		public int FrameIndex { get; set;}
		public Morpher Morph { get; set;}
		public ImageSource Src { get; set;}
		public ImageSource Dest { get; set;}
		private BitmapSource[] bs;

		public MorphViewer()
		{
			InitializeComponent();
			FrameIndex = 0;
		}

		public void setImageSrc(ImageSource im)
		{
			image.Source = im;
		}

		private async void playBtn_Click(object sender, RoutedEventArgs e)
		{
			setImageSrc(Src);
			UpdateLayout();
			await Task.Run(() =>
			{
				Thread.Sleep(25);
			});
			for (int i = 0; i < Morph.Frames.Count; i++)
			{
				setImageSrc(Morph.Frames[i]);
				UpdateLayout();
				await Task.Run(() =>
				{
					Thread.Sleep(25);
				});
			}
			setImageSrc(Dest);
			UpdateLayout();
		}
		private void prevBtn_Click(object sender, RoutedEventArgs e)
		{
			FrameIndex--;
			if (FrameIndex < 0)
			{
				setImageSrc(Src);
				FrameIndex = 0;
			}
			else
			{
				setImageSrc(Morph.Frames[FrameIndex]);
			}
			
			UpdateLayout();
		}
		private void nextBtn_Click(object sender, RoutedEventArgs e)
		{
			if (FrameIndex >= Morph.Frames.Count)
			{
				setImageSrc(Dest);
			}
			else
			{	
				setImageSrc(Morph.Frames[FrameIndex]);
				FrameIndex++;
			}
			
			UpdateLayout();
		}
		private async void reverseBtn_Click(object sender, RoutedEventArgs e)
		{
			setImageSrc(Dest);
			UpdateLayout();
			await Task.Run(() =>
			{
				Thread.Sleep(25);
			});
			for (int i = Morph.Frames.Count - 1; i >= 0; i--)
			{
				setImageSrc(Morph.Frames[i]);
				UpdateLayout();
				await Task.Run(() =>
				{
					Thread.Sleep(25);
				});
			}
			setImageSrc(Src);
			UpdateLayout();
		}
	}
}
