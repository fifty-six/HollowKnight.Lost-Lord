using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Modding;
using UObject = UnityEngine.Object;

// ReSharper disable Unity.NoNullPropogation


namespace LostLord
{
    // ReSharper disable once UnusedMember.Global
    public class LostLord : Mod<LordSettings, VoidModSettings>, ITogglableMod
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
            ModHooks.Instance.SetPlayerBoolHook += SetBoolHandler;
        }

        private void SetBoolHandler(string set, bool val)
        {
            if (set == LOST_KIN_VAR && val && PlayerData.instance.infectedKnightDreamDefeated)
            {
                Settings.DefeatedLord = true;
            }
            PlayerData.instance.SetBoolInternal(set, val);
        }

        private bool GetBoolHandler(string get)
        {
            if (get == LOST_KIN_VAR)
            {
                return PlayerData.instance.infectedKnightDreamDefeated && Settings.DefeatedLord;
            }
            return PlayerData.instance.GetBoolInternal(get);
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

            KinFinder x = GameManager.instance?.gameObject.GetComponent<KinFinder>();
            if (x == null) return;
            UObject.Destroy(x);
        }
    }
}