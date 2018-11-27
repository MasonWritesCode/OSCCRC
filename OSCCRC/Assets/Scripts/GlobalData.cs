using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalData {

    // Global Data:

    public static GameController.GameMode mode;
    public static Menu_Panel.Folder folder;
    public static string currentStageFile;
    public static int curPage;


    // Development Related Values:

    // Disables vsync so that performance can properly be measured
    public static bool d_uncapFrames = true;

    // Performance related experiment, code exists in GameMap and MapTile
    // Added TileTiledColor material, which is included in Resource Pack
    public static bool x_useBigTile = false;
}
