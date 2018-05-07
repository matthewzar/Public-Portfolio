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

            var temp = new MinesweeperModel(12, 10, 20);
            Console.WriteLine(temp.ToString());

            temp.RevealCell(0, 0);
            Console.WriteLine();
            Console.WriteLine(temp.ToString());

            temp.RevealCell(1, 1);
            Console.WriteLine();
            Console.WriteLine(temp.ToString());


            temp.RevealCell(2, 2);
            Console.WriteLine();
            Console.WriteLine(temp.ToString());
        }
    }
}
