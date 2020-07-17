using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;

/*
 * SudokuSolver in C#
 * Author:  Jelle Huibregtse
 * Date:    23 September 2019
 * Version: 1.0
 * 
 * Description:
 * This is a sudoku solver using brute-force search and backtracking.
 * Backtracking is a depth-first search.
 *
 * "Although it has been established that approximately 5.96 x 1126 final grids exist,
 * a brute force algorithm can be a practical method to solve Sudoku puzzles."
 *
 * Advantages of this method are:
 * - A solution is guaranteed (as long as the puzzle is valid).
 * - Solving time is mostly unrelated to degree of difficulty.
 * - The algorithm (and therefore the program code) is simpler than other algorithms,
 * especially compared to strong algorithms that ensure a solution to the most difficult puzzles.
 * 
 * Source: https://en.wikipedia.org/wiki/Sudoku_solving_algorithms
 */

namespace SudokuSolver
{
    class Program
    {
        // Debug stopwatch to get elapsed time
        Stopwatch stopWatch;

        // The size of the sudoku.
        readonly int SudokuSize = 9;
        // The minimal and maximum of a cell input for the sudoku.
        readonly int SudokuMin = 1;
        readonly int SudokuMax = 9;
        // The size of a box inside the sudoku.
        readonly int SudokuBoxSize = 3;

        // 2D int[] representing the grid, 0 means empty.

        // An very easy sudoku.
        // Source: https://www.sudokukingdom.com/very-easy-sudoku.php (modified)
        int[,] sudoku1 = {
            {0, 0, 0, 0, 0, 0, 0, 0, 0},
            {4, 2, 8, 0, 0, 0, 1, 0, 7},
            {0, 0, 3, 1, 8, 6, 0, 0, 2},
            {9, 0, 0, 6, 0, 0, 2, 0, 8},
            {0, 0, 0, 0, 0, 0, 0, 0, 0},
            {2, 6, 0, 0, 5, 8, 0, 0, 4},
            {0, 1, 0, 2, 0, 7, 3, 4, 0},
            {3, 0, 9, 0, 1, 5, 0, 0, 0},
            {0, 7, 0, 0, 9, 0, 5, 8, 1},
        };

        // An extremely difficult sudoku.
        // Source: https://www.extremesudoku.info/sudoku.html/
        int[,] sudoku2 = {
            {6, 0, 4, 0, 0, 5, 9, 0, 8},
            {0, 7, 0, 0, 8, 0, 0, 4, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 9, 0, 0, 0, 0, 0, 6},
            {0, 2, 0, 0, 4, 0, 0, 1, 0},
            {3, 0, 0, 0, 0, 0, 5, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 3, 0, 0, 7, 0, 0, 2, 0},
            {9, 0, 6, 4, 0, 0, 1, 0, 7},
        };

        // World's hardest sudoku according to the Telegraph
        // Source: https://www.telegraph.co.uk/news/science/science-news/9359579/Worlds-hardest-sudoku-can-you-crack-it.html
        int[,] sudoku3 = {
            {8, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 3, 6, 0, 0, 0, 0, 0},
            {0, 7, 0, 0, 9, 0, 2, 0, 0},
            {0, 5, 0, 0, 0, 7, 0, 0, 0},
            {0, 0, 0, 0, 4, 5, 7, 0, 0},
            {0, 0, 0, 1, 0, 0, 0, 3, 0},
            {0, 0, 1, 0, 0, 0, 0, 6, 8},
            {0, 0, 8, 5, 0, 0, 0, 1, 0},
            {0, 9, 0, 0, 0, 0, 4, 0, 0},
        };

        // Stores all the possible solutions for the given sudoku.
        List<int[,]> solutions = new List<int[,]>();

        // Stores how many solutions there are.
        int solutionCounter;

        // Returns if a value at (row, column), gives conflict.
        bool GivesConflict(int[,] grid, int row, int column, int value)
        {
            return RowConflict(grid, row, value) ||
                ColumnConflict(grid, column, value) ||
                BoxConflict(grid, row, column, value);
        }

        // Returns if a value at row, gives conflict.
        bool RowConflict(int[,] grid, int row, int value)
        {
            for (int i = 0; i < SudokuSize; i++)
            {
                if (grid[row, i] == value)
                {
                    return true;
                }
            }
            return false;
        }

        // Returns if a value at column, gives conflict.
        bool ColumnConflict(int[,] grid, int column, int value)
        {
            for (int i = 0; i < SudokuSize; i++)
            {
                if (grid[i, column] == value)
                {
                    return true;
                }
            }
            return false;
        }

        // Returns if a value in box at (row, column), gives conflict.
        bool BoxConflict(int[,] grid, int row, int column, int value)
        {
            // First we have to get the top left of the box.
            // Then we can iterate through the rows and columns of the box.
            int[] boxTopLeft = { row - (row % SudokuBoxSize), column - (column % SudokuBoxSize) };
            for (int i = boxTopLeft[0]; i < boxTopLeft[0] + SudokuBoxSize; i++)
            {
                for (int j = boxTopLeft[1]; j < boxTopLeft[1] + SudokuBoxSize; j++)
                {
                    if (grid[i, j] == value)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        void Print(int[,] grid)
        {
            // First, print the top of the grid (for aesthetic purposes).
            Console.WriteLine("+-----------------+");
            // Iterate from 0 to 9 (so 9 times), vertically through the grid. 
            for (int i = 0; i < SudokuSize; i++)
            {
                // Each line of the grid starts with a | (for aesthetic purposes)
                Console.Write("|");
                // Iterate from 0 to 9 (so 9 times), horizontally through the grid.
                for (int j = 0; j < SudokuSize; j++)
                {
                    // Here handle the cell type printing.
                    if (grid[i, j] == 0)
                    {
                        // We check if we are at the end of a box (for aesthetic purposes).
                        Console.Write((j + 1) % SudokuBoxSize == 0 ? " " : "  ");
                    }
                    else
                    {
                        // The same if not empty, but here fill the cell with the actual value.
                        Console.Write((j + 1) % SudokuBoxSize == 0 ? "{0}" : "{0} ", grid[i, j]);
                    }
                    // Here we handle the divider for each box (for aesthetic purposes).
                    if ((j + 1) % SudokuBoxSize == 0 && (j + 1) != SudokuSize)
                    {
                        Console.Write("|");
                    }
                }
                // If we reach the vertical end of the box add another horizontal divider.
                if ((i + 1) % SudokuBoxSize == 0 && (i + 1) != SudokuSize)
                {
                    Console.Write("|\n|-----------------");
                }
                // Always end a line with a "|" (for aesthetic purposes).
                Console.Write("|\n");
            }
            // Finally, we print the bottom of the grid (for aesthetic purposes).
            Console.Write("+-----------------+\n");
        }

        // Returns an int[] with (row, column) of the next empty cell, null otherwise.
        int[] FindEmptyCell(int[,] grid)
        {
            for (int i = 0; i < SudokuSize; i++)
            {
                for (int j = 0; j < SudokuSize; j++)
                {
                    // 0 signifies an empty cell.
                    if (grid[i, j] == 0)
                    {
                        return new int[] { i, j };
                    }
                }
            }
            return null;
        }

        void Solve(int[,] grid)
        {
            // Put the location of the next empty cell in an array.
            int[] nextEmptyCell = FindEmptyCell(grid);
            int row = nextEmptyCell[0];
            int column = nextEmptyCell[1];

            // Try number 1 till 9 and check for conflict
            for (int i = SudokuMin; i <= SudokuMax; i++)
            {
                if (!GivesConflict(grid, row, column, i))
                {
                    grid[row, column] = i;

                    // If there is no empty cell, sudoku is solved.
                    if (FindEmptyCell(grid) == null)
                    {
                        solutionCounter++;

                        int[,] solution = new int[SudokuSize, SudokuSize];
                        for (int j = 0; j < SudokuSize; j++)
                        {
                            for (int k = 0; k < SudokuSize; k++)
                            {
                                solution[j, k] = grid[j, k];
                            }
                        }
                        // Add the solution to the list.
                        solutions.Add(solution);
                    }
                    else
                    {
                        // Solve the sudoku recursively.
                        Solve(grid);
                    }
                }
            }
            // Backtracking
            grid[row, column] = 0;
        }

        string GetElapsedTime(TimeSpan ts)
        {
            // String in HH/MM/SS/MS
            return String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
        }

        void Run()
        {
            // Create a new instance of the StopWatch class.
            stopWatch = new Stopwatch();
            stopWatch.Start();

            Solve(sudoku3);

            // Print the time elapsed in a specific format.
            stopWatch.Stop();
            Console.WriteLine("Time elapsed (Run time): {0}", GetElapsedTime(stopWatch.Elapsed));

            Console.WriteLine("Number of solutions: " + solutionCounter);

            foreach (int[,] solution in solutions)
            {
                Print(solution);
            }

            // Make sure the window does not instantly close.
            Console.WriteLine("\nPress any key to continue.");
            Console.ReadKey();
        }
        static void Main(string[] args)
        {
            new Program().Run();
        }
    }
}