---
name: csharp-coding-guidelines
description: Coding guidelines for C# development. Use whenever you generate C# code.
---

## Single-Line Statements

Always use braces `{}` for single-line statements, even if they are not required.

Bad:

```csharp
if (condition)
    DoSomething();
```

Good:

```csharp
if (condition)
{
    DoSomething();
}
```