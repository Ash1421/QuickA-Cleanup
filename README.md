# 🧹 QuickA-Cleanup

> **Smart Quick Access Navigation Pane cleaner for Windows Explorer — dual GUI + CLI, self-contained, and fast.**

---

## ✨ Socials & Stars

[![Discord Server Invite](https://img.shields.io/badge/Discord-Server%20Invite-7289DA?style=for-the-badge&logo=discord&logoColor=white&color=blueviolet&labelColor=1c1917)](https://rb.ash1421.com/discord)
[![GitHub Stars](https://img.shields.io/github/stars/Ash1421/QuickA-Cleanup?style=for-the-badge&logo=github&logoColor=white&labelColor=1c1917&color=gold)](https://github.com/Ash1421/QuickA-Cleanup/stargazers)

## 📦 Repository Information

[![Latest Version](https://img.shields.io/github/v/release/Ash1421/QuickA-Cleanup?style=for-the-badge&label=Latest%20Version&logo=github&logoColor=white&labelColor=1c1917&color=6829B1)](https://github.com/Ash1421/QuickA-Cleanup/releases/latest)
[![CodeFactor Grade](https://img.shields.io/codefactor/grade/github/ash1421/QuickA-Cleanup?style=for-the-badge&logo=codefactor&logoColor=white&labelColor=1c1917&color=6829B1)](https://www.codefactor.io/repository/github/ash1421/QuickA-Cleanup)
[![Total Downloads](https://img.shields.io/github/downloads/Ash1421/QuickA-Cleanup/total?style=for-the-badge&logo=github&logoColor=white&labelColor=1c1917&color=6829B1&label=Total%20Downloads)](https://github.com/Ash1421/QuickA-Cleanup/releases)
[![Downloads at Latest](https://img.shields.io/github/downloads/Ash1421/QuickA-Cleanup/latest/total?style=for-the-badge&logo=github&logoColor=white&labelColor=1c1917&color=6829B1&label=Downloads%20@%20Latest)](https://github.com/Ash1421/QuickA-Cleanup/releases/latest)
[![GitHub Issues](https://img.shields.io/github/issues/Ash1421/QuickA-Cleanup/open?style=for-the-badge&labelColor=1c1917&logo=github&logoColor=white)](https://github.com/Ash1421/QuickA-Cleanup/issues)
[![Closed Issues](https://img.shields.io/github/issues-closed/Ash1421/QuickA-Cleanup/closed?style=for-the-badge&color=red&labelColor=1c1917&logo=github&logoColor=white)](https://github.com/Ash1421/QuickA-Cleanup/issues?q=is:closed)
[![New Issue](https://img.shields.io/badge/Open%20A%20New-Issue-orange?style=for-the-badge&labelColor=1c1917&logo=github&logoColor=white)](https://github.com/Ash1421/QuickA-Cleanup/issues/new)

## ❤️ Made With Love Using

[![.NET](https://img.shields.io/badge/Built%20With-.NET%209-512BD4?style=for-the-badge&logo=dotnet&logoColor=white&labelColor=1c1917)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logoColor=white&labelColor=1c1917&logo=C#)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![WinUI 3](https://img.shields.io/badge/WinUI%203-Windows%20App%20SDK-512BD4?style=for-the-badge&logo=windows&logoColor=white&labelColor=1c1917)](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)
[![Windows](https://img.shields.io/badge/Platform-Windows%20Only-0078D4?style=for-the-badge&logo=windows&logoColor=white&labelColor=1c1917)](https://learn.microsoft.com/en-us/windows/)
[![GNU](https://img.shields.io/badge/GNU-A42E2B?style=for-the-badge&logo=gnu&logoColor=white&labelColor=1c1917)](https://www.gnu.org/)
[![Shields.io](https://img.shields.io/badge/Shields.io-4C9B3F?style=for-the-badge&logo=shields.io&logoColor=white&labelColor=1c1917)](https://shields.io/)
[![CodeFactor](https://img.shields.io/badge/Code%20Factor-E05C2A?style=for-the-badge&logo=codefactor&logoColor=white&labelColor=1c1917)](https://www.codefactor.io/)

## 📜 Licensed Under

[![License: GPL v3.0](https://img.shields.io/badge/License-GPL%20v3.0-6829B1.svg?style=for-the-badge&labelColor=1c1917&logo=gnu&logoColor=white)](./LICENSE)

---

## 👤 For Regular Users

### What does this do?

When you open **File Explorer**, the left sidebar shows a list of folders — things like OneDrive, Desktop, Downloads, and entries left behind by apps you've already uninstalled. Windows gives you no easy built-in way to remove these leftovers.

**QuickA-Cleanup** scans for those entries and lets you remove them in a few clicks. It backs everything up automatically before touching anything, and restarts Explorer when done — available as both a modern GUI app and a command-line tool.

---

### 🚀 Getting Started — 3 Steps

**1. Download**

Go to the [**Latest Release**](https://github.com/Ash1421/QuickA-Cleanup/releases/latest) and grab the right file for your PC:

| I have a...                       | GUI                                       | CLI                                       |
| --------------------------------- | ----------------------------------------- | ----------------------------------------- |
| Normal Windows PC _(most people)_ | `QuickA-Cleanup-GUI-VX.X.X-win-x64.zip`   | `QuickA-Cleanup-CLI-VX.X.X-win-x64.exe`   |
| Older 32-bit PC                   | `QuickA-Cleanup-GUI-VX.X.X-win-x86.zip`   | `QuickA-Cleanup-CLI-VX.X.X-win-x86.exe`   |
| Windows on ARM                    | `QuickA-Cleanup-GUI-VX.X.X-win-arm64.zip` | `QuickA-Cleanup-CLI-VX.X.X-win-arm64.exe` |

> **Not sure?** Grab **win-x64** — it works on most Windows PCs. Use the GUI unless you prefer the terminal.

**2. Run it**

**GUI:** extract the `.zip` to its own folder, then run `QuickA-Cleanup-GUI.exe` inside it (the app needs its supporting files alongside it — don't move just the `.exe` on its own).
**CLI:** just run the `.exe` directly, no extraction needed.

If Windows shows a SmartScreen warning click **More info → Run anyway**.
The app will ask for **Administrator** permission — required to read and modify registry entries.

**3. Clean up**

- Click **Scan** to find navigation pane entries
- Tick the ones you want to remove
- Click **Remove Selected** and confirm

Explorer restarts automatically and the entries will be gone. ✅

> [!NOTE]
> **Changed your mind?** A backup file named `QuickA-Backup-<timestamp>.reg` is automatically saved next to the app every time you remove something (in the GUI's folder, or next to the `.exe` for CLI). Double-click it and click **Yes** to restore everything instantly.

---

### 🏷️ What Gets Flagged Automatically?

QuickA-Cleanup recognises these common entries and highlights them as **Bloatware** in the item list:

| Name                  | Added by                      |
| --------------------- | ----------------------------- |
| OneDrive — Personal   | Microsoft OneDrive            |
| OneDrive for Business | Microsoft 365 / work accounts |
| SharePoint            | Microsoft 365                 |

Everything else shows as **Unknown** — typically entries left by third-party apps.

> [!TIP]
> Essential system folders (Desktop, Documents, Downloads, Pictures, Music, Videos) are **always hidden** from the list and cannot be accidentally removed.

---

### ❓ FAQ

<details>
<summary><b>Is this safe to use?</b></summary>
<br>

Yes. Before removing anything the tool saves a `.reg` backup automatically. If you want to undo changes, double-click the backup file and click Yes. You can also use **Dry Run** mode to preview everything before committing.

</details>

<details>
<summary><b>Will this delete my actual files or folders?</b></summary>
<br>

No. It only removes the **shortcut entries** from the navigation pane. Your files and folders on disk are never touched.

</details>

<details>
<summary><b>Why does it need Administrator permission?</b></summary>
<br>

Navigation pane entries are stored in the Windows Registry. Reading and writing registry keys requires elevated permissions — the same as installing or uninstalling software.

</details>

<details>
<summary><b>File Explorer looks wrong after running it — what do I do?</b></summary>
<br>

The tool restarts Explorer automatically. If something still looks off, restart it manually:

1. Press `Ctrl + Shift + Esc` → Task Manager
2. Find **Windows Explorer** → Right-click → **Restart**

To restore removed entries, find the `QuickA-Backup-*.reg` file next to the app (in the GUI's folder, or next to the `.exe` for CLI), double-click it, and click **Yes**.

</details>

<details>
<summary><b>SmartScreen is blocking the app — is it a virus?</b></summary>
<br>

No. SmartScreen warns about any `.exe` without a paid code-signing certificate. QuickA-Cleanup is fully open source — every line of code is in this repository. Click **More info → Run anyway**.

</details>

<details>
<summary><b>How do I test it without risking anything?</b></summary>
<br>

Use **Settings → Testing** in the GUI — download and install dummy test entries with one click, scan to see them appear, then remove them. Or use the `testpins.reg` file from this repo directly: double-click to import, then use QuickA-Cleanup to remove them.

</details>

---

## 🔧 For Advanced Users & Developers

<details>
<summary><b>📋 Full Feature List</b></summary>
<br>

**GUI:**

- Native WinUI 3 window — real Windows 11 caption buttons (minimize/maximize/close, snap layouts included), Mica backdrop
- Light / Dark / "Match Windows" theme, plus a picker of 8 accent colors that apply live across the app
- Three always-visible status indicator dots (Bloat / Test pins / Errors)
- Item list with a colored type badge per row (Bloatware / Test / Unknown), native list add/remove animation
- Progress bar + live sub-text during scan and removal
- Dry Run and Backup toggles in the scan bar
- Settings is a native dialog (`ContentDialog`), not a separate window

**Settings dialog:**

- Appearance section — theme mode + accent color swatches
- Testing section — download + install + remove testpins with live status text
- Log section — TRACE/DEV/WARN/ERROR level filter, expandable log viewer, clear button, writes to `QuickA-Cleanup.log`

**Core:**

- Parallel registry scanning via `Parallel.ForEach` + `ConcurrentBag`
- Known bloatware tagging (OneDrive, OneDrive for Business, SharePoint)
- Testpin detection on startup with status bar warning
- Protected system folder blacklist (Desktop, Documents, Downloads, Pictures, Music, Videos)
- User-defined custom folder protection list in `ItemFilter.cs`
- Automatic `.reg` backup before any deletion
- Explorer restart after removal
- Custom app icon (`explorer_clean_x64x64.ico`)

**CLI:**

- Same scan/remove/backup/dry-run/restore logic as GUI
- `--dry-run`, `--no-backup`, `--help` flags
- Color-coded table output with bloatware tagging
- Trimmed, self-contained single-file publish

</details>

<details>
<summary><b>🗂️ How It Works — Registry Internals</b></summary>
<br>

Windows stores navigation pane entries under:

```
HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace
```

Each sub-key is a GUID. The `(Default)` value is the display name shown in File Explorer. QuickA-Cleanup enumerates these sub-keys, filters out protected GUIDs and user-defined custom folder names, then presents the remainder as removable.

Removal is done via `Registry.CurrentUser.OpenSubKey(..., writable: true)` → `DeleteSubKeyTree`. The backup step serialises the keys into a valid `.reg` file using Unicode encoding before any deletion.

Explorer is restarted by calling `Process.Kill()` on all `explorer.exe` instances, sleeping 800ms, then `Process.Start("explorer.exe")`.

</details>

<details>
<summary><b>🛠️ Customisation Guide</b></summary>
<br>

All filtering logic lives in `Core/Services/ItemFilter.cs`.

**Add a GUID to the protected blacklist** (never shown, never removable):

```csharp
private static readonly HashSet<string> Blacklist =
    new(StringComparer.OrdinalIgnoreCase)
    {
        "{YOUR-GUID-HERE}",  // description
        // ...
    };
```

**Register a known bloatware entry** (shown with amber highlight and "Bloatware" tag):

```csharp
private static readonly Dictionary<string, string> KnownItems =
    new(StringComparer.OrdinalIgnoreCase)
    {
        { "{YOUR-GUID-HERE}", "Friendly Display Name" },
        // ...
    };
```

**Protect a custom folder by name** (case-insensitive, uses `Contains`):

```csharp
private static readonly string[] CustomFolders =
{
    "My Folder Name",
    // ...
};
```

**Finding a GUID:** open `regedit.exe` and navigate to:

```
HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace
```

Each sub-key is a GUID. The `(Default)` value is the display name.

</details>

<details>
<summary><b>📦 Build Instructions</b></summary>
<br>

Requires [.NET SDK 9.0](https://dotnet.microsoft.com/en-us/download).

Building the GUI also requires the **Windows App SDK** tooling — in Visual Studio, the "Windows application development" workload; in VS Code, the C# Dev Kit extension plus the .NET SDK is enough (no live XAML designer/hot-reload, but builds and runs fine).

**GUI — win-x64:**

```bash
dotnet publish QuickA-Cleanup-GUI\QuickA-Cleanup-GUI.csproj -c Release -r win-x64 --self-contained true -p:WindowsPackageType=None -p:DebugType=None -p:DebugSymbols=false
```

Output is a self-contained folder (not a single `.exe` — WinUI 3 doesn't support `PublishSingleFile`). Zip the output folder for distribution, or run `QuickA-Cleanup-GUI.exe` directly from inside it.

**CLI — win-x64:**

```bash
dotnet publish QuickA-Cleanup-CLI\QuickA-Cleanup-CLI.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=true -p:DebugType=None -p:DebugSymbols=false
```

Replace `-r win-x64` with `-r win-x86` or `-r win-arm64` for other targets.

> ⚠️ Do not change the GUI's `net9.0-windows10.0.19041.0` target framework or the CLI's `net9.0-windows` — both are required for their respective UI frameworks.

</details>

<details>
<summary><b>🚀 CLI Flags</b></summary>
<br>

```bash
QuickA-Cleanup-CLI.exe [options]
```

| Flag            | Description                                       |
| --------------- | ------------------------------------------------- |
| `--dry-run`     | Preview all changes — nothing written to registry |
| `--no-backup`   | Skip the automatic `.reg` backup                  |
| `--help` / `-h` | Show usage and exit                               |

</details>

---

## 🐛 Issues & Support

Found a bug? Have a suggestion?

- 🐛 [Report an Issue](https://github.com/Ash1421/QuickA-Cleanup/issues)
- 💬 [Join Discord](https://rb.ash1421.com/discord)

---

## 📄 License

This project is licensed under the **GNU General Public License v3.0**.

- ✅ Commercial use, distribution, and modification allowed
- ⚠️ Must disclose source and changes
- ⚠️ Derivative works must be open-source under the same license

Read the full license [here](/LICENSE).

---

## ⚠️ Disclaimer

**By using, editing, or publishing this tool you acknowledge that you have read and understood the license terms and agree to be bound by them.**

**Modifying the Windows Registry carries inherent risk. Always use the built-in backup feature before making changes. Use at your own discretion.**

---

<div align="center">

**If you find this tool useful, please consider giving it a ⭐!**

Made with 💜 by [@Ash1421](https://github.com/Ash1421)

</div>
