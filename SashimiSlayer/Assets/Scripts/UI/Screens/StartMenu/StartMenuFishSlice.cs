using System.Collections.Generic;
using EditorUtils.BoldHeader;
using Interactions.Components;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace UI.Screens.StartMenu
{
    public class StartMenuFishSlice : MonoBehaviour
    {
        [BoldHeader("Start Menu Fish Slice Minigame")]
        [InfoBox("Handles the fish slicing minigame")]
        [Header("Depends")]

        [SerializeField]
        private List<StartMenuFish> _fish;

        [SerializeField]
        private Transform _fishPlaceTransform;

        [SerializeField]
        private CircularSliceableMono _fishSliceableMono;

        /// <summary>
        ///     Gameobject to show when no fish are placed
        /// </summary>
        [SerializeField]
        private GameObject _noFishView;

        [Header("Events")]

        [SerializeField]
        private UnityEvent _onFishSlice;

        [SerializeField]
        private UnityEvent _onFishPlace;

        private StartMenuFish _currentFish;

        private void Start()
        {
            _fishSliceableMono.OnSliced += HandleOnFishSliced;
            PlaceFish();
            _noFishView.SetActive(false);
        }

        private void OnDestroy()
        {
            _fishSliceableMono.OnSliced -= HandleOnFishSliced;
        }

        public void PlaceFish()
        {
            if (_currentFish != null)
            {
                return;
            }

            // Place a new fish down
            StartMenuFish fishPrefab = _fish[Random.Range(0, _fish.Count)];
            StartMenuFish newFish = Instantiate(fishPrefab, _fishPlaceTransform.position, Quaternion.identity);
            _currentFish = newFish;
            _currentFish.Place();
            _onFishPlace.Invoke();
            _fishSliceableMono.Enabled = true;
            _noFishView.SetActive(false);
        }

        private void HandleOnFishSliced()
        {
            if (_currentFish == null)
            {
                return;
            }

            _currentFish.Slice();
            _currentFish = null;
            _onFishSlice.Invoke();
            _fishSliceableMono.Enabled = false;
            _noFishView.SetActive(true);
        }
    }
}