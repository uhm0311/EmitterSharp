# EmitterSharp
C# implementation of [Emitter](https://github.com/component/emitter) in JavaScript module.

# Installation
- [Nuget gallery](https://www.nuget.org/packages/EmitterSharp)

- Command `Install-Package EmitterSharp` in nuget package manager console.

# Usage
## Namespace ##
```csharp
using EmitterSharp;
```

## Inheritance ##
```csharp
class ExampleEmitter : Emitter<ExampleEmitter, string, object>
{
  ...
}
```

### Emitter<TChildClass, TEvent, TArgument> ###
```csharp
public abstract class Emitter<TChildClass, TEvent, TArgument>
```

- The first generic type is **a type of class that inherits Emitter**. This is used for chaining.
- The second generic type is **a type of event**.
- The third generic type is **a type of argument in callback**.

## Listener ##
```csharp
void ExampleCallback() 
{
  Console.WriteLine("This is callback.");
}

void ExampleCallbackWithArgument(object value) 
{
  if (value != null) 
  {
    Console.WriteLine("This is callback : " + value);
  }
  else
  {
    Console.WriteLine("This is callback... Wait, what?");
  }
}

ExampleEmitter emitter = new ExampleEmitter();

emitter.On("custom event", () => Console.WriteLine("Hello world!")); // Callback can be lambda without argument.
emitter.On("custom event", (value) => // Callback can be lambda with argument.
{
  if (value != null)
  {
    Console.WriteLine(value);
  }
  else 
  {
    Console.WriteLine("Excuse me?");
  }
});

emitter.On("custom event", ExampleCallback); // Callback can be method without argument.
emitter.On("custom event", ExampleCallbackWithArgument); // Callback can be method with argument.

emitter.Emit("custom event"); // Emit without argument.
// Console will print "Hello world!", "Excuse me?", "This is callback." and "This is callback... Wait, what?"

emitter.Emit("custom event", 30); // Emit with argument.
// Console will print "Hello world!", 30, "This is callback." and "This is callback : 30"
```

### Emit without argument ###
- When `Emitter.Emit` is called only with event, default value of argument is always `0`.
- If `TArgument` is `bool`, default value of argument is `false` since it's defined as `0`.
- If `TArgument` is `string`, default value of argument is `null` since it's a reference to address `0`.
- If `TArgument` is `IntPtr`, default value of argument is `IntPtr.Zero` that is defined as `null` reference.

# Maintenance
Welcome to report issue or create pull request. I will check it happily.

# Dependencies
There is no dependencies in `EmitterSharp`.

# License
`EmitterSharp` is under [The MIT License](https://github.com/uhm0311/EmitterSharp/blob/master/LICENSE).
