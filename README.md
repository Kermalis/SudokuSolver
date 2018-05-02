# SudokuSolver

A program that works out the solution to a sudoku puzzle by using human techniques and proofs.

It is designed with speed in mind, but there are still many improvements to be had. I'm currently just trying to write each technique before optimizing further.

The form draws the puzzle and its changes. If it gets stuck, the candidates for each cell will be shown for debugging:

![Early Preview](https://i.imgur.com/jQpsa8P.png)

Once design is done, of course, there will be no candidates showing, as every cell will be filled (assuming the input puzzle is valid).

## Todo

1. Hidden Triples & Hidden Quads

   The hidden triple puzzle currently included is solved without using the hidden triple technique  
   Hidden quads are not working fully; I did not write a proper function yet
2. Should keep row/column/block points in an array instead of generating these arrays several times per loop
3. Can change most pair/triple/quad functions into recursion because they're mostly the same but with additional for loops
4. Fix candidates for set cells; right now it empties on each loop and that's not efficient
5. Find the name of the technique I use after I check for hidden singles
6. Maybe lower the framework version