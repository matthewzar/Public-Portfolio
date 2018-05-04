using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper
{
    public enum DisplayedCellState
    {
        untouched = 'O',
        unkown = '?',
        exposed = 'E',
        flagged = '!',
        exploded = 'X'
    }

    public enum TrueCellState
    {
        mine,
        empty,
    }

    public class MinesweeperModel
    {
        /// <summary>
        /// The total number of neigboring mines for each cell
        /// Calculated once when the model is generated to prevent re-computing neighbors on every update
        /// </summary>
        private int[,] neighboringMineCounts;

        /// <summary>
        /// The true layout of the board, hidden to the player 
        /// </summary>
        private TrueCellState[,] solution;

        /// <summary>
        /// What is being shown to the player
        /// </summary>
        private DisplayedCellState[,] currentDisplay;

        private Random rng = new Random();

        public int TotalMines { private get; set; }

        public MinesweeperModel(int rowCount, int columnCount, int totalMines)
        {
            if(rowCount * columnCount < totalMines) throw new ArgumentException($"Cannot fit {totalMines} in {rowCount * columnCount} spaces");

            TotalMines = totalMines;
            InitialiseEmptyGrid(rowCount, columnCount);
            PopulateGrid();
            CountNeighbors();
        }

        private void InitialiseEmptyGrid(int rowCount, int columnCount)
        {
            neighboringMineCounts = new int[rowCount, columnCount];
            solution = new TrueCellState[rowCount, columnCount];
            currentDisplay = new DisplayedCellState[rowCount, columnCount];

            for (int i = 0; i < solution.GetLength(0); i++)
            {
                for (int j = 0; j < solution.GetLength(1); j++)
                {
                    solution[i, j] = TrueCellState.empty;
                    currentDisplay[i, j] = DisplayedCellState.untouched;
                }
            }
        }

        private void PopulateGrid()
        {
            int addedMines = 0;
            while (addedMines < TotalMines)
            {
                int row = rng.Next(solution.GetLength(0));
                int col = rng.Next(solution.GetLength(1));
                if(solution[row,col] == TrueCellState.mine) continue;
                solution[row, col] = TrueCellState.mine;
                addedMines++;
            }
        }

        private void CountNeighbors()
        {
            for (int i = 0; i < solution.GetLength(0); i++)
            {
                for (int j = 0; j < solution.GetLength(1); j++)
                {
                    neighboringMineCounts[i,j] = CountSingleCellsNeighbors(i, j);
                }
            }
        }

        private int CountSingleCellsNeighbors(int row, int col)
        {
            int total = 0;
            foreach (var posPair in GetLegalCellNeighbors(row,col))
            {
                if(solution[posPair.Item1, posPair.Item2] == TrueCellState.empty) continue;
                total++;
            }

            return total;
        }

        private IEnumerable<Tuple<int, int>> GetLegalCellNeighbors(int row, int col)
        {
            int rowBound = row == solution.GetLength(0) - 1 ? 0 : 1;
            int colBound = col == solution.GetLength(1) - 1 ? 0 : 1;
            for (int i = row == 0 ? 0 : -1; i <= rowBound; i++)
            {
                for (int j = col == 0 ? 0 : -1; j <= colBound; j++)
                {
                    if (i == 0 && j == 0) continue;
                    yield return new Tuple<int, int>(row+i, col+j);
                }
            }
        }

        public DisplayedCellState RevealCell(int row, int col)
        {
            if (currentDisplay[row, col] != DisplayedCellState.untouched) return currentDisplay[row, col];

            if (solution[row, col] == TrueCellState.mine)
            {
                currentDisplay[row, col] = DisplayedCellState.exploded;
                return DisplayedCellState.exploded;
            }

            currentDisplay[row, col] = DisplayedCellState.exposed;
            ExposeLinkedZeroNeighbors(row, col);
            return DisplayedCellState.exposed;
        }

        private void ExposeLinkedZeroNeighbors(int row, int col)
        {
            if (neighboringMineCounts[row, col] != 0) return;

            foreach (var posPair in GetLegalCellNeighbors(row, col))
            {
                if (currentDisplay[posPair.Item1, posPair.Item2] == DisplayedCellState.exposed) continue;
                currentDisplay[posPair.Item1, posPair.Item2] = DisplayedCellState.exposed;
                ExposeLinkedZeroNeighbors(posPair.Item1, posPair.Item2);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < currentDisplay.GetLength(0); i++)
            {
                for (int j = 0; j < currentDisplay.GetLength(1); j++)
                {
                    if (currentDisplay[i, j] == DisplayedCellState.exposed)
                    {
                        sb.Append(neighboringMineCounts[i, j]);
                    }
                    else sb.Append((char)currentDisplay[i, j]);
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
