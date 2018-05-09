using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Minesweeper
{
    public enum DisplayedCellState
    {
        untouched = '-',
        unsure = '?',
        exposed = '_',
        flagged = 'F',
        exploded = 'E',
        incorrectFlag = 'I',
        unexplodedMine = 'M'
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

        private readonly Random rng = new Random();

        public int TotalMines { private get; set; }
        public bool GameOver { private get; set; }

        public MinesweeperModel(int rowCount, int columnCount, int totalMines)
        {
            StartNewGame(rowCount, columnCount, totalMines);
        }

        /// <summary>
        /// Resets the model to a new game state.
        /// </summary>
        /// <param name="rowCount"></param>
        /// <param name="columnCount"></param>
        /// <param name="totalMines"></param>
        public void StartNewGame(int rowCount, int columnCount, int totalMines)
        {
            if (rowCount * columnCount < totalMines) throw new ArgumentException($"Cannot fit {totalMines} in {rowCount * columnCount} spaces");

            TotalMines = totalMines;
            GameOver = false;
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
            if (GameOver || currentDisplay[row, col] != DisplayedCellState.untouched) return currentDisplay[row, col];

            if (solution[row, col] == TrueCellState.mine)
            {
                currentDisplay[row, col] = DisplayedCellState.exploded;
                GameOver = true;
                ExposeExplodedBoard();
                return DisplayedCellState.exploded;
            }

            currentDisplay[row, col] = DisplayedCellState.exposed;
            ExposeLinkedZeroNeighbors(row, col);
            return DisplayedCellState.exposed;
        }

        /// <summary>
        /// Toggles an unexposed cell between: unexposed, flagged, and unsure (in that order)
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns>Returns true if the cell was in a changable state</returns>
        public bool ToggleCellFlagType(int row, int col)
        {
            if (GameOver) return false;

            var transitions = new Dictionary<DisplayedCellState, DisplayedCellState>
            {
                {DisplayedCellState.untouched, DisplayedCellState.flagged},
                {DisplayedCellState.flagged, DisplayedCellState.unsure},
                {DisplayedCellState.unsure, DisplayedCellState.untouched},
            };
            var startState = currentDisplay[row, col];
            if (transitions.ContainsKey(startState))
            {
                currentDisplay[row, col] = transitions[startState];
                return true;
            }

            return false;
        }

        private void ExposeExplodedBoard()
        {
            for (int i = 0; i < currentDisplay.GetLength(0); i++)
            {
                for (int j = 0; j < currentDisplay.GetLength(1); j++)
                {
                    var cellShows = currentDisplay[i, j];
                    var cellSolution = solution[i, j];
                    if (cellSolution == TrueCellState.mine)
                    {
                        if (cellShows == DisplayedCellState.unsure ||
                            cellShows == DisplayedCellState.untouched)
                            currentDisplay[i, j] = DisplayedCellState.unexplodedMine;
                    }
                    else
                    {
                        if (cellShows == DisplayedCellState.flagged)
                            currentDisplay[i, j] = DisplayedCellState.incorrectFlag;
                    }
                }
            }
        }

        public DisplayedCellState GetCellState(int row, int col)
        {
            return currentDisplay[row, col];
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
