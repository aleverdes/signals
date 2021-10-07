# AffenSignals

### Simple EventBus-pattern implementation for Unity with Coroutine support.

---

A simple library for working with the message bus. 

It uses strict typing to declare event types.
Events can have parameters (up to four parameters, but this can be fraught with exhaustive generic code generation). 
There is a YieldInstruction for coroutines.
It is possible to subscribe a method without parameters to a parameterized signal.

Inspired by *[yankooliveira](https://github.com/yankooliveira) / [Signals](https://github.com/yankooliveira/signals)*.

### Usage

Define signal-classes:
```c#
public class WinSignal : Signal {}
public class LoseSignal : Signal {}
public class TakeDamageSignal : Signal<int> {}
public class ChatMessageSignal : Signal<string, string, string> {}
```

Signal subscription:
```c#
void Start()
{
    Signals.Get<WinSignal>().AddListener(OnWin);
    Signals.Get<TakeDamageSignal>().AddListener(PlayWoundAnimation);
    Signals.Get<TakeDamageSignal>().AddListener(DecreaseHealth);
}

void OnDestroy()
{
    Signals.Get<WinSignal>().RemoveListener(OnWin);
    Signals.Get<TakeDamageSignal>().RemoveListener(PlayWoundAnimation);
    Signals.Get<TakeDamageSignal>().RemoveListener(DecreaseHealth);
}

void OnWin()
{
    Debug.Log("win!!!");
}

void PlayWoundAnimation()
{
    // play animation
}

void DecreaseHealth(int damage)
{
    // health -= damage;
}
```

Signal raise:
```c#
Signals.Get<ChatMessageSignal>().Invoke("John Smith", "Subject", "Hello World!");
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

### Local signal container

Instead of using a global message bus, you can declare a local one, for example, only for the player and its parts, or only for the UI.

```c#
private void Awake()
{
    SignalContainer localSignalContainer = new SignalContainer();
    localSignalContainer.Get<TakeDamageSignal>().AddListener(OnTakeDamage);
}
```