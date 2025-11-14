# 🎯 AleVerDes.Signals

**A high-performance, feature-rich signal/messaging system for Unity with coroutine support, async operations, and advanced features like prioritized handlers and keyed scoping.**

[![Unity](https://img.shields.io/badge/Unity-2019.4+-blue.svg)](https://unity.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Version](https://img.shields.io/badge/Version-2.0.0-green.svg)]()

---

## ✨ Features

- 🚀 **High Performance** - Optimized signal dispatching with minimal overhead
- 🎮 **Unity Integration** - Custom editors, ScriptableObject support, and coroutine waiting
- 🔄 **Async Support** - Asynchronous signal handlers with `async/await`
- 🎯 **Prioritized Handlers** - Execute handlers in priority order
- 🏷️ **Keyed Scoping** - Component-specific messaging with key-based isolation
- 🛡️ **Thread Safe** - Safe subscription/unsubscription during signal invocation
- 📚 **Rich API** - Comprehensive interfaces for testability and extensibility
- 📖 **Full Documentation** - Complete XML documentation and examples

---

## 📦 Installation

### Unity Package Manager (Recommended)

Add this to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.aleverdes.signals": "https://github.com/aleverdes/signals.git"
  }
}
```

### Manual Installation

1. Download the latest release from [GitHub Releases](https://github.com/aleverdes/signals/releases)
2. Import the `.unitypackage` into your Unity project

---

## 🚀 Quick Start

### 1. Define Your Signals

```csharp
using UnityEngine;

namespace MyGame.Signals
{
    public readonly struct PlayerHealthChangedSignal
    {
        public readonly int PlayerId;
        public readonly float Health;
        public readonly float MaxHealth;

        public PlayerHealthChangedSignal(int playerId, float health, float maxHealth)
        {
            PlayerId = playerId;
            Health = health;
            MaxHealth = maxHealth;
        }

        public float HealthPercentage => Health / MaxHealth;
    }

    public readonly struct GameStartedSignal
    {
        public readonly System.DateTime Timestamp;
        public GameStartedSignal() => Timestamp = System.DateTime.Now;
    }
}
```

### 2. Set Up Signal Bus

```csharp
using UnityEngine;
using AleVerDes.Signals;

public class GameManager : MonoBehaviour
{
    [SerializeField] private SignalBus _signalBus;

    private void Start()
    {
        // Subscribe to signals
        _signalBus.Subscribe<PlayerHealthChangedSignal>(OnPlayerHealthChanged);
        _signalBus.Subscribe<GameStartedSignal>(OnGameStarted);

        // Start the game
        _signalBus.Invoke(new GameStartedSignal());
    }

    private void OnPlayerHealthChanged(PlayerHealthChangedSignal signal)
    {
        Debug.Log($"Player {signal.PlayerId} health: {signal.HealthPercentage:P1}");
    }

    private void OnGameStarted(GameStartedSignal signal)
    {
        Debug.Log($"Game started at {signal.Timestamp:HH:mm:ss}");
    }

    private void OnDestroy()
    {
        // Always unsubscribe to prevent memory leaks
        _signalBus.Unsubscribe<PlayerHealthChangedSignal>(OnPlayerHealthChanged);
        _signalBus.Unsubscribe<GameStartedSignal>(OnGameStarted);
    }
}
```

### 3. Use Global Signal Bus (Optional)

For easier setup, use the ScriptableObject-based global signal bus:

1. Create: `Assets > Create > AleVerDes > Signals > Global Signal Bus`
2. Assign it to your components
3. Use it like a regular signal bus

---

## 🎮 Signal Bus Types

### SignalBus - Basic Global Messaging

```csharp
public class AudioManager : MonoBehaviour
{
    [SerializeField] private SignalBus _signalBus;

    private void Start()
    {
        _signalBus.Subscribe<PlaySoundSignal>(OnPlaySound);
    }

    private void OnPlaySound(PlaySoundSignal signal)
    {
        // Play audio
    }
}
```

### KeySignalBus<TKey> - Scoped Messaging

```csharp
public class InventorySystem : MonoBehaviour
{
    [SerializeField] private KeySignalBus<string> _inventoryBus;

    private void Start()
    {
        // Subscribe to inventory changes for player 1
        _inventoryBus.Subscribe<ItemAddedSignal>("player1", OnItemAdded);
    }

    private void OnItemAdded(ItemAddedSignal signal)
    {
        // Handle item addition for player 1 only
    }
}
```

### GlobalSignalBus - ScriptableObject Singleton

```csharp
[CreateAssetMenu]
public class GlobalSignalBus : ScriptableObject, ISignalBus
{
    // Use in inspector or reference from code
}
```

### PrioritizedSignalBus - Priority-Based Execution

```csharp
public class UIManager : MonoBehaviour
{
    [SerializeField] private PrioritizedSignalBus _uiBus;

    private void Start()
    {
        // High priority for critical UI updates
        _uiBus.Subscribe<ShowDialogSignal>(OnShowDialog, priority: 100);

        // Lower priority for background UI updates
        _uiBus.Subscribe<UpdateUISignal>(OnUpdateUI, priority: 10);
    }
}
```

### AsyncSignalBus - Asynchronous Operations

```csharp
public class DataManager : MonoBehaviour
{
    [SerializeField] private AsyncSignalBus _asyncBus;

    private async void Start()
    {
        _asyncBus.Subscribe<LoadDataSignal>(OnLoadDataAsync);

        // Fire and forget
        _asyncBus.InvokeFireAndForget(new LoadDataSignal("player_data"));

        // Or wait for completion
        await _asyncBus.InvokeAsync(new LoadDataSignal("game_settings"));
    }

    private async Task OnLoadDataAsync(LoadDataSignal signal)
    {
        // Asynchronous data loading
        await LoadDataFromServer(signal.DataId);
    }
}
```

---

## ⏳ Coroutine Support

Pause coroutines until signals are received:

```csharp
using System.Collections;
using AleVerDes.Signals;

public class GameController : MonoBehaviour
{
    [SerializeField] private SignalBus _signalBus;

    private IEnumerator Start()
    {
        Debug.Log("Waiting for game to start...");
        yield return new WaitForSignal<GameStartedSignal>(_signalBus);

        Debug.Log("Game started! Waiting for player input...");
        yield return new WaitForSignal<PlayerInputSignal>(_signalBus, signal => {
            Debug.Log($"Received input: {signal.Action}");
        });

        Debug.Log("Starting gameplay...");
    }
}
```

---

## 🏷️ Keyed Signals

Use keyed signals for component-specific messaging:

```csharp
public class PlayerController : MonoBehaviour
{
    [SerializeField] private KeySignalBus<int> _playerBus;
    [SerializeField] private int _playerId;

    private void Start()
    {
        // Subscribe to signals for this specific player
        _playerBus.Subscribe<HealthChangedSignal>(_playerId, OnHealthChanged);
        _playerBus.Subscribe<AbilityUsedSignal>(_playerId, OnAbilityUsed);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Send signal only to this player's subscribers
            _playerBus.Invoke(_playerId, new AbilityUsedSignal("jump"));
        }
    }

    private void OnHealthChanged(HealthChangedSignal signal)
    {
        // Only called for this player
    }
}
```

---

## 🔧 Advanced Usage

### Custom Signal Bus Implementation

```csharp
using AleVerDes.Signals;

public class CustomSignalBus : ISignalBus
{
    private readonly SignalBus _internalBus = new SignalBus();

    public void Subscribe<TSignal>(SignalReceivedDelegate<TSignal> receiver) =>
        _internalBus.Subscribe(receiver);

    public void Unsubscribe<TSignal>(SignalReceivedDelegate<TSignal> receiver) =>
        _internalBus.Unsubscribe(receiver);

    public void Invoke<TSignal>(TSignal signal) =>
        _internalBus.Invoke(signal);

    // ... implement other interface methods
}
```

### Signal Filtering

```csharp
public class FilteredSignalHandler : MonoBehaviour
{
    private void Start()
    {
        _signalBus.Subscribe<PlayerDiedSignal>(OnPlayerDied, signal => signal.PlayerId == LocalPlayerId);
    }

    private bool OnPlayerDied(PlayerDiedSignal signal)
    {
        // Only handle local player death
        return signal.PlayerId == _localPlayerId;
    }
}
```

### Dependency Injection

```csharp
using Zenject;

public class GameInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<SignalBus>().AsSingle();
        Container.Bind<KeySignalBus<string>>().AsSingle();
    }
}

public class PlayerController : MonoBehaviour
{
    [Inject] private readonly SignalBus _signalBus;

    // Signal bus is automatically injected
}
```

---

## 📚 API Reference

### ISignalBus Interface

```csharp
public interface ISignalBus
{
    void Subscribe<TSignal>(SignalReceivedDelegate<TSignal> receiver);
    void Unsubscribe<TSignal>(SignalReceivedDelegate<TSignal> receiver);
    void Invoke<TSignal>(TSignal signal);
    int GetSubscriberCount<TSignal>();
    bool HasSubscribers<TSignal>();
    void ClearAll();
    void Clear<TSignal>();
}
```

### IKeySignalBus<TKey> Interface

```csharp
public interface IKeySignalBus<TKey>
{
    void Subscribe<TSignal>(TKey key, SignalReceivedDelegate<TSignal> receiver);
    void Unsubscribe<TSignal>(TKey key, SignalReceivedDelegate<TSignal> receiver);
    void Invoke<TSignal>(TKey key, TSignal signal);
    int GetSubscriberCount<TSignal>(TKey key);
    bool HasSubscribers<TSignal>(TKey key);
    void Clear(TKey key);
    void Clear<TSignal>(TKey key);
    void ClearAll();
    TKey[] GetActiveKeys();
}
```

### SignalReceivedDelegate

```csharp
public delegate void SignalReceivedDelegate<in TSignal>(TSignal signal);
```

---

## 🎯 Best Practices

### ✅ Do's

- **Always unsubscribe** in `OnDestroy()` to prevent memory leaks
- **Use descriptive signal names** that indicate their purpose
- **Make signals immutable** (use `readonly struct`)
- **Use keyed signals** for component-specific messaging
- **Handle exceptions** in signal handlers gracefully
- **Use interfaces** for dependency injection and testing

### ❌ Don'ts

- **Don't modify collections** during signal iteration
- **Don't use signals for high-frequency updates** (use direct method calls instead)
- **Don't forget to unsubscribe** from signals
- **Don't make signals mutable** (avoid reference types for signal data)
- **Don't block** in synchronous signal handlers

### 📏 Signal Design Guidelines

```csharp
// ✅ Good: Immutable, descriptive, focused
public readonly struct PlayerScoredSignal
{
    public readonly int PlayerId;
    public readonly int Points;
    public readonly string Reason;

    public PlayerScoredSignal(int playerId, int points, string reason)
    {
        PlayerId = playerId;
        Points = points;
        Reason = reason;
    }
}

// ❌ Bad: Mutable, unclear purpose, too broad
public class GameEventSignal
{
    public string EventType;
    public object Data;
}
```

---

## 🧪 Testing

```csharp
using NUnit.Framework;
using AleVerDes.Signals;

public class SignalBusTests
{
    private SignalBus _signalBus;
    private bool _signalReceived;

    [SetUp]
    public void Setup()
    {
        _signalBus = new SignalBus();
        _signalReceived = false;
    }

    [Test]
    public void SignalIsReceivedBySubscriber()
    {
        _signalBus.Subscribe<TestSignal>(signal => _signalReceived = true);
        _signalBus.Invoke(new TestSignal());

        Assert.IsTrue(_signalReceived);
    }

    private readonly struct TestSignal { }
}
```

---

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- Inspired by [yankooliveira/signals](https://github.com/yankooliveira/signals)
- Built with ❤️ for the Unity community

---

## 📞 Support

- 📧 **Email**: aleverdes@example.com
- 🐛 **Issues**: [GitHub Issues](https://github.com/aleverdes/signals/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/aleverdes/signals/discussions)

---

*Made with ❤️ by [AleVerDes](https://github.com/aleverdes)*