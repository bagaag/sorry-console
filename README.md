# Sorry Console

This is a terminal/console implementation of the popular board game, Sorry. It requires .Net Core. It has been tested on both Windows 10 and Arch Linux. 

To use:

    git clone https://github.com/wiseley/sorry-console.git
    cd sorry-console
    cd SorryConsole
    dotnet restore
    dotnet run

From there, you'll see:

	Welcome to Sorry
	Enter 2, 3 or 4 to specify number of players.
	You can also specify color names as Yellow,Green,Red,Blue.
	> 2

	Enter 'help' for a list of valid commands.

	 Y---------------------------------G
	 | . > - - o . . . . > - - - o . . |
	 | .   |  Y Y                    v |
	 | o   |  Y Y      + + - - - - - | |
	 | |   |           + +           | |
	 | |   |                     G G o |
	 | |   |                     G G . |
	 | ^  + +    Player: Y           . |
	 | .  + +      Card: 8           . |
	 | .                        + +  . |
	 | .                        + +  v |
	 | . + +                     |   | |
	 | o + +                     |   | |
	 | |           + +           |   | |
	 | | - - - - - + +      + +  |   o |
	 | ^                    + +  |   . |
	 | . . o - - - < . . . . o - - < . |
	 B---------------------------------R
	Yellow draws a 8.

	> help
	Commands:
	  (blank)         Let the computer handle the current turn
	  board           Display the game board
	  candidates      List opponent pawns in play on the board with their positions
	  exit            Quit the game
	  help [command]  Print this help or get help for specified command
	  info            Display game and current player information
	  move # [#]      Move specified current player's pawn specied number of spaces
	  pass            End current turn
	  replace # #     Replace opponent pawn with current player pawn
	> 

## Status

As of March, 2018, this is still somewhat of a work in progress. I did this as a fun reason to practice my C# and explore the dynamics of the game. I plan to make periodic improvements as time and interest allow.

