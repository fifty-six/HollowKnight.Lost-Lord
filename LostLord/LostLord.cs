using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HutongGames.PlayMaker;
using InControl;
using Modding;

namespace LostLord
{
    // ReSharper disable once UnusedMember.Global
    public class LostLord : Mod
    {
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once NotAccessedField.Global
        public LostLord Instance;

        public override string GetVersion()
        {
            return FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(LostLord)).Location).FileVersion;
        }

        public override void Initialize()
        {
            Instance = this;
            
            Log("Initalizing.");
            ModHooks.Instance.AfterSavegameLoadHook += AfterSaveGameLoad;
            ModHooks.Instance.NewGameHook += AddComponent;
        }

        private static void AfterSaveGameLoad(SaveGameData data) => AddComponent();

        private static void AddComponent()
        {
            GameManager.instance.gameObject.AddComponent<KinFinder>();
        }
    }
}