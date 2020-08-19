
// This class defines values that persist between scenes, so that a scene knows how to initialize itself.

public static class GlobalData {

    // MainMenu exports:

    public static GameController.GameMode mode;
    public static string currentStagePath;
    public static bool[] isHumanPlayer = { true, false, false, false };


    // Development Related Values:

    // Disables vsync in the game scene so that performance can properly be measured
    public static readonly bool d_uncapFrames = false;

    // Begins loading of Game scene asynchronously while still in the main menu.
    public static readonly bool d_loadSceneAsync = true;

}
