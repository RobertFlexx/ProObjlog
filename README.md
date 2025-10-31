# ProObjLog

> Logging, Universalized

## Notice

This repository is under active development. The main branch may not always be perfectly stable — use tagged releases for production builds.

## What is this?

**ProObjLog** is a modern, cross-language logging tool written in **C#**, inspired by [Kokonico’s ObjLog](https://github.com/Kokonico/ObjLog).
It’s designed to be simple, fast, and universal — usable from **any programming language** via a single executable.

No Python runtime. No dependencies. Just one binary.

---

## What dependencies do I need?

None. The C# build runs on .NET 9.0 and ships as a self-contained binary when published.

To build:

```bash
dotnet build
```

To publish standalone:

```bash
dotnet publish -c Release -o out
```

---

## How do I use it?

You can use it directly from the command line:

```bash
./ProObjLog INFO "This is an info message"
./ProObjLog WARN "This is a warning message"
./ProObjLog ERROR "This is an error message"
./ProObjLog FATAL "This is a fatal message"
```

Output:

```shell
[prObjLog] [2025-10-31 08:45:15.310] INFO: This is an info message
[prObjLog] [2025-10-31 08:45:15.311] WARN: This is a warning message
[prObjLog] [2025-10-31 08:45:15.312] ERROR: This is an error message
[prObjLog] [2025-10-31 08:45:15.313] FATAL: This is a fatal message
```

(Colored in the console!)

---

## Log to a File

By default, ProObjLog automatically creates daily log files under:

```text
logs/YYYY-MM-DD.log
```

Example:

```bash
./ProObjLog INFO "Logging to a daily file"
```

`logs/2025-10-31.log`:

```shell
[prObjLog] [2025-10-31 08:46:02.200] INFO: Logging to a daily file
```

---

## Use It From Other Languages

### Perl:

```bash
perl perl/probjlog.pl WARN "Perl talking to C# logger"
```

### Python:

```python
import subprocess
subprocess.run(["./ProObjLog", "ERROR", "Python reporting in!"])
```

### Bash:

```bash
./ProObjLog DEBUG "Shell logging works too"
```

All of these write to the same file and console output — there’s only **one real logger**.

---

## Custom Message Types (C#)

You can extend the logger if you’re embedding it in a C# project:

```csharp
using ProObjLog.Core;

public sealed class CustomMessage : LogMessage
{
    public CustomMessage(string message) : base(message)
    {
        Level = "CUSTOM";
    }

    public override string Colorize(string text) => $"\u001b[96m{text}\u001b[0m"; // cyan
}

// Example usage
var log = new LogNode(name: "Custom Example", printToConsole: true);
log.Log(new CustomMessage("This is a custom message!"));
```

Output:

```shell
[Custom Example] [2025-10-31 08:46:03.120] CUSTOM: This is a custom message
```

---

## Buffered Example

Messages are stored in memory (default 500). You can print them later or dump to a file programmatically.

```csharp
var log = new LogNode(name: "Buffered Example", printToConsole: false);

log.Log(new InfoMessage("Buffered message 1"));
log.Log(new WarnMessage("Buffered message 2"));

foreach (var msg in log.GetMessages())
    Console.WriteLine(msg.Colorize(msg.Format()));
```

Output:

```shell
[Buffered Example] [2025-10-31 08:46:05.050] INFO: Buffered message 1
[Buffered Example] [2025-10-31 08:46:05.051] WARN: Buffered message 2
```

---

## Limit Stored Messages

You can control how many messages are kept in memory and file:

```csharp
var log = new LogNode(name: "Limited Example", maxMessagesInMemory: 5, printToConsole: true);

log.Log(new InfoMessage("message 1"));
log.Log(new InfoMessage("message 2"));
log.Log(new InfoMessage("message 3"));
```

Older messages are automatically discarded once the limit is reached.

---

## Cross-Language Logging

ProObjLog’s design lets you unify logs from any program or script into a single file and timestamped stream.
It’s the same concept as ObjLog, but without needing a running daemon or per-language bindings.

---

## Why C#?

* Compiles to one fast binary
* Zero runtime dependencies
* Native color, file, and date handling
* Perfect for multilingual environments

---

## License

This project is released under the **Zlib license**, just like ObjLog — use it freely in any project as long as you credit the original author.

---

## Contributing

Fork, improve, and submit a PR! Improvements like JSON output, log filtering, or socket-based daemon mode are welcome!

* [Contributing Guidelines](CONTRIBUTING.md)
* [Security Policy](SECURITY.md)

---

## Credits

**Original Inspiration:** [Kokonico’s ObjLog](https://github.com/Kokonico/ObjLog)
This project is a multilingual reimagining of that concept — rebuilt in C# for universal compatibility.

---

## Summary

**ProObjLog** brings ObjLog’s simplicity to *every language* — one logger, one format, one place to read everything. :D
