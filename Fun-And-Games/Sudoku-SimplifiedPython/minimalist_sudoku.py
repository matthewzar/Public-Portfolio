from tkinter import Label, Tk, Entry, Button, END
from functools import partial
import copy

class Window(Tk):
    def __init__(self,parent):
        '''constructor'''
        parent.title("Sudoku")
        self.load_puzzle("puzzle.txt")       
        self.parent = parent
        self.make_ui()

    def load_puzzle(self, puzzleAddress):
        file = open(puzzleAddress, mode='r')
        self.puzzle = []
        row_num = 0        
        for row_string in file:
            string_list = row_string.split(",")
            self.puzzle.append(list(map(int, string_list)))

        self.current_solution = copy.deepcopy(self.puzzle)
        file.close()
        
    def make_ui(self):
        '''
        Adds a grid of buttons to the window.
        Each button will get it's own on-click partial command (this lets each button track it's XY coords).
        Formatting labels are also added here
        '''
        self.buttonArray = [[] for i in range(9)]
        
        for r in range(11):
            if(r % 4 == 3):
                self.add_formatting_row(r)
                continue
            
            for col in range(11):
                if(col % 4 == 3):                    
                    divider = Label(self.parent, text="|")
                    divider.grid(row = r, column = col)
                    continue

                sudoku_row = r - r//4;
                sudoku_col = col - col//4

                newButton = self.get_new_button(sudoku_row, sudoku_col)
                self.buttonArray[sudoku_row].append(newButton)
                newButton.grid(row = r, column = col)
                
    def get_new_button(self, s_row, s_col):
        '''
        Creates a new button based on the current coords, and the puzzle's content
        '''
        action_with_arg = partial(self.on_click, s_row, s_col)

        puzzle_num = self.puzzle[s_row][s_col]
        text_to_display = " " if puzzle_num == 0 else str(puzzle_num)

        #Added a known default colour
        newButton = Button(self.parent, text=text_to_display, height = 1, width = 1, bg = "lightgrey")
        
        newButton.bind("<Button-1>", action_with_arg)
        newButton.bind("<Button-3>", action_with_arg)
       
        return newButton

    def on_click(self, row, col, button_press_args):
        if(self.puzzle[row][col] != 0):
            print("That button is locked, and can't be changed")
            return
        
        if(button_press_args.num == 1):
            self.change_number(row, col, 1)            
        elif(button_press_args.num == 3):
            self.change_number(row, col, -1)

    def change_number(self, row, col, incOrDec):
        '''
        Gets the available numbers for a clicked cell, and then either iterates up or down
        through that list of available numbers.
        '''        
        clickedNumber = self.current_solution[row][col]
        numbers = self.get_available_numbers(row,col)       

        #Check of only the 0 option is available, and if so, make it red.
        if(len(numbers) == 1):
            print("HERE")
            self.buttonArray[row][col].configure(bg = "red")
            return
        else:
            self.buttonArray[row][col].configure(bg = "lightgrey")

        if(clickedNumber == 0):
            newIndex = 0 if incOrDec == 1 else -2
        else:
            startIndex = numbers.index(clickedNumber)
            newIndex = startIndex + incOrDec      
        
        newNum = numbers[newIndex]
        self.current_solution[row][col] = newNum
        
        if(newNum == 0):
            self.buttonArray[row][col]["text"] = ""
        else:
            self.buttonArray[row][col]["text"] = str(newNum)

        if(self.is_win()):
            self.buttonArray[row][col].configure(bg = "green")            
            print("YOU WIN")
            return

        #Check if the most recent change makes this a win:

    def get_available_numbers(self, row, col):
        '''
        Looks at the row, col, and box for a given cell,
        and return any numbers that aren't present in any of them. The result includes the clicked number
        The results are based on current_solution (not self.puzzle)
        '''        
        block_nums = []

        for y in range(row-row%3, row-row%3 + 3):
            for x in range(col-col%3, col-col%3 + 3):
                if(y==row and x==col):
                    continue
                block_nums.append(self.current_solution[y][x])
        
        col_nums = []
        row_nums = []
        for i in range(9):
            if(i!=col):                
                row_nums.append(self.current_solution[row][i])
            if(i!=row):
                col_nums.append(self.current_solution[i][col])

        ret = []
        for num in range(1,10):
            if (num in col_nums or
                num in row_nums or
                num in block_nums):
                continue
            ret.append(num)
            
        ret.append(0)
        return ret
        
    def is_win(self):
        '''
        Checks there is a single zero left in the solution, of not it returns true
        '''
        for row in self.current_solution:
            for num in row:
                if(num == 0):
                    return False
        return True

    def add_formatting_row(self, rowNumber):
        '''
        Creates an entire row of dividing symbols.
        Every 4th symbol is a plus.
        Dividing labels are placed on the parent grid.
        '''
        
        for col in range(11):
            if(col == 3 or col == 7):
                divider = Label(self.parent, text="+")
            else:
                divider = Label(self.parent, text="-")            
            divider.grid(row = rowNumber, column = col)

root = Tk()
my_gui = Window(root)
root.mainloop()
