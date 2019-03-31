
// This class defines values that persist between scenes, so that a scene knows how to initialize itself.

public static class GlobalData {

    // MainManu exports:

    public static GameController.GameMode mode;
    public static string currentStagePath;


    // Development Related Values:

    // Disables vsync so that performance can properly be measured
    public static bool d_uncapFrames = false;

    // Performance related experiment, code exists in GameMap and MapTile
    // Added TileTiledColor material, which is included in Resource Pack
    public static bool x_useBigTile = true;
}
