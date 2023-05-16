using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;

namespace ListEmAll
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    public class ListEmAllPlugin : BaseUnityPlugin
    {
        private const string PluginAuthor = "FixItFelix";
        private const string PluginName = "ListEmAll";
        private const string PluginVersion = "1.0.0";
        public const string PluginGuid = PluginAuthor + "." + PluginName;

        private void Awake()
        {
            CommandManager.Instance.AddConsoleCommand(new PrefabsTranslationsPrinterController());
        }
    }

    public class PrefabsTranslationsPrinterController : ConsoleCommand
    {
        public override void Run(string[] args)
        {
            if (args.Length > 0)
            {
                TranslationsPrinter.WriteData(args[0]);
            }
            else
            {
                TranslationsPrinter.WriteData();
            }
        }

        public override string Name => "print_translations_to_file";

        public override string Help =>
            "Write all character based default information to a YAML file inside the BepInEx config folder.";
    }
}