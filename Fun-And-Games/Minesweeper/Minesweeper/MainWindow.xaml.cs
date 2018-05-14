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
        const int ROWS = 6;
        const int COLS = 8;

        private MinesweeperModel gameModel = new MinesweeperModel(ROWS, COLS, 10);

        public MainWindow()
        {
            InitializeComponent();

            for (int col = 0; col < gameModel.Columns; col++)
            {
                var stk = new StackPanel();
                stk.Orientation = Orientation.Horizontal;
                StackOfRows.Children.Add(stk);
                for (int row = 0; row < gameModel.Rows; row++)
                {
                    var btn = new Button
                    {
                        Content = "-",
                        Width = 40,
                        Height = 40
                    };
                    var closureRow = row;
                    var closureCol = col;
                    //add a check for right/left click, and link to flagging behaviour
                    btn.MouseUp += (sender, e) =>
                    {
                        if (e.ChangedButton == MouseButton.Right)
                            FlagCell(closureRow, closureCol);
                    };
                    btn.Click += (sender, e) =>
                    {
                        UpdateDisplay(closureRow, closureCol);
                    };
                    stk.Children.Add(btn);
                }
            }
        }

        private void UpdateDisplay(int click_row, int click_col)
        {
            gameModel.RevealCell(click_row, click_col);
            RedrawGrid();
        }

        private void FlagCell(int click_row, int click_col)
        {
            gameModel.ToggleCellFlagType(click_row, click_col);
            RedrawGrid();
        }

        private void RedrawGrid()
        {
            for (int col = 0; col < gameModel.Columns; col++)
            {
                var currentRow = (StackPanel)StackOfRows.Children[col];
                for (int row = 0; row < gameModel.Rows; row++)
                {
                    var btn = (Button)currentRow.Children[row];
                    btn.Content = gameModel.GetUserFriendlyCellState(row, col);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            gameModel.ResetGame();
            RedrawGrid();
        }
    }
}
