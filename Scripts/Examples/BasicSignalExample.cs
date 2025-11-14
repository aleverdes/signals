using UnityEngine;
using AleVerDes.Signals;
using AleVerDes.Signals.Examples;

namespace AleVerDes.Signals.Examples
{
    /// <summary>
    /// Basic example demonstrating how to use the signal bus system.
    /// This example shows subscription, unsubscription, and signal invocation.
    /// </summary>
    public class BasicSignalExample : MonoBehaviour
    {
        [Header("Signal Bus Configuration")]
        [SerializeField] private SignalBus _signalBus;

        private void Start()
        {
            // Subscribe to signals
            _signalBus.Subscribe<GameStartedSignal>(OnGameStarted);
            _signalBus.Subscribe<PlayerHealthChangedSignal>(OnPlayerHealthChanged);
            _signalBus.Subscribe<ButtonClickedSignal>(OnButtonClicked);

            // Start the game by firing a signal
            _signalBus.Invoke(new GameStartedSignal());

            Debug.Log("Basic Signal Example: Subscribed to signals and started game");
        }

        private void Update()
        {
            // Simulate player taking damage
            if (Input.GetKeyDown(KeyCode.D))
            {
                var healthSignal = new PlayerHealthChangedSignal(1, 75f, 100f);
                _signalBus.Invoke(healthSignal);
            }

            // Simulate button click
            if (Input.GetKeyDown(KeyCode.B))
            {
                var buttonSignal = new ButtonClickedSignal("MainMenuButton", Input.mousePosition);
                _signalBus.Invoke(buttonSignal);
            }
        }

        private void OnDestroy()
        {
            // Always unsubscribe when the component is destroyed
            _signalBus.Unsubscribe<GameStartedSignal>(OnGameStarted);
            _signalBus.Unsubscribe<PlayerHealthChangedSignal>(OnPlayerHealthChanged);
            _signalBus.Unsubscribe<ButtonClickedSignal>(OnButtonClicked);

            Debug.Log("Basic Signal Example: Unsubscribed from all signals");
        }

        private void OnGameStarted(GameStartedSignal signal)
        {
            Debug.Log($"Game started at {signal.Timestamp:HH:mm:ss}");
        }

        private void OnPlayerHealthChanged(PlayerHealthChangedSignal signal)
        {
            Debug.Log($"Player {signal.PlayerId} health: {signal.Health}/{signal.MaxHealth} ({signal.HealthPercentage:P1})");
        }

        private void OnButtonClicked(ButtonClickedSignal signal)
        {
            Debug.Log($"Button '{signal.ButtonName}' clicked at screen position {signal.ScreenPosition}");
        }
    }
}
