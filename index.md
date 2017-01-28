# Microsoft.Services.Core
This NuGet package contains core libraries that are used within the platform that **do not** have any external dependencies.

### [Collections](xref:Microsoft.Services.Core.Collections)
Set of generic collections that are needed for service development. Example is [`ConcurrentHashSet`](xref:Microsoft.Services.Core.Collections.ConcurrentHashSet`1)

### [Process Management](xref:Microsoft.Services.Core.Process)
Contains everything Win32 process related

* [Windows job objects](xref:Microsoft.Services.Core.Process.JobObject). Here is a [detailed explanation](articles/processManagement-readme.md)

### [String value attribute](xref:Microsoft.Services.Core.StringValueAttribute)
C# enums by default cannot take string values. This solves that problem by providing two classes:
* [StringValueAttribute](xref:Microsoft.Services.Core.StringValueAttribute)
* [StringValueAttrbuteExtensions](xref:Microsoft.Services.Core.StringValueAttributeExtensions)

### [ExceptionHelper](xref:Microsoft.Services.Core.ExceptionHelper)
Generic exception helper methods to help users search through and format exception strings.

### [Contract](xref:Microsoft.Services.Core.Contract)
A contracts class that mimics the functionality of [System.Diagnostics.Contracts](xref:System.Diagnostics.Contracts). 

This is a class used to do basic argument checks in a less verbose way.

This will also print out the source location of the statement.

```csharp
Contract.Requires<ArgumentNullException>(elements != null, "elements cannot be null");
...
Contract.AssertArgNotNull(elements,nameof(elements));
...
Contract.AssertArgNotNullOrEmptyOrWhitespace(myString, nameof(mystring));
```
### [Retries](xref:Microsoft.Services.Core.Retries)
Abstractions for retry policies and stock implementations of commonly used startegies [here](articles/Retries-ReadMe.md).