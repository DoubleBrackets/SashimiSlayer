using Steamworks;
using UnityEngine;

namespace Steam
{
    public class SteamTest : MonoBehaviour
    {
        private void Start()
        {
            if (SteamManager.Initialized)
            {
                string name = SteamFriends.GetPersonaName();
                Debug.Log(name);
            }
        }
    }
}