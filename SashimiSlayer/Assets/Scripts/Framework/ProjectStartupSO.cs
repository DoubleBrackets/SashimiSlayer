using UnityEngine;

namespace Framework
{
    [CreateAssetMenu(fileName = "ProjectStartup", menuName = "Framework/Project Startup", order = 0)]
    public class ProjectStartupSO : ScriptableObject
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            // NOTE: read docs to see directory requirements for Resources.Load!
            var prefab = Resources.Load<GameObject>("GlobalServices");
            // create the prefab in your scene
            GameObject inScene = Instantiate(prefab);

            // mark root as DontDestroyOnLoad();
            DontDestroyOnLoad(inScene);
        }
    }
}