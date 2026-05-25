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

namespace ReadAndWrite
{
    public partial class FreezeAppealDialog : Window
    {
        public string Result { get; private set; }

        public FreezeAppealDialog()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Result = null;
            Close();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            Result = TbReason.Text.Trim();
            Close();
        }
    }
}