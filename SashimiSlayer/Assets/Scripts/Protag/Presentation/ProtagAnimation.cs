using Events;
using UnityEngine;

namespace Protag.Presentation
{
    public class ProtagAnimation : MonoBehaviour
    {
        [Header("Listening Events")]

        [SerializeField]
        private SOEvent _protagSuccessfulBlockEvent;

        [SerializeField]
        private SOEvent _protagTryBlockEvent;

        [SerializeField]
        private SOEvent _protagUnsheatheEvent;

        [SerializeField]
        private SOEvent _protagSheatheEvent;

        [SerializeField]
        private SOEvent _protagTrySliceEvent;

        [SerializeField]
        private SOEvent _protagSuccessfulSliceEvent;

        [SerializeField]
        private SOEvent _protagTakeDamageEvent;

        [SerializeField]
        private SOEvent _protagVictoryEvent;

        [SerializeField]
        private SOEvent _protagLossEvent;

        [SerializeField]
        private Animator _animator;

        private void Awake()
        {
            _protagSuccessfulBlockEvent.AddListener(OnProtagSuccessfulBlock);
            _protagTryBlockEvent.AddListener(OnProtagTryBlock);

            _protagUnsheatheEvent.AddListener(OnProtagUnsheathe);
            _protagSheatheEvent.AddListener(OnProtagSheathe);

            _protagTrySliceEvent.AddListener(OnProtagTrySlice);
            _protagSuccessfulSliceEvent.AddListener(OnProtagSuccessfulSlice);

            _protagTakeDamageEvent.AddListener(OnProtagTakeDamage);

            _protagVictoryEvent.AddListener(OnProtagVictory);
            _protagLossEvent.AddListener(OnProtagLoss);
        }

        private void OnDestroy()
        {
            _protagSuccessfulBlockEvent.RemoveListener(OnProtagSuccessfulBlock);
            _protagTryBlockEvent.RemoveListener(OnProtagTryBlock);

            _protagUnsheatheEvent.RemoveListener(OnProtagUnsheathe);
            _protagSheatheEvent.RemoveListener(OnProtagSheathe);

            _protagTrySliceEvent.RemoveListener(OnProtagTrySlice);
            _protagSuccessfulSliceEvent.RemoveListener(OnProtagSuccessfulSlice);

            _protagTakeDamageEvent.RemoveListener(OnProtagTakeDamage);

            _protagVictoryEvent.RemoveListener(OnProtagVictory);
            _protagLossEvent.RemoveListener(OnProtagLoss);
        }

        private void OnProtagVictory()
        {
            _animator.Play("ProtagVictory", 0, 0);
        }

        private void OnProtagDead()
        {
            _animator.SetBool("ProtagDead", true);
        }

        private void OnProtagLoss()
        {
            _animator.Play("ProtagLoss", 0, 0);
        }

        private void OnProtagSuccessfulBlock()
        {
            _animator.SetTrigger("SuccessfulBlock");
        }

        private void OnProtagTryBlock()
        {
            _animator.SetTrigger("TryBlock");
        }

        private void OnProtagUnsheathe()
        {
            _animator.SetBool("Sheathed", false);
        }

        private void OnProtagSheathe()
        {
            _animator.SetBool("Sheathed", true);
        }

        private void OnProtagTrySlice()
        {
            // _animator.SetTrigger("TrySlice");
        }

        private void OnProtagSuccessfulSlice()
        {
            _animator.SetTrigger("SuccessfulSlice");
        }

        private void OnProtagTakeDamage()
        {
            _animator.SetTrigger("TakeDamage");
        }
    }
}