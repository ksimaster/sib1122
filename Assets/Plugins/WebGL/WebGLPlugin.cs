using System.Runtime.InteropServices;
/// <summary>
/// Class with a JS Plugin functions for WebGL.
/// </summary>
public static class WebGLPluginJS
{
    // Importing "CallFunction"
    [DllImport("__Internal")]
    public static extern void CallFunction();
    // Importing "ShareFunction" add ksimaster
    [DllImport("__Internal")]
    public static extern void ShareFunction();
    // Importing "ShareFunction" add ksimaster
    [DllImport("__Internal")]
    public static extern void AutoSaveEnglishFunction();
    // Importing "RewardFunction" add ksimaster
    [DllImport("__Internal")]
    public static extern void RewardFunction();
    // Importing "InterstitialFunction" add ksimaster
    [DllImport("__Internal")]
    public static extern void InterstitialFunction();
    // Importing "PassTextParam"
    [DllImport("__Internal")]
    public static extern void PassTextParam(string text);

    // Importing "ChangeLanguageFunction" add ksimaster
    [DllImport("__Internal")]
    public static extern void SetEnglishFunction(string message);
    // Importing "ChangeLanguageFunction" add ksimaster
    [DllImport("__Internal")]
    public static extern void PassNumberParam(int number);
    // Importing "GetTextValue"
    [DllImport("__Internal")]
    public static extern string GetTextValue();
    // Importing "GetNumberValue"
    [DllImport("__Internal")]
    public static extern int GetNumberValue();
}