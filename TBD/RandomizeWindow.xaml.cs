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

namespace TBD
{
    /// <summary>
    /// Interaction logic for RandomizeWindow.xaml
    /// </summary>
    public partial class RandomizeWindow : Window
    {
        public int numberOfActions = 0;

        public RandomizeWindow()
        {
            InitializeComponent();
        }

        public RandomizeWindow(int numberOfActions)
        {
            InitializeComponent();

            if (numberOfActions == 0) return;

            this.numberOfActions = numberOfActions;

            TextBoxNumber.Text = numberOfActions.ToString();
        }

        private void ButtonRandomize_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBoxNumber.Text))
            {
                ShowErrorDialog();
                return; // nothing on the textbox
            }
                

            if (Int32.TryParse(TextBoxNumber.Text, out numberOfActions))
            { 
                if (numberOfActions <= 0)
                {
                    ShowErrorDialog();
                    return;
                }
            }
            else
            {
                ShowErrorDialog();
                return;
            }

            this.Close();
        }

        private void ShowErrorDialog()
        {
            string error = "There is a problem with the input!";
            MessageBox.Show(error, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            numberOfActions = 0;
            this.Close();
        }
    }
}
