using UnityEngine;
using AleVerDes.Signals;
using AleVerDes.Signals.Examples;

namespace AleVerDes.Signals.Examples
{
    /// <summary>
    /// Example demonstrating the use of keyed signal buses for component-specific messaging.
    /// Shows how different components can communicate through isolated signal scopes.
    /// </summary>
    public class KeyedSignalExample : MonoBehaviour
    {
        [Header("Signal Bus Configuration")]
        [SerializeField] private KeySignalBus<string> _componentSignalBus;

        [Header("Component Settings")]
        [SerializeField] private string _componentId = "PlayerController";
        [SerializeField] private float _health = 100f;

        private void Start()
        {
            // Subscribe to component-specific signals
            _componentSignalBus.Subscribe<PlayerHealthChangedSignal>(_componentId, OnHealthChanged);
            _componentSignalBus.Subscribe<ButtonClickedSignal>(_componentId, OnButtonClicked);

            // Subscribe to global signals (empty key scope)
            _componentSignalBus.Subscribe<GameStartedSignal>("", OnGameStarted);

            Debug.Log($"{_componentId}: Subscribed to keyed signals");
        }

        private void Update()
        {
            // Simulate component-specific events
            if (Input.GetKeyDown(KeyCode.H))
            {
                _health = Mathf.Max(0, _health - 25f);
                var healthSignal = new PlayerHealthChangedSignal(1, _health, 100f);
                _componentSignalBus.Invoke(_componentId, healthSignal);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                _health = 100f;
                var healthSignal = new PlayerHealthChangedSignal(1, _health, 100f);
                _componentSignalBus.Invoke(_componentId, healthSignal);
            }

            // Send global game event
            if (Input.GetKeyDown(KeyCode.G))
            {
                _componentSignalBus.Invoke("", new GameStartedSignal());
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from all signals for this component
            _componentSignalBus.Unsubscribe<PlayerHealthChangedSignal>(_componentId, OnHealthChanged);
            _componentSignalBus.Unsubscribe<ButtonClickedSignal>(_componentId, OnButtonClicked);
            _componentSignalBus.Unsubscribe<GameStartedSignal>("", OnGameStarted);

            Debug.Log($"{_componentId}: Unsubscribed from all keyed signals");
        }

        private void OnHealthChanged(PlayerHealthChangedSignal signal)
        {
            Debug.Log($"{_componentId}: Health changed to {signal.Health}/{signal.MaxHealth} ({signal.HealthPercentage:P1})");

            if (signal.Health <= 0)
            {
                Debug.Log($"{_componentId}: Component destroyed due to zero health!");
                Destroy(gameObject);
            }
        }

        private void OnButtonClicked(ButtonClickedSignal signal)
        {
            Debug.Log($"{_componentId}: Received button click for '{signal.ButtonName}'");
        }

        private void OnGameStarted(GameStartedSignal signal)
        {
            Debug.Log($"{_componentId}: Global game started event received at {signal.Timestamp:HH:mm:ss}");
        }
    }
}
