using BepInEx;
using CreatureLister;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;

namespace ListEmAll
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    public class ListEmAllPlugin : BaseUnityPlugin
    {
        private const string PluginAuthor = "FitItFelix";
        private const string PluginName = "ListEmAll";
        private const string PluginVersion = "1.0.0";
        public const string PluginGuid = PluginAuthor + "." + PluginName;

        private void Awake()
        {
            CommandManager.Instance.AddConsoleCommand(new CreatureListerController());
        }
    }
    
    public class CreatureListerController : ConsoleCommand {
        public override void Run(string[] args) {
            HumanoidLister.WriteData();
        }

        public override string Name => "creature_lister_generate_defaults_file";

        public override string Help =>
            "Write all character based default information to a YAML file inside the BepInEx config folder.";
    }
}