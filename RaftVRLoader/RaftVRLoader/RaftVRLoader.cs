using HMLLibrary;
using RaftVR;

public class RaftVRLoader : Mod
{
    public static SettingsAPI ExtraSettingsAPI_Settings = new SettingsAPI();

    private ModInitializer initializer;

    public void Start()
    {
        initializer = gameObject.AddComponent<ModInitializer>();

        ExtraSettingsAPI_Settings.initializerInstance = initializer;

        if (ExtraSettingsAPI_Settings.ExtraSettingsAPI_Loaded)
        {
            initializer.OnSettingsAPILoaded();
        }

        initializer.Init();
    }

    public void OnModUnload()
    {
        initializer.Unload();
    }
}