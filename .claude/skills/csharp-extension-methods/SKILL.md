---
name: csharp-extension-methods
description: A skill for generating C# extension methods. Use this whenever you are going to generate an extension method in C#
---

Avoid `this` keyword for extension methods. Prefer extension blocks from C# 14 instead.

You can also use the Microsoft Learn MCP server to learn more about extension blocks in C# >= 14.

## Bad

```csharp
public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string str)
    {
      return string.IsNullOrEmpty(str);
    }
}
```

## Good

```csharp
public static class StringExtensions
{
  extension(string str)
  {
    public bool IsNullOrEmpty()
    {
      return string.IsNullOrEmpty(str);
    }
  }
}
