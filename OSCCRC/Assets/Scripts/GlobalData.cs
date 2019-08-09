
// This class defines values that persist between scenes, so that a scene knows how to initialize itself.

public static class GlobalData {

    // MainManu exports:

    public static GameController.GameMode mode;
    public static string currentStagePath;


    // Development Related Values:

    // Disables vsync so that performance can properly be measured
    public static readonly bool d_uncapFrames = false;

    // Begins loading of Game scene once main menu is shown where the slowdown is less noticible.
    // This is currently disabled by default, because beginning an async load can cause input drops which I consider much worse.
    // This will disable the profiler apparently, so make sure this is false if you want to profile the main menu.
    public static readonly bool d_loadSceneAsync = false;

    // Makes the game always open the competitive test map in competitive mode
    public static readonly bool d_forceCompetitiveTest = false;
}
