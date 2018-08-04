using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using Logger = Modding.Logger;
using ModHooks = Modding.ModHooks;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace LostLord
{
    internal class Kin : MonoBehaviour
    {
        private readonly Dictionary<string, float> _fpsDict = new Dictionary<string, float>
        {
            ["Dash Antic 1"] = 30,
            ["Dash Antic 2"] = 30,
            ["Dash Antic 3"] = 30,
            ["Dash Attack 1"] = 60,
            ["Dash Attack 2"] = 90,
            ["Dash Attack 3"] = 50,
            ["Jump Antic"] = 30,
            ["Jump"] = 60,
            ["Downstab"] = 100, //48,
            ["Downstab Antic"] = 56,
            ["Downstab Land"] = 30,
            ["Downstab Slam"] = 30,
            ["Land"] = 60,
            ["Overhead Slash"] = 20,
            ["Overhead Slashing"] = 20,
            ["Overhead Antic"] = 34,
            ["Roar Start"] = 20,
            ["Roar Loop"] = 20,
            ["Roar End"] = 20,
        };

        private HealthManager _hm;

        private tk2dSpriteAnimator _anim;

        private Recoil _recoil;

        private PlayMakerFSM _stunControl;
        private PlayMakerFSM _balloons;
        private PlayMakerFSM _control;

        private void Start()
        {
            if (!PlayerData.instance.infectedKnightDreamDefeated) return;
            
            ModHooks.Instance.ObjectPoolSpawnHook += Projectile;

            HeroController.instance.AddMPChargeSpa(999);

            Log("Added Kin MonoBehaviour");

            _hm = gameObject.GetComponent<HealthManager>();
            _stunControl = gameObject.LocateMyFSM("Stun Control");
            _balloons = gameObject.LocateMyFSM("Spawn Balloon");
            _anim = gameObject.GetComponent<tk2dSpriteAnimator>();
            _control = gameObject.LocateMyFSM("IK Control");
            _recoil = gameObject.GetComponent<Recoil>();

            _stunControl.FsmVariables.GetFsmInt("Stuns Total").Value = 999;

            _balloons.ChangeTransition("Spawn Pause", "SPAWN", "Stop");

            _control.GetAction<WaitRandom>("Idle", 5).timeMax = 0.01f;
            _control.GetAction<WaitRandom>("Idle", 5).timeMin = 0.001f;

            _hm.hp = 1500;

            // Disable Knockback
            _recoil.enabled = false;

            // 2x Damage on All Components
            foreach (DamageHero i in gameObject.GetComponentsInChildren<DamageHero>(true))
            {
                Log(i.name);
                i.damageDealt *= 2;
            }

            // Speed up some attacks.
            foreach (KeyValuePair<string, float> i in _fpsDict)
            {
                _anim.GetClipByName(i.Key).fps = i.Value;
            }

            // 2x Damage
            _control.GetAction<SetDamageHeroAmount>("Roar End", 3).damageDealt.Value = 2;

            // Increase Jump X
            _control.GetAction<FloatMultiply>("Aim Dstab", 3).multiplyBy = 5;
            _control.GetAction<FloatMultiply>("Aim Jump", 3).multiplyBy = 2.2f;

            // Decrease walk idles.
            RandomFloat walk = _control.GetAction<RandomFloat>("Idle", 3);
            walk.min = 0.001f;
            walk.max = 0.01f;

            // Speed up
            _control.GetAction<Wait>("Jump", 5).time = 0.01f;
            _control.GetAction<Wait>("Dash Antic 2", 2).time = 0.27f;

            // Fall faster.
            _control.GetAction<SetVelocity2d>("Dstab Fall", 4).y = -200; // -130; // -90
            _control.GetAction<SetVelocity2d>("Dstab Fall", 4).everyFrame = true;

            // Make him jump a little lower
            RandomFloat jumpRand = _control.GetAction<RandomFloat>("Jump", 3);
            jumpRand.max = jumpRand.max.Value / 1.2f;


            // Combo Dash into Upslash followed by Dstab's Projectiles..
            _control.CopyState("Dstab Land", "Spawners");
            _control.CopyState("Ohead Slashing", "Ohead Combo");
            _control.CopyState("Dstab Recover", "Dstab Recover 2");

            _control.ChangeTransition("Dash Recover", "FINISHED", "Ohead Combo");

            _control.RemoveAnim("Dash Recover", 3);
            _control.RemoveAnim("Spawners", 3);

            _control.ChangeTransition("Ohead Combo", "FINISHED", "Spawners");
            _control.ChangeTransition("Spawners", "FINISHED", "Dstab Recover 2");
            _control.GetAction<Wait>("Dstab Recover 2", 0).time = 0f;

            List<FsmStateAction> a = _control.GetState("Dstab Fall").Actions.ToList();
            a.AddRange(_control.GetState("Spawners").Actions);

            _control.GetState("Dstab Fall").Actions = a.ToArray();

            // Spawners before Overhead Slashing.
            _control.CopyState("Spawners", "Spawn Ohead");
            _control.ChangeTransition("Ohead Antic", "FINISHED", "Spawn Ohead");
            _control.ChangeTransition("Spawn Ohead", "FINISHED", "Ohead Slashing");
            _control.FsmVariables.GetFsmFloat("Evade Range").Value *= 2;

            // Dstab => Upslash
            _control.CopyState("Ohead Slashing", "Ohead Combo 2");
            _control.ChangeTransition("Dstab Land", "FINISHED", "Ohead Combo 2");
            _control.ChangeTransition("Ohead Combo 2", "FINISHED", "Dstab Recover");

            // Aerial Dash => Dstab
            _control.ChangeTransition("Dash Recover", "FALL", "Dstab Antic");

            // bingo bongo ur dash is now lightspeed
            _control.FsmVariables.GetFsmFloat("Dash Speed").Value *= 2;
            _control.FsmVariables.GetFsmFloat("Dash Reverse").Value *= 2;

            Log("fin.");
        }

        private static GameObject Projectile(GameObject go)
        {
            if (go.name != "IK Projectile DS(Clone)" && go.name != "Parasite Balloon Spawner(Clone)") return go;

            foreach (DamageHero i in go.GetComponentsInChildren<DamageHero>(true))
            {
                i.damageDealt = 2;
            }

            return go;
        }

        private static void Log(object obj)
        {
            Logger.Log("[Lost Lord] " + obj);
        }
    }
}