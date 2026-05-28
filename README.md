# ProObjLogLite

ProObjLogLite is a fast CLI logger for scripts, apps, and automation pipelines.

- Works across Linux, macOS, and Windows.
- Works from any language that can run a process.
- Supports pretty console logs and JSON logs.

## Install

### Requirements

- .NET SDK 10+

### Linux / macOS

`install.sh` auto-detects Homebrew-installed `dotnet` paths on macOS and Linux.

```bash
./scripts/install.sh
```

Default install path: `~/.local/bin/ProObjLogLite`

Custom install path:

```bash
./scripts/install.sh /usr/local/bin
```

Uninstall:

```bash
./scripts/uninstall.sh
```

### Windows (PowerShell)

```powershell
.\scripts\install.ps1
```

Default install path: `%LOCALAPPDATA%\Programs\ProObjLogLite\ProObjLogLite.exe`

Custom install path:

```powershell
.\scripts\install.ps1 -InstallDir "C:\Tools\ProObjLogLite"
```

Uninstall:

```powershell
.\scripts\uninstall.ps1
```

## Quick Start

```bash
ProObjLogLite --level INFO --source web-api --message "Server started"
```

JSON output:

```bash
ProObjLogLite --json --source payments --message "Charge accepted"
```

Read from stdin:

```bash
printf "build complete" | ProObjLogLite --message - --source ci --json
```

## Serious World Test Tool

Run the Python integration/stress tool that exercises smart logging, logic logging,
timers, chain hashing, sampling, redaction, metrics, decisions, profiles, and
concurrency:

```bash
python3 tools/proobjlog_world_tester.py --bin ProObjLogLite --workers 6 --iterations 30
```

Keep generated artifacts for inspection:

```bash
python3 tools/proobjlog_world_tester.py --bin ProObjLogLite --keep
```

## Features

- Standard levels: `TRACE`, `DEBUG`, `INFO`, `WARN`, `ERROR`, `FATAL`
- Minimum level filtering: `--min-level`
- JSON mode: `--json`
- Source tags: `--source` / `--tag`
- Event tracing: `--event-id`, `--correlation-id` / `--cid`
- Extra metadata: repeatable `--context key=value`
- Exception field: `--exception`
- UTC + custom timestamp format: `--utc`, `--timestamp-format`
- Disable ANSI colors: `--no-color`
- File rotation: `--max-size`, `--max-files`
- Explicit output file path: `--file`
- Console-only mode: `--stdout-only`
- Dry run mode: `--dry-run`
- Repetition counter: `--count 250`
- Tamper-evident chain hashes: `--chain-hash`
- Built-in named timers: `--timer build --timer-start` then `--timer build --timer-stop`
- Environment presets: `--profile dev|ci|prod`
- Probabilistic sampling: `--sample-rate 0.25`
- Secret redaction: `--redact-keys password,token,apiKey`
- Log type system: `--type event|audit|metric|decision|logic`
- Smart inference mode: `--smart`
- Logic conditions: `--when "env == prod"`, `--assert "status == ok"`
- Decision and metric helpers: `--decision`, `--outcome`, `--metric latency_ms=123`
- Async low-latency writer (default), optional `--sync`
- Fire-and-forget mode: `--fire-and-forget`
- Config template creation: `--init-config`
- Installed version output: `--version`
- Supported levels output: `--list-levels`

## CLI Usage

```text
ProObjLogLite [OPTIONS]
```

Core:

- `-d, --dir <path>` output directory (default: `logs`)
- `-l, --level <lvl>` current log level
- `-m, --message <msg>` message text (`-` reads stdin)
- `-n, --noprint` write file only
- `-h, --help` show help
- `-v, --version` show version
- `--list-levels` print all supported levels
- `--init-config` create `proobjloglite.json`

Formatting:

- `--json`
- `-s, --source <name>`
- `--utc`
- `--timestamp-format <fmt>`
- `--no-color`
- `--stdout-only`
- `--dry-run`
- `--count <n>`
- `--chain-hash`
- `--timer <name>`
- `--timer-start`
- `--timer-stop`
- `--profile <dev|ci|prod>`
- `--sample-rate <0..1>`
- `--redact-keys a,b,c`
- `--type <event|audit|metric|decision|logic>`
- `--smart`
- `--when <expr>`
- `--assert <expr>`
- `--metric name=value`
- `--decision <name>`
- `--outcome <value>`
- `--sync`
- `--fire-and-forget`

Storage:

- `-f, --file <path>`
- `--min-level <lvl>`
- `--max-size <bytes|KB|MB|GB>`
- `--max-files <n>`

Advanced:

- `--event-id <id>`
- `--correlation-id <id>` or `--cid <id>`
- `--exception <text>`
- `--context key=value` (repeat)

## Config File

Create a config quickly:

```bash
ProObjLogLite --init-config
```

You can also write your own `proobjloglite.json` in the current directory:

```json
{
  "directory": "logs",
  "level": "INFO",
  "minLevel": "TRACE",
  "jsonOutput": false,
  "source": "default",
  "utc": false,
  "timestampFormat": "yyyy-MM-ddTHH:mm:ss.fffK",
  "maxFiles": 7
}
```

CLI flags override config values.

## Cross-Language Examples

Python:

```python
import subprocess

subprocess.run([
    "ProObjLogLite",
    "--json",
    "--source", "payments",
    "--event-id", "PAY-2001",
    "--context", "orderId=1234",
    "--message", "Charge accepted"
], check=True)
```

Node.js:

```javascript
import { spawn } from "node:child_process";

spawn("ProObjLogLite", [
  "--stdout-only",
  "--json",
  "--source", "frontend",
  "--message", "UI loaded"
]);
```

Bash:

```bash
ProObjLogLite --level ERROR --source worker --exception "Timeout" --message "Job failed"
```

## Exit Codes

- `0`: success
- `1`: argument or parsing failure
- `2`: write failure
