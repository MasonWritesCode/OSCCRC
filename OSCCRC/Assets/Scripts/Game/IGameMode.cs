using System.Collections;
using System.Collections.Generic;

// This is an interface between the game controller and a game mode that it will currently be using.

public interface IGameMode {

    // TODO: How are we checking for winning or losing conditions?

    // Do we want an update function here to have the game controller call each update?

    // Begins the game of this mode
    // Puzzle starts paused, multiplayer starts after a countdown or something?
    void startGame();

    // Ends the game of this mode
    // Multiplayer might want to display who wins, while puzzle might want to allow easy access to next level
    void endGame();

    // Pauses the game
    void pauseGame();

    // Places a directional tile
    // Puzzle mode allows a limited set of tiles, wheras multiplayer keeps the most recent three
    void placeDirection(MapTile tile, Directions.Direction dir);
}
