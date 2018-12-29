using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using ModCommon.Util;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace LostLord
{
    [UsedImplicitly]
    public class LostLord : Mod<VoidModSettings, LordSettings>, ITogglableMod
    {
        public static LostLord Instance;

        public static readonly IList<Sprite> SPRITES = new List<Sprite>();

        private string _lastScene;

        internal bool IsInHall => _lastScene == "GG_Workshop";

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
            ModHooks.Instance.LanguageGetHook += LangGet;
            USceneManager.activeSceneChanged += LastScene;

            int ind = 0;
            Assembly asm = Assembly.GetExecutingAssembly();

            LoadGlobalSettings();

            var timer = new Stopwatch();
            timer.Start();
            foreach (string res in asm.GetManifestResourceNames())
            {
                if (!res.EndsWith(".png"))
                {
                    Log("Unknown resource: " + res);

                    continue;
                }

                bool pureFile = res.StartsWith("LostLord.pure") || res.StartsWith("LostLord.z");

                if (GlobalSettings.Pure ? !pureFile : pureFile)
                    continue;

                using (Stream s = asm.GetManifestResourceStream(res))
                {
                    if (s == null) continue;

                    byte[] buffer = new byte[s.Length];
                    s.Read(buffer, 0, buffer.Length);
                    s.Dispose();

                    // Create texture from bytes
                    var tex = new Texture2D(1, 1);
                    tex.LoadImage(buffer, true);

                    // Create sprite from texture
                    SPRITES.Add(Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)));

                    Log("Created sprite from embedded image: " + res + " at ind " + ++ind);
                }
            }
            timer.Stop();
            Log("Loaded images in " + timer.Elapsed);
        }

        private void SetupSettings()
        {
            if (!File.Exists(this.GetAttr<string>("_globalSettingsFilename")))
            {
                GlobalSettings?.Reset();
            }

            SaveGlobalSettings();
        }

        private void LastScene(Scene arg0, Scene arg1) => _lastScene = arg0.name;

        private string LangGet(string key, string sheettitle)
        {
            return key == "INFECTED_KNIGHT_DREAM_MAIN" && PlayerData.instance.infectedKnightDreamDefeated && IsInHall
                ? "Lord"
                : Language.Language.GetInternal(key, sheettitle);
        }

        private void AfterSaveGameLoad(SaveGameData data) => AddComponent();

        private void AddComponent()
        {
            SetupSettings();

            GameManager.instance.gameObject.AddComponent<KinFinder>();
        }

        public void Unload()
        {
            ModHooks.Instance.AfterSavegameLoadHook -= AfterSaveGameLoad;
            ModHooks.Instance.NewGameHook -= AddComponent;
            ModHooks.Instance.LanguageGetHook -= LangGet;
            USceneManager.activeSceneChanged -= LastScene;

            // ReSharper disable once Unity.NoNullPropogation
            var x = GameManager.instance?.gameObject.GetComponent<KinFinder>();

            if (x == null) return;

            UObject.Destroy(x);
        }
    }
}