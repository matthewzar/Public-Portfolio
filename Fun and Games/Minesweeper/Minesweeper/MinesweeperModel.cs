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
        exposed = ' ',
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

    public enum GameState
    {
        NotStarted,
        Started,
        Win,
        Loss
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

        public GameState CurrentState { get; private set; }

        private readonly Random rng = new Random();

        public int TotalMines { get; private set; }
        
        public int Columns { get; private set; }
        public int Rows { get; private set; }

        public MinesweeperModel(int rowCount, int columnCount, int totalMines)
        {
            if (rowCount * columnCount <= totalMines) throw new ArgumentException($"Cannot fit {totalMines} (and an opening click space) in {rowCount * columnCount} spaces");

            TotalMines = totalMines;
            Columns = rowCount;
            Rows = columnCount;
            ResetGame();
        }

        /// <summary>
        /// Tell the model to start a new game of the same difficulty.
        /// </summary>
        public void ResetGame()
        {
            CurrentState = GameState.NotStarted;
            InitialiseEmptyGrid();
        }

        private void PopulateGameGrids(int openingRow, int openingCol)
        {
            CurrentState = GameState.Started;
            InitialiseEmptyGrid();
            PopulateGrid(openingRow, openingCol);
            CountNeighbors();
        }

        private void InitialiseEmptyGrid()
        {
            neighboringMineCounts = new int[Rows, Columns];
            solution = new TrueCellState[Rows, Columns];
            currentDisplay = new DisplayedCellState[Rows, Columns];

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    solution[i, j] = TrueCellState.empty;
                    currentDisplay[i, j] = DisplayedCellState.untouched;
                }
            }
        }

        private void PopulateGrid(int openingRow, int openingCol)
        {
            int addedMines = 0;
            while (addedMines < TotalMines)
            {
                int row = rng.Next(Rows);
                int col = rng.Next(Columns);

                //Is it an already used space, or one that will kill the player on their first move?
                if(solution[row,col] == TrueCellState.mine ||
                   row == openingRow && col == openingCol) continue;

                solution[row, col] = TrueCellState.mine;
                addedMines++;
            }
        }

        private void CountNeighbors()
        {
            for (var i = 0; i < Rows; i++)
            {
                for (var j = 0; j < Columns; j++)
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
            int rowBound = row == Rows - 1 ? 0 : 1;
            int colBound = col == Columns - 1 ? 0 : 1;
            for (var i = row == 0 ? 0 : -1; i <= rowBound; i++)
            {
                for (var j = col == 0 ? 0 : -1; j <= colBound; j++)
                {
                    if (i == 0 && j == 0) continue;
                    yield return new Tuple<int, int>(row+i, col+j);
                }
            }
        }

        /// <summary>
        /// Get the result of revealing a cell. Note that if this is the first move of a game, the layout
        /// will be created here (a tiny delay may occur, but only for HUGE games).
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public DisplayedCellState RevealCell(int row, int col)
        {
            if (CurrentState == GameState.NotStarted) PopulateGameGrids(row, col);

            //Can't get here in a notStarted state
            //Check if we are either in an end-game state or clicked a revealed cell
            if (CurrentState != GameState.Started || currentDisplay[row, col] != DisplayedCellState.untouched) return currentDisplay[row, col];

            if (solution[row, col] == TrueCellState.mine)
            {
                currentDisplay[row, col] = DisplayedCellState.exploded;
                CurrentState = GameState.Loss;
                ExposeExplodedBoard();
                return DisplayedCellState.exploded;
            }
            else
            {
                //TODO - add a check for (total remaining unrevealable cells + the placed flags) == TotalMines
                //TODO - if true, set the CurrentState to win
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
            if (CurrentState != GameState.Started) return false;

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

        public char GetUserFriendlyCellState(int row, int col)
        {
            if (currentDisplay[row, col] == DisplayedCellState.exposed)
                return neighboringMineCounts[row, col] == 0 ? ' ' : neighboringMineCounts[row, col].ToString()[0];
            return (char)currentDisplay[row, col];
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
