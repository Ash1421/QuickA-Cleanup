# 🧹 QuickA-Cleanup

> Smart Quick Access cleaner for Windows Explorer – cross-version, lightweight, and fast.

---

## Badges and Repositorie information

[![.NET](https://img.shields.io/badge/Built%20With-.NET%208/9-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20Only-blue?style=for-the-badge&logo=windows)](https://learn.microsoft.com/en-us/windows/)
[![Self-Contained](https://img.shields.io/badge/Deployment-Self--Contained-purple?style=for-the-badge)](#-deployment)
[![Version](https://img.shields.io/badge/version-v1.0.0-green?style=for-the-badge)](https://github.com/Ash1421/QuickA-Cleanup/releases/tag/v1.0.0)
---

## 📜 Liscense

[![License: GPL v3](https://img.shields.io/badge/License-GPL%20v3-gold.svg?style=for-the-badge)](https://www.gnu.org/licenses/gpl-3.0)

---

## 📋 Features

- 🧠 **Cleans broken, Bloated, Useless, or outdated Quick Access entries**
- ⚡ **Runs as a single-file, self-contained executable command line tool**
- 💻 **Built for Windows with .NET 8.0 and .NET 9.0**
- 🔐 **Safe and transparent operation (no external dependencies)**
- 🪟 **Supports Windows 7.0+**
- 🚀 **Fast startup, small footprint**

---

## 🛠️ Requirements

- ✅ Windows 7, 10, 11 (x64 or ARM64)
- ✅ [.NET SDK 8.0 or 9.0](https://dotnet.microsoft.com/en-us/download)
- ✅ No external dependencies

> For end-users, no .NET installation is needed thanks to self-contained publishing.

---

## 📦 Build Instructions

You can build the project using the .NET SDK. **Specify the framework explicitly** depending on which version you want to publish:

### ✅ .NET 8.0 Build

```bash
dotnet publish -c Release -f net8.0 -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true
```

Resulting file:

```
./bin/Release/net8.0/win-x64/publish/QuickA-Cleanup.exe
```

---

### ✅ .NET 9.0 Build

```bash
dotnet publish -c Release -f net9.0 -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true
```

Resulting file:

```
./bin/Release/net9.0/win-x64/publish/QuickA-Cleanup.exe
```

---

## 🚀 Usage

Just run the generated executable:

```bash
QuickA-Cleanup.exe
```

You can optionally run it from PowerShell or CMD in the directory that it is in . No installation required.

---

## 📄 License

This project is licensed under the **GNU General Public License v3.0**.

Key points:

- ✅ Commercial use, distribution, and modification allowed
- ⚠️ Must disclose source and changes
- ⚠️ Derivative works must be open-source (same license)

Read full license [here](https://www.gnu.org/licenses/gpl-3.0).

---

## 👤 Author

**Ash1421**

- GitHub: [@Ash1421](https://github.com/Ash1421)

---

## 📝 Notes

- You’re free to rename the executable after build (`QuickA-Cleanup.exe` ➝ `QuickAccessCleaner.exe`, etc.)
- For x86 or ARM64 builds, adjust the `-r` flag accordingly:
  - `-r win-x86`
  - `-r win-arm64`

---

<div align="center">

**If you find this tool useful, please consider giving it a ⭐!**  
Made with 💜 by [@Ash1421](https://github.com/Ash1421)

</div>
