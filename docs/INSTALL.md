# Install Guide

## Linux / macOS

1. Ensure `.NET SDK 10+` is installed.
2. Run `./scripts/install.sh` from repo root.
3. Verify with `ProObjLogLite --version`.

Optional custom path:

```bash
./scripts/install.sh /usr/local/bin
```

Uninstall:

```bash
./scripts/uninstall.sh
```

## Windows

1. Ensure `.NET SDK 10+` is installed.
2. Open PowerShell at repo root.
3. Run `.\scripts\install.ps1`.
4. Open a new terminal and run `ProObjLogLite --version`.

Optional custom path:

```powershell
.\scripts\install.ps1 -InstallDir "C:\Tools\ProObjLogLite"
```

Uninstall:

```powershell
.\scripts\uninstall.ps1
```
