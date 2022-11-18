# SudokuSolver

A program that works out the solution to a Sudoku puzzle by using human techniques and proofs.
Because it logs the human techniques it uses, you can learn how to get past obstacles you found yourself in.

![Success](Showcase/Success.gif)

It is designed with speed in mind, but there are still many improvements to be had.
The program has been updated to .NET 7.0, but I'm not actively working on the program.
The goal was to write each technique before optimizing further.

The form draws the puzzle and its changes.
If it gets stuck, the candidates for each cell will be shown for debugging:

![Fail](Showcase/Fail.png)

Once design is done, of course, there will be no candidates showing, as every cell will be filled (assuming the input puzzle is valid and has human-solvable steps).

Big thanks to http://hodoku.sourceforge.net and http://www.sudokuwiki.org for providing a lot of information on tough Sudoku techniques.

## To Do

* A way to toggle techniques and logging with the UI
* Remaining techniques
* Unit tests