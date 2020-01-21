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
		public static int FrameRate { get; set; }
		public Morpher Morph { get; set;}
		public ImageSource Src { get; set;}
		public ImageSource Dest { get; set;}
		public bool playing = false;
		public bool reversing = false;

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
			if (!playing)
			{
				playBtn.Content = "Pause";
				playing = true;
				//setImageSrc(Src);
				UpdateLayout();
				await Task.Run(() =>
				{
					Thread.Sleep(1000 / FrameRate);
				});
				for (; FrameIndex < Morph.Frames.Count; FrameIndex++)
				{
					if (playing) {
						setImageSrc(Morph.Frames[FrameIndex]);
						UpdateLayout();
						await Task.Run(() =>
						{
							Thread.Sleep(1000 / FrameRate);
						}); }
					else {
						break;
					}
				}
				if (playing) {
					FrameIndex = Morph.Frames.Count - 1;
					setImageSrc(Dest);
					UpdateLayout();
					playBtn.Content = "Play";
				}
			} else
			{
				playBtn.Content = "Play";
				playing = false;
			}
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
		private void startBtn_Click(object sender, RoutedEventArgs e)
		{
			FrameIndex = 0;
			setImageSrc(Src);
			UpdateLayout();
		}
		private void endBtn_Click(object sender, RoutedEventArgs e)
		{
			FrameIndex = Morph.Frames.Count - 1;
			setImageSrc(Dest);
			UpdateLayout();
		}
		private async void reverseBtn_Click(object sender, RoutedEventArgs e)
		{
			if (!reversing) {
				reverseBtn.Content = "Pause";
				reversing = true;
				//setImageSrc(Dest);
				UpdateLayout();
				await Task.Run(() =>
				{
					Thread.Sleep(1000 / FrameRate);
				});
				for (; FrameIndex >= 0; FrameIndex--)
				{
					if (reversing)
					{
						setImageSrc(Morph.Frames[FrameIndex]);
						UpdateLayout();
						await Task.Run(() =>
						{
							Thread.Sleep(1000 / FrameRate);
						});
					} else
					{
						break;
					}
				}
				
				if (reversing)
				{
					FrameIndex = 0;
					setImageSrc(Src);
					reverseBtn.Content = "Reverse";
					UpdateLayout();
				}
			}
			 else
			{
				reverseBtn.Content = "Reverse";
				reversing = false;
			}
		}
	}
}
