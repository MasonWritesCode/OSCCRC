
// This is an interface between the game controller and a game mode that it will currently be using.

public interface IGameMode {

    // Do we want an update function here to have the game controller call each update?

    // Begins the game of this mode, or resets an in-progress game.
    // Puzzle starts paused, multiplayer starts after a countdown or something?
    void startGame();


    // Ends the game of this mode
    // This is not where to put something like Multiplayer displaying who wins, or puzzle prompting for next level
    //   as that is handled by the mode itself and doesn't need an outside script to trigger it
    // This will allow for cleanup actions such as unsubscribing from events
    void endGame();


    // Places a directional tile
    // Puzzle mode allows a limited set of tiles, wheras multiplayer keeps the most recent three
    void placeDirection(MapTile tile, Directions.Direction dir);
}
