using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Modding;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;

// ReSharper disable Unity.NoNullPropogation


namespace LostLord
{
    // ReSharper disable once UnusedMember.Global
    public class LostLord : Mod, ITogglableMod
    {
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once NotAccessedField.Global
        public LostLord Instance;
        
        private const string LOST_KIN_VAR = "infectedKnightDreamDefeated";

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
            ModHooks.Instance.GetPlayerBoolHook += GetBoolHandler;
            ModHooks.Instance.LanguageGetHook += LangGet;
        }

        private static bool GetBoolHandler(string get)
        {
            return get != LOST_KIN_VAR && PlayerData.instance.GetBoolInternal(get);
        }
        
        private static string LangGet(string key, string sheettitle)
        {
            return key == "INFECTED_KNIGHT_DREAM_MAIN" && PlayerData.instance.infectedKnightDreamDefeated
                ? "Lord"
                : Language.Language.GetInternal(key, sheettitle);
        }
        
        private static void AfterSaveGameLoad(SaveGameData data) => AddComponent();

        private static void AddComponent()
        {
            GameManager.instance.gameObject.AddComponent<KinFinder>();
        }

        public void Unload()
        {
            ModHooks.Instance.AfterSavegameLoadHook -= AfterSaveGameLoad;
            ModHooks.Instance.NewGameHook -= AddComponent;
            ModHooks.Instance.GetPlayerBoolHook -= GetBoolHandler;
            ModHooks.Instance.LanguageGetHook -= LangGet;

            KinFinder x = GameManager.instance?.gameObject.GetComponent<KinFinder>();
            if (x == null) return;
            UObject.Destroy(x);
        }
    }
}