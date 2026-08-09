# 🌿 Canopy

![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/SynesthesiaDev/Canopy/build.yml?branch=main&style=for-the-badge&label=Build&color=33cc33)
![NuGet Version](https://img.shields.io/github/v/release/SynesthesiaDev/Canopy?style=for-the-badge&color=blue&label=Release)

![Target .NET](https://img.shields.io/badge/.NET-10.0-512bd4?style=for-the-badge&logo=dotnet)
![License](https://img.shields.io/badge/License-MIT-black?style=for-the-badge)

A lightweight wallpaper engine that automatically switches your desktop based on real-world conditions like time of day, weather, season, and holidays.

---

### ⚡ Features

- **☀️ Condition-Based Wallpapers** 
  - Automatically switch wallpapers based on time of day, current weather, season, and holidays
- **🔌 WebSocket Support:** 
  - Canopy can broadcast the current wallpaper along with info like it's accent color over a local WebSocket, so other apps (e.g. zebar) can theme themselves to match automatically
- **🖥️ System Theme Sync**
  - Optionally switch your system's light/dark theme alongside the wallpaper based on time of day
- 🚫 **No AI Slop**
    - Purely written by passionete single-brain-celled autistic individual

---

### 🚀 Getting Started

#### Prerequisites
- Windows 10 / 11
- [.NET 10.0 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)

---

### 🛠️ Configuration

Canopy is configured via a `config.synx` file, written in [Synx](https://github.com/SynesthesiaDev/Synx). A default config with a starter wallpaper set is generated automatically on first launch:

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

See the [full config schema](https://github.com/SynesthesiaDev/Canopy/blob/main/schema.md) for every available option.
