# AI / Vibe Coding Notes

## 1. Which AI Tool(s) I Used

**Claude (Anthropic)** via the Cowork desktop app — used for generating boilerplate, reviewing implementation logic, drafting documentation, and catching edge cases I described in natural language.

---

## 2. Two or Three Prompts That Helped the Most

**Prompt 1 — Date parsing strategy:**
> "I need to parse date strings in at least three different formats (MM/dd/yyyy, 'June 2, 2022', 'Jul-13-2020') in C# and also catch logically invalid dates like April 31 without crashing. What is the cleanest approach?"

This led directly to the `DateTime.TryParseExact` multi-format loop in `DateParserService`. The key insight the AI surfaced was that `TryParseExact` with `DateTimeStyles.None` rejects logically impossible dates — it does *not* silently roll them forward — which meant I didn't need a separate calendar-validation step.

**Prompt 2 — Open-Meteo response deserialization:**
> "The Open-Meteo API returns an array of values indexed by position, not a list of objects. How should I model this in C# so that I can look up data for a specific date?"

The AI suggested modeling `DailyData` with parallel arrays (`Time[]`, `Temperature2mMax[]`, etc.) and using `List<T>.IndexOf` to locate the correct position. This was cleaner than the dictionary approach I was initially considering.

**Prompt 3 — React detail panel UX:**
> "I want clicking a table row to open a side panel with full details. The panel should close when pressing Escape or clicking outside it. Show me a minimal React pattern for this."

The AI produced the `DetailPanel` component with the overlay/dialog pattern, `useEffect` for the Escape listener, and the `e.target === e.currentTarget` check for outside-click detection — all in one pass.

---

## 3. One Concrete Example Where the AI Was Wrong (and How I Corrected It)

**What the AI initially suggested:**

When asked how to resolve the path for `dates.txt` and `weather-data/`, the AI's first response used `Directory.GetCurrentDirectory()`:

```csharp
var datesFilePath = Path.Combine(Directory.GetCurrentDirectory(), _settings.DatesFile);
```

**Why it was wrong:**

`Directory.GetCurrentDirectory()` returns whichever directory the process was launched from — which varies depending on how the developer runs the app (`dotnet run` from the project folder, running the published binary, running from the solution root, etc.). This would cause `FileNotFoundException` in several common launch scenarios.

**How I detected it:**

I ran the app using `dotnet run` from the solution root (one level above `WeatherApp.Api/`) and it immediately failed with a "file not found" error because `GetCurrentDirectory()` pointed to the solution root, not the binary output directory.

**The correction I applied:**

Replaced it with `AppContext.BaseDirectory`, which always points to the directory containing the compiled assembly, regardless of where the process was started:

```csharp
private static string ResolveRelativePath(string relativePath) =>
    Path.IsPathRooted(relativePath)
        ? relativePath
        : Path.Combine(AppContext.BaseDirectory, relativePath);
```

This is the correct approach for file resources that are copied alongside the binary (as `dates.txt` is via the `.csproj` `<Content>` item).

---

## 4. Parts I Chose to Write Myself Rather Than Rely on AI

**`DateParserService.cs` — the format list order:**

I deliberately hand-tuned the order of formats in `SupportedFormats`. The AI generated the format strings correctly, but it placed `"MMMM dd, yyyy"` before `"MMMM d, yyyy"`. That ordering matters because `ParseExact` is strict: `"June 2, 2022"` (single-digit day) would fail to match `"MMMM dd, yyyy"` and fall through correctly anyway, but the reverse could cause subtle mis-parses with two-digit days. I rearranged them to single-digit-first to be explicit and added a comment explaining the reasoning.

**Error handling flow in `WeatherService.cs`:**

I wrote the "cache corrupt → fall through to re-fetch" logic myself. The AI's initial scaffold either threw on a bad cache file or silently returned null without re-fetching. I added the explicit log warning and re-fetch path because in a real system a corrupt cache should be automatically healed, not silently surfaced as an error to the user.

**The `WeatherSettings` typed config class:**

I chose to write the strongly-typed options class and the `IOptions<WeatherSettings>` injection pattern manually because the AI suggested putting raw `IConfiguration` as a constructor parameter in `WeatherService`. Injecting raw `IConfiguration` into a service is an anti-pattern (tight coupling, harder to unit-test); typed options with `Configure<T>` is the correct ASP.NET Core idiom and I wanted that to be explicit.
