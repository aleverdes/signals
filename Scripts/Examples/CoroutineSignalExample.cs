using System.Collections;
using UnityEngine;
using AleVerDes.Signals;
using AleVerDes.Signals.Examples;

namespace AleVerDes.Signals.Examples
{
    /// <summary>
    /// Example demonstrating coroutine-based signal waiting.
    /// Shows how to pause coroutines until specific signals are received.
    /// </summary>
    public class CoroutineSignalExample : MonoBehaviour
    {
        [Header("Signal Bus Configuration")]
        [SerializeField] private SignalBus _signalBus;

        [Header("Example Settings")]
        [SerializeField] private float _delayBetweenSignals = 2f;

        private void Start()
        {
            StartCoroutine(GameFlowCoroutine());
        }

        private void Update()
        {
            // Press space to trigger the next signal in sequence
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _signalBus.Invoke(new GameStartedSignal());
            }
        }

        private IEnumerator GameFlowCoroutine()
        {
            Debug.Log("Waiting for game to start... (Press SPACE to start)");

            // Wait for game to start
            yield return new WaitForSignal<GameStartedSignal>(_signalBus);

            Debug.Log("Game started! Now waiting for player health change... (Press D in BasicSignalExample)");

            // Wait for player health to change
            yield return new WaitForSignal<PlayerHealthChangedSignal>(_signalBus,
                signal => Debug.Log($"Health change detected: {signal.HealthPercentage:P1}"));

            Debug.Log("Health changed! Waiting for button click... (Press B in BasicSignalExample)");

            // Wait for button click
            yield return new WaitForSignal<ButtonClickedSignal>(_signalBus);

            Debug.Log("Button clicked! Game flow complete.");
        }

        /// <summary>
        /// Example of a more complex coroutine that waits for multiple signals in sequence.
        /// </summary>
        private IEnumerator ComplexGameFlow()
        {
            Debug.Log("=== Complex Game Flow Started ===");

            // Phase 1: Wait for game start
            yield return new WaitForSignal<GameStartedSignal>(_signalBus);
            Debug.Log("✓ Game started");

            // Phase 2: Wait for player to take damage (health < 100%)
            yield return new WaitForSignal<PlayerHealthChangedSignal>(_signalBus,
                signal =>
                {
                    if (signal.HealthPercentage < 1.0f)
                    {
                        Debug.Log($"✓ Player took damage: {signal.HealthPercentage:P1} health remaining");
                        return true; // Signal handled
                    }
                    return false; // Keep waiting
                });

            // Phase 3: Wait for UI interaction
            yield return new WaitForSignal<ButtonClickedSignal>(_signalBus,
                signal =>
                {
                    Debug.Log($"✓ UI interaction: {signal.ButtonName}");
                    return true;
                });

            // Phase 4: Simulate some processing time
            Debug.Log("Processing game state...");
            yield return new WaitForSeconds(1f);

            Debug.Log("=== Complex Game Flow Complete ===");
        }
    }
}
