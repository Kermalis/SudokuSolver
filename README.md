# SudokuSolver

A program that works out the solution to a Sudoku puzzle by using human techniques and proofs.
Because it logs the human techniques it uses, you can learn how to get past obstacles you found yourself in.

It is designed with speed in mind, but there are still many improvements to be had. I'm currently just trying to write each technique before optimizing further.

The form draws the puzzle and its changes. If it gets stuck, the candidates for each cell will be shown for debugging:

![Preview](https://i.imgur.com/7Yb2xNP.png)

Once design is done, of course, there will be no candidates showing, as every cell will be filled (assuming the input puzzle is valid and has human-solvable steps).

Big thanks to http://www.sudokuwiki.org for providing a lot of information on tough Sudoku techniques.

## To Do

1. Maybe lower the framework version
2. Building on the log, it could be possible to have an entire clickable list that shows the board through each change it made

   This would slow down the solver and increase memory usage, so disabling logging should be an option
3. Find a hidden triple puzzle that this program actually uses the hidden triple technique on
4. A way to toggle techniques with the UI