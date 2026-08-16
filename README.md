<div align="center">

<h1>🌿 Canopy</h1>

A lightweight wallpaper engine that automatically synchronizes your wallpaper based on real-world conditions like time
of day,
weather, season, and holidays.

<p>
    <img src="https://img.shields.io/github/actions/workflow/status/SynesthesiaDev/Canopy/build.yml?branch=main&style=for-the-badge&label=Build&color=33cc33" alt="Build Status">
    <img src="https://img.shields.io/github/v/release/SynesthesiaDev/Canopy?style=for-the-badge&color=blue&label=Release" alt="NuGet Version">
    <img src="https://img.shields.io/badge/.NET-10.0-512bd4?style=for-the-badge&logo=dotnet" alt=".NET 10.0">
    <img src="https://img.shields.io/badge/License-MIT-black?style=for-the-badge" alt="License">
</p>

<br>

<img width="75%" alt="preview" src="https://github.com/user-attachments/assets/1152c15b-7cd4-4cab-87a1-f805d1de70d1" />

</div>
---

## ⚡ Features

- **☀️ Condition-Based Wallpapers**
    - Automatically switch wallpapers based on time of day, current weather, season, and holidays
- **🔌 WebSocket Support:**
    - Canopy can broadcast the current wallpaper along with info like it's accent color over a local WebSocket, so other
      apps (e.g. zebar) can theme themselves to match automatically
- **🖥️ System Theme Sync**
    - Optionally switch your system's light/dark theme alongside the wallpaper based on time of day
- 🚫 **No AI Slop**
    - Purely written by passionete single-brain-celled autistic individual

---

## 🚀 Getting Started

### Prerequisites

* **OS:** Windows 10 or 11
* **Runtime:** [.NET 10.0 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)

### Installation & Updates

- Download the latest
  installer: [Canopy-win-Setup.exe](https://github.com/SynesthesiaDev/Canopy/releases/latest/download/Canopy-win-Setup.exe)
- Run the installer and launch Canopy.
- **Auto-Updates:** Powered by [Velopack](https://github.com/velopack/velopack), Canopy checks for updates on startup
  automatically *(can be disabled in config)*.

---

### 🛠️ Configuration

Canopy is configured via a `config.synx` file, written in [Synx](https://github.com/SynesthesiaDev/Synx). A default
config with a starter wallpaper set is generated automatically on first launch:

```synx
Wallpapers = [
    {
        Path = "./default/beach.jpg"
        Time = ["Afternoon"]
        Weather = ["Clear"]
        Season = ["Summer"]
        Holiday = null
        Accent = "#207ad9"
    }

```

📖 See the [full config schema](https://github.com/SynesthesiaDev/Canopy/blob/main/schema.md) for every available config
option and websocket message format

---

If you like Canopy, consider leaving a ⭐ on the repository!