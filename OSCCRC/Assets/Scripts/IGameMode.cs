using System.Collections;
using System.Collections.Generic;

public interface IGameMode {

    // TODO: How are we checking for winning or losing conditions?

    // Do we want an update function here to have the game controller call each update?

    // Puzzle starts paused, multiplayer starts after a countdown or something?
    void startGame();

    // Multiplayer might want to display who wins, while puzzle might want to allow easy access to next level
    void endGame();

    // Puzzle mode allows a limited set of tiles, wheras multiplayer keeps the most recent three
    void placeDirection(MapTile tile, Directions.Direction dir);
}
