using NexusBuddyHades.Tools;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NexusBuddyHades
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //private void ButtonAddName_Click(object sender, RoutedEventArgs e)
        //{
        //    if (!string.IsNullOrWhiteSpace(txtName.Text) && !lstNames.Items.Contains(txtName.Text))
        //    {
        //        lstNames.Items.Add(txtName.Text);
        //        txtName.Clear();
        //    }
        //}

        private void ExtractLz4_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".lz4";
            dlg.Filter = "LZ4 Files (*.lz4)|*.lz4";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                var lzConverter = new Lz();
                Lz.Main(dlg.FileName, "-d");
                // Open document 
                //textBox1.Text = dlg.FileName;
            }
        }

        private void compressLz4_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".gr2";
            dlg.Filter = "GR2 Files (*.gr2)|*.gr2";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                var lzConverter = new Lz();
                Lz.Main(dlg.FileName, "-c");
                // Open document 
                //textBox1.Text = dlg.FileName;
            }
        }
    }
}