# AffenSignals

### Simple EventBus-pattern implementation for Unity with Coroutine support.

---

A simple library for working with the message bus. 

It uses strict struct-typing to declare event types. 
There is a YieldInstruction for coroutines.
It is possible to subscribe a method without parameters to a parameterized signal.

*Inspired by [yankooliveira](https://github.com/yankooliveira) / [Signals](https://github.com/yankooliveira/signals).*

### Usage

Define signal-classes:
```c#
public struct PlayParticlesSignal : ISignal
{
    public string Name;
    public int Count;
}

public struct DebugSignal : ISignal
{
}

public struct ChatMessageSignal : ISignal
{
    public string Text;
}
```

Signal subscription:
```c#
void Start()
{
    Signals<PlayParticlesSignal>.AddListener(OnWin);
    Signals<PlayParticlesSignal>.AddListener(PlayParticlesSignal);
}

void OnDestroy()
{
    Signals<PlayParticlesSignal>.RemoveListener(OnWin);
    Signals<PlayParticlesSignal>.RemoveListener(PlayParticlesSignal);
}

void OnWin()
{
    Debug.Log("win!!!");
}

void PlayAnimation(PlayParticlesSignal signal)
{
    // play animation
}
```

Signal raise:
```c#
Signals<ChatMessageSignal>().Invoke(new()
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
    
    Debug.Log("Next frame after WinSignal was invoked")
}
```