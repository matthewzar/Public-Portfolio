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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Minesweeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var temp = new MinesweeperModel(12, 10, 50);
            Console.WriteLine(temp.ToString());

            temp.ToggleCellFlagType(0, 0);
            temp.ToggleCellFlagType(0, 0);
            temp.RevealCell(0, 1);
            Console.WriteLine();
            Console.WriteLine(temp.ToString());

            for (int i = 1; i < 5; i++)
            {
                temp.RevealCell(i, i);
                Console.WriteLine();
                Console.WriteLine(temp.ToString());

            }
        }
    }
}
