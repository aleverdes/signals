# AleVerDes.Signals

### Simple EventBus-pattern implementation for Unity with Coroutine support.

---

A simple library for working with the message bus. 
Supports a CustomYieldInstruction for coroutines.

*Inspired by [yankooliveira](https://github.com/yankooliveira)/[Signals](https://github.com/yankooliveira/signals).*

### Usage

Define signals:
```c#
public struct PlayParticlesSignal
{
    public string Name;
    public int Count;
}

public struct DebugSignal
{
}

public struct ChatMessageSignal
{
    public string Text;
}
```

Signal subscription:
```c#
[Inject] private readonly SignalBus _signalBus;
[Inject] private readonly KeySignalBus<string> _idSignalBus;

void Start()
{
    _signalBus.Subscribe<PlayParticlesSignal>(OnPlayParticlesSignal);
    _idSignalBus.Subscribe<PlayParticlesSignal>("invokedOnlyForThisId", OnPlayParticlesSignal);
}

void OnDestroy()
{
    _signalBus.Unsubscribe<PlayParticlesSignal>(OnPlayParticlesSignal);
    _idSignalBus.Unsubscribe<PlayParticlesSignal>("invokedOnlyForThisId", OnPlayParticlesSignal);
}

void OnPlayParticlesSignal(PlayParticlesSignal signal)
{
    // play animation
}
```

Signal raise:
```c#
_signalBus.Invoke(new()
{
    Text = "test"
});

_idSignalBus.Invoke("invokedOnlyForThisId", new()
{
    Text = "test"
});
```

### Coroutine usage

```c#
private IEnumerator Start()
{
    Debug.Log("Start-coroutine started");
    
    yield return new WaitForSignal<WinSignal>();
    yield return new WaitForKeySignal<WinSignal>("invokedOnlyForThisId");
    
    Debug.Log("Next frame after WinSignal was invoked")
}
```