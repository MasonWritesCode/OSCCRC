
// This is an interface between the game controller and a game mode that it will currently be using.

public interface IGameMode {

    // Do we want an update function here to have the game controller call each update?

    // Begins the game of this mode on the currently loaded stage.
    // resetGame() should be used for resetting an already started game.
    void startGame();


    // Resets the currents game, so that the state resembles the state when startGame as called.
    void resetGame();


    // Ends the game of this mode.
    // This will allow for cleanup actions such as unsubscribing from events, and so should always be called when game finishes.
    // It does not restore any map changes or make the stage ready to start another game mode.
    // This is not where to put something like Multiplayer displaying who wins, or puzzle prompting for next level
    //   as that is handled by the mode itself and doesn't need an outside script to trigger it
    void endGame();


    // Places a directional tile
    // Puzzle mode allows a limited set of tiles, wheras multiplayer keeps the most recent three
    void placeDirection(MapTile tile, Directions.Direction dir);
}
