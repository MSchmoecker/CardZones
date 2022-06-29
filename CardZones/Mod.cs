using System;
using BepInEx;
using HarmonyLib;

namespace CardZones {
    [BepInPlugin(GUID, Name, Version)]
    public class Mod : BaseUnityPlugin {
        public const string Name = "Card Zones";
        public const string GUID = "com.maxsch.stacklands.cardzones";
        public const string Version = "0.0.0";

        private static Harmony harmony;

        public void Awake() {
            Log.Init(Logger);

            harmony = new Harmony(GUID);
            harmony.PatchAll();
        }
    }
}
