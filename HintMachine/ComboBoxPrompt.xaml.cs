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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HintMachine
{
    /// <summary>
    /// Interaction logic for ComboBoxPrompt.xaml
    /// </summary>
    public partial class ComboBoxPrompt : Window
    {
        public ComboBoxPrompt(string prompt, List<string> comboOptions)
        {
            InitializeComponent();
            Prompt.Content = prompt;
            ComboMain.ItemsSource = comboOptions;
        }

        private void Confirmed(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
