using HMLLibrary;
using RaftVR;

public class RaftVRLoader : Mod
{
    public static SettingsAPI ExtraSettingsAPI_Settings = new SettingsAPI();

    private ModInitializer initializer;

    public void Start()
    {
        System.AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

        initializer = gameObject.AddComponent<ModInitializer>();

        ExtraSettingsAPI_Settings.initializerInstance = initializer;

        if (ExtraSettingsAPI_Settings.ExtraSettingsAPI_Loaded)
        {
            initializer.OnSettingsAPILoaded();
        }

        initializer.Init();
    }

    private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, System.ResolveEventArgs args)
    {
        if (args.Name.Contains("AssetsTools.NET"))
        {
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.FullName.Contains("AssetsTools.NET")) return asm;
            }
        }
        return null;
    }

    public void OnModUnload()
    {
        initializer.Unload();
    }
}