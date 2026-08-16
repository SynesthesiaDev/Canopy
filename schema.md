# Config Schema

Config is written in the [Synx](https://github.com/SynesthesiaDev/Synx) language which is very easy to understand and write even without any knowledge of it but **the TLDR is:**
- Types with `?` after them are nullable, you can specify `null` directly or just don't define them at all
- Enums are defined as string so with "string quotes"
 
Below are all schemas related to the config file.

## Config

- `_schemaVersion` - Automatic variable inserted by a codec, don't touch or stuff breaky!!!
- `_schema` - Link to this! Does nothing other than that
- `General` - General section
- `System` - System section
- `Updater` - Updater section
- `Weather` - Weather section
- `Websocket` - Websocket section
- `Wallpapers` - Wallpapers section

## General

- `RefreshPeriod`
  - Interval at which Canopy will check current conditions and potentially apply new wallpaper _(Keep in mind, the API limit for weather api is 10,000 requests a day!)_
  - Type: `Int`
  
- `FitMode`
  - How the wallpaper will be applied 
  - Type: `Enum` [`Fill`, `Stretch`, `Tile`,  `Center` or `Span`]

## System

- `UseLegacyWindowsApi`
    - Uses legacy windows api for compatibility
    - Type: `Boolean`

- `ApplyToAllMacOsSpaces`
    - Apply wallpapers to all MacOS spaces
    - Type: `Boolean`

- `DontUpdateWhenBatteryLow`
    - Don't run new checks when device battery is low 
    - Type: `Boolean`

- `ChangeSystemThemesDependingOnTime`
    - Change to dark theme when `Night` is selected and light mode when `Morning` is selected
    - Type: `Boolean`

## Updater

- `ReleaseStream`
    - What release stream the auto updater uses
    - Type: `Enum` [`Release`, `PreRelease`]

- `AutoUpdate`
    - Should new updates be automatically downloaded when Canopy launches
    - Type: `Boolean`

- `Source`
    - Source for the release stream (must be github releases)
    - Type: `String`

## Weather

- `UseAutoLocation`
    - Automatically detects your location from your IP Address _(Not sent anywhere.. what are we.. microslop?)_
    - Type: `Boolean`

- `OfflineFallback`
    - What should happen if you are offline and requests cannot be made
    - Type: `Enum` [`UseLastKnownState`, `IgnoreWeather`]

- `Coordinates`
    - Manual coordinates
    - Type: `Coordinates?`

## Coordinates

- `Latitude`
    - Your latitude
    - Type: `Double`

- `Longitude`
    - Your longitude
    - Type: `Double`

## Websocket

- `Enabled`
    - Should Canopy start a websocket server on startup
    - Type: `Boolean`

- `Url`
    - URL of the websocket. Must include `http://` at the beginning and `:port` at the end _(example: `http://localhost:5808/`)_
    - Type: `String`


### Websocket Message Schemas

Following are the schemas for messages Canopy sends over the websocket as JSON:

#### `/update` - NewWallpaperMessage

- `Timestamp`
    - Timestamp of wallpaper change
    - Type: `Long`

- `Wallpaper`
    - the new Wallpaper object
    - Type: `Wallpaper`

## Wallpaper

- `Path`
    - Path to the image relative to the `.canopy` folder in user folder
    - Type: `String`

- `Time`
    - List of Time enum, indicating at what time of day should the wallpaper appear
    - Type: `List of Time` [`Sunrise`, `Morning`, `Afternoon`, `Sunset`, `Night`, `DeepNight`]

- `Weather`
    - List of Weather enum, indicating at what weather should the wallpaper appear
    - Type: `List of Weather` [`Clear`, `Cloudy`, `Rainy`, `Stormy`]

- `Season`
    - List of Season enum, indicating during what season should the wallpaper appear
    - Type: `List of Season` [`Spring`, `Summer`, `Autumn`, `Winter`]

- `Holiday`
    - Indicating during what holiday this wallpaper should appear. **Note that `Holiday` overrides any other condition and is always picked**. Can be null or missing
    - Type: `Holiday?` [`Christmas`, `NewYear`, `Easter`, `Halloween`]

- `Accent`
    - Hex color for accent color, not used internally, but is sent in websocket messages so other programs may use it
    - Type: `String?`

**(Note that you may leave any of the lists empty or not define them to mark them as wildcard, meaning it will be allowed in any time/weather/season)**

# Default Config File

```hocon
_schemaVersion = 3
_schema = "https://github.com/SynesthesiaDev/Canopy/blob/main/schema.md"
General = {
    RefreshPeriod = 60000
    FitMode = "Fill"
    UseSolarNoonAsMidday = true
}
System = {
    UseLegacyWindowsApi = false
    ApplyToAllMacOsSpaces = true
    DontUpdateWhenBatteryLow = true
    ChangeSystemThemesDependingOnTime = false
}
Updater = {
    ReleaseStream = "Release"
    AutoUpdate = true
    Source = "https://github.com/SynesthesiaDev/Canopy"
}
Weather = {
    UseAutoLocation = true
    OfflineFallback = "UseLastKnownState"
    Coordinates = {
        Longitude = 14.421194
        Latitude = 50.087555
    }
}
Websocket = {
    Enabled = false
    Url = "http://localhost:5808/"
}
Wallpapers = [
    {
        Path = "./default/cloudy-quasar.png"
        Time = ["Night", "DeepNight"]
        Weather = ["Cloudy"]
        Season = []
        Holiday = null
        Accent = "#c5d9d7"
    },
    {
        Path = "./default/beach.jpg"
        Time = ["Afternoon"]
        Weather = ["Clear"]
        Season = ["Summer"]
        Holiday = null
        Accent = "#207ad9"
    },
    {
        Path = "./default/halloween.jpg"
        Time = []
        Weather = []
        Season = []
        Holiday = "Halloween"
        Accent = "#f56b3d"
    },
    {
        Path = "./default/eclipse.jpg"
        Time = ["Sunset"]
        Weather = ["Cloudy", "Clear"]
        Season = []
        Holiday = null
        Accent = "#f4545e"
    },
    {
        Path = "./default/flower-field.jpg"
        Time = ["Morning", "Afternoon"]
        Weather = ["Clear"]
        Season = ["Spring"]
        Holiday = null
        Accent = "#9ca15e"
    },
    {
        Path = "./default/i-touch-this.jpg"
        Time = ["Morning"]
        Weather = ["Clear"]
        Season = []
        Holiday = null
        Accent = "#89b238"
    },
    {
        Path = "./default/pink-clouds.jpg"
        Time = ["Sunset", "Sunrise"]
        Weather = ["Clear", "Cloudy"]
        Season = []
        Holiday = null
        Accent = "#e69c94"
    },
    {
        Path = "./default/snowflakes.jpg"
        Time = ["Night", "DeepNight"]
        Weather = ["Rainy", "Clear"]
        Season = ["Winter"]
        Holiday = null
        Accent = "#c2e6ff"
    },
    {
        Path = "./default/swirly-painting.jpg"
        Time = ["Sunset", "Sunrise"]
        Weather = ["Clear", "Cloudy"]
        Season = []
        Holiday = null
        Accent = "#df7488"
    },
    {
        Path = "./default/flowering-rain.png"
        Time = ["Morning", "Afternoon"]
        Weather = ["Rainy", "Stormy"]
        Season = []
        Holiday = null
        Accent = "#598fb1"
    },
    {
        Path = "./default/fallback/Sunrise.jpg"
        Time = ["Sunrise"]
        Weather = []
        Season = []
        Holiday = null
        Accent = "#1a4a4a"
    },
    {
        Path = "./default/fallback/Morning.jpg"
        Time = ["Morning"]
        Weather = []
        Season = []
        Holiday = null
        Accent = "#1b4a40"
    },
    {
        Path = "./default/fallback/Afternoon.jpg"
        Time = ["Afternoon"]
        Weather = []
        Season = []
        Holiday = null
        Accent = "#3d76a1"
    },
    {
        Path = "./default/fallback/Sunset.jpg"
        Time = ["Sunset"]
        Weather = []
        Season = []
        Holiday = null
        Accent = "#e56f32"
    },
    {
        Path = "./default/fallback/Night.jpg"
        Time = ["Night"]
        Weather = []
        Season = []
        Holiday = null
        Accent = "#314d3f"
    },
    {
        Path = "./default/fallback/DeepNight.jpg"
        Time = ["DeepNight"]
        Weather = []
        Season = []
        Holiday = null
        Accent = "#1b2836"
    }
]
```