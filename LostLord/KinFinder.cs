using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace LostLord
{
    internal class KinFinder : MonoBehaviour
    {
        private void Start()
        {
            USceneManager.activeSceneChanged += SceneChanged;
        }

        private void SceneChanged(Scene arg0, Scene arg1)
        {
            if (arg1.name != "GG_Lost_Kin") return;

            StartCoroutine(AddComponent());
        }

        private static IEnumerator AddComponent()
        {
            yield return null;

            GameObject.Find("Lost Kin").AddComponent<Kin>();
        }

        private void OnDestroy()
        {
            USceneManager.activeSceneChanged -= SceneChanged;
        }
    }
}