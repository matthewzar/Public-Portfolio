# Minesweeper
This 2 hour project was meant as an MVC design exercise. The final product is a working Minesweeper game, with good separation of concerns. 
## Seperation of Concerns
```MinesweeperModel.cs``` contains the stand-alone game logic. To prove it's independence I created both GUI and console versions of the game without changes to the Model.
The lines between the Control and View aspects are a bit blurred, and are housed in ```MainWindow```
## UI
As the goal of this project was design, not UX, the interface is very minimalist. Although there is some fun dynamic component creation to facilitate any sized game.
The game board is created using nested ```StackPanels``` for uniformity of layout.
