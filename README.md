# SudokuSolver

A program that works out the solution to a Sudoku puzzle by using human techniques and proofs.
Because it logs the human techniques it uses, you can learn how to get past obstacles you found yourself in.

It is designed with speed in mind, but there are still many improvements to be had. I'm currently just trying to write each technique before optimizing further.

The form draws the puzzle and its changes. If it gets stuck, the candidates for each cell will be shown for debugging:

![Early Preview](https://i.imgur.com/yTBUkGg.png)

Once design is done, of course, there will be no candidates showing, as every cell will be filled (assuming the input puzzle is valid and has human-solvable steps).

## To Do

1. Can change most pair/triple/quad functions into recursion because they're mostly the same but with additional for loops
2. Maybe lower the framework version
3. Building on the log, it could be possible to have an entire clickable list that shows the board through each change it made
4. Write region functions instead of the same LINQ functions everywhere in the solver