using Events;
using UnityEngine;

public class DebugService : MonoBehaviour
{
    [SerializeField]
    private bool _showGuiLabel;

    [Header("Events (Out)")]

    [SerializeField]
    private VoidEvent _onDrawGuiEvent;

    public static DebugService Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            _showGuiLabel = !_showGuiLabel;
        }
    }

    private void OnGUI()
    {
        if (!_showGuiLabel)
        {
            return;
        }

        GUI.color = Color.black;
        _onDrawGuiEvent.Raise();
        GUI.color = Color.white;
    }
}