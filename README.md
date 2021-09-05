# SharpMixin

[![Nuget](https://img.shields.io/nuget/vpre/SharpMixin.Generator?style=plastic)](https://www.nuget.org/packages/SharpMixin.Generator/)

Source generator for creating mixins in c#

## Installation

Two packages are requrired:

```bash
dotnet add package SharpMixin.Generator --version 1.0.0-preview1
dotnet add package SharpMixin.Attributes --version 1.0.0-preview1
```

First contains actual Source Generator, which is used as developer dependency. Second contains metadata compiled added to your assembly.

## Usage

1. Declare class with `[Mixin]` attribute and interfaces you need:

    ```cs
    [Mixin]
    public partial class SharpMixin : IFoo, IBar { }
    ```

1. Create instance and provide implementations of your interfaces:

    ```cs
    var fooBar = new SharpMixin(myFoo, myBar);
    ```

1. Use your mixin 😋

    ```cs
    barConsumer.Consume(fooBar);
    fooConsumer.Consume(fooBar);
    fooBar.FooProperty = 5;
    ```

1. Augument your mixin as necessary

   ```cs
   [Mixin][ConstructUsing(typeof(SharpMixin))]
   public partial class FooBarBaz : IFoo, IBar, IBaz {}

   var fooBarBaz = new FooBarBaz(fooBar, baz);
   ```

## How it works

Behind the scenes field is generated for each interface implmentation,
and all interface calls are proxied to the right field.
Generated code looks like this:

```cs
partial class SharpMixin 
{ 
    public SharpMixin(IFoo foo, IBar bar)
    {
        this.foo = foo;
        this.bar = bar;
    }

    private IFoo foo;
    private IBar bar;

    public void DoFoo(){
        foo.DoFoo();
    }

    public string DoBar(string arg){
        return bar.DoBar(arg);
    }

    public FooProperty {
        get => foo.FooProperty;
        set => foo.FooProperty = value;
    }
}
```
