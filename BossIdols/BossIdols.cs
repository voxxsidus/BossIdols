using BepInEx;
using BepInEx.Configuration;
using Jotunn.Managers;
using Jotunn.Utils;

namespace BossIdols
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class BossIdols : BaseUnityPlugin
    {
        public const string PluginGUID = "com.voxxsidus.bossidols";
        public const string PluginName = "BossIdols";
        public const string PluginVersion = "1.0.0";

        public static ConfigEntry<bool> Enabled;
        public static ConfigEntry<int> MinPerIdol;
        public static ConfigEntry<int> MaxPerIdol;

        // Kept in a field so the underlying FileSystemWatcher isn't garbage collected
        private ConfigFileWatcher _configWatcher;

        private void Awake()
        {
            BindConfig();

            // Reload the config when the file is edited (e.g. on a dedicated server). Events are raised on the
            // main thread; the reload fires SettingChanged and Jötunn pushes synced values to connected clients.
            _configWatcher = new ConfigFileWatcher(Config);

            // ZNetScene.Awake, i.e. every time a world is loaded
            PrefabManager.OnPrefabsRegistered += IdolDrops.Apply;

            Jotunn.Logger.LogInfo("BossIdols has landed");
        }

        private void BindConfig()
        {
            // Admin-only so Jötunn syncs the server's values to every client: drops are rolled
            // by whichever machine owns the boss when it dies, so all machines must agree
            ConfigDescription Synced(string description, AcceptableValueBase range) =>
                new ConfigDescription(description, range, new ConfigurationManagerAttributes { IsAdminOnly = true });

            Enabled = Config.Bind("General", "Enabled", true,
                Synced("Whether bosses drop their biome's Battle and Protection Idols.", null));
            MinPerIdol = Config.Bind("General", "MinPerIdol", 1,
                Synced("Minimum number of each idol (Battle and Protection, rolled separately) a boss drops.",
                    new AcceptableValueRange<int>(1, 100)));
            MaxPerIdol = Config.Bind("General", "MaxPerIdol", 3,
                Synced("Maximum number of each idol (Battle and Protection, rolled separately) a boss drops. " +
                       "Values below MinPerIdol are treated as MinPerIdol.",
                    new AcceptableValueRange<int>(1, 100)));

            // Clients join a world before the server's values arrive, so re-apply once they do
            SynchronizationManager.OnConfigurationSynchronized += (_, _) => IdolDrops.Apply();
            Enabled.SettingChanged += (_, _) => IdolDrops.Apply();
            MinPerIdol.SettingChanged += (_, _) => IdolDrops.Apply();
            MaxPerIdol.SettingChanged += (_, _) => IdolDrops.Apply();
        }
    }
}
