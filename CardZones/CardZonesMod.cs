using System;
using HarmonyLib;

namespace CardZones {
    public class CardZonesMod : Mod {
        public const string Name = "Card Zones";
        public const string GUID = "com.maxsch.stacklands.cardzones";
        public const string Version = "0.3.0";

        public static ConfigEntry<bool> stickyZones;

        private static Harmony harmony;

        public void Awake() {
            Log.Init(Logger);

            stickyZones = Config.GetEntry<bool>("Sticky Card Zones", false, new ConfigUI() {
                Tooltip = "Prevents other cards from pushing zone cards around.\n" +
                          "Behaves like magic glue, two zone cards will still be pushed away from each other. " +
                          "Magic glue has priority over zone cards."
            });

            harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        private void OnDestroy() {
            harmony.UnpatchSelf();
        }
    }
}
