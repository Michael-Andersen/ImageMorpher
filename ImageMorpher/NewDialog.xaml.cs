using System;
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
	/// Interaction logic for NewDialog.xaml
	/// </summary>
	public partial class NewDialog : Window
	{
		public string MorphName { get; set; }

		public bool OwnerClosing { get; set; } = false;

		public NewDialog()
		{
			InitializeComponent();
		}

		public delegate void Ok_Handler(object sender, RoutedEventArgs args);

		public event Ok_Handler OkEvent;

		public void Ok_Click(object sender, RoutedEventArgs e)
		{
			OkEvent(sender, e);
		}

		public void Cancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
