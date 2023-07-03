using System;
using HarmonyLib;

namespace CardZones {
    public class CardZonesMod : Mod {
        public const string Name = "Card Zones";
        public const string GUID = "com.maxsch.stacklands.cardzones";
        public const string Version = "0.2.0";

        private static Harmony harmony;

        public void Awake() {
            Log.Init(Logger);

            harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        private void OnDestroy() {
            harmony.UnpatchSelf();
        }
    }
}
