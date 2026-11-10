# Play on your own PC

The full guide behind the README's three steps.

1. Download **`RO3Emu-win-x64.zip`** from the [latest release](https://github.com/aethai/ro3emu-server/releases/latest)
   and extract it anywhere.
2. Double-click **`RO3Emu.exe`**. Nothing to install: no .NET, Java or Node.js. If Windows says "Windows protected
   your PC" (RO3Emu is not signed), click *More info*, then *Run anyway*.
3. The first time, it finds your Ragnarok 3: Genesis game folder (or asks for the one with `ro3.exe`), prepares the
   game data it needs (about a minute), starts the server and the game. Next time it only starts the server and the
   game.
4. Log in with any account name and password: the account is created on first login. Keep the RO3Emu window open
   while you play; press **Q** there (or close it) to stop the server.

**Don't have the game?** RO3Emu.exe downloads it from Genesis' update servers (about 24 GB) and checks every
file against Genesis' signature: it offers this when it finds no game folder, or run `RO3Emu.exe install`.
If it cannot reach the servers, your internet provider may block Genesis' mirror: connect through Cloudflare WARP
(or another VPN) and try again, or install the game with the Genesis launcher and choose "I have the game" (issue #104).
RO3Emu never hosts game files. Tested with the Genesis client releases of 7, 16, 21, 23, 25, 29 and 30 October 2026 (2026.10.30.1,
the current one); the 20 October release passes `RO3Emu.exe check` and the data export.

| Command | What it does |
|---|---|
| `RO3Emu.exe` | play on your own server (same as `RO3Emu.exe start`) |
| `RO3Emu.exe join <address>` | play on someone else's server (`host`, `host:port` or `http://host:port`) |
| `RO3Emu.exe host` | your server, open to friends (`--gm <account>` for GM commands, once per account: `--gm you --gm friend`) |
| `RO3Emu.exe panel [address]` | opens the [web panel](panel.md) of your server (or of the server at `address`) in the browser and prints its address |
| `RO3Emu.exe install [folder]` / `update [folder]` | the game from Genesis' update servers, every file checked |
| `RO3Emu.exe restore` | undo the patch: the game talks to Genesis' servers again |
| `RO3Emu.exe check` | does this game version fit this RO3Emu? (changes nothing) |
| `RO3Emu.exe diag` | makes `RO3Emu-diagnostics.zip` for a bug report |

Your server also serves a [web panel](panel.md) on `http://127.0.0.1:5381` (your account, characters, rankings, and
an admin area for GM accounts). `--panel-port <n>` moves it, `--no-panel` turns it off; two servers on one PC need
different ports for it too (`--http-port`, `--micro-port`, `--game-port`, `--panel-port`).

**What it changes.** In the game folder: a few DLLs in `ro3_Data\Managed` (the originals are kept in `ro3emu-backup`)
and `ro3emu\loc_en.tsv` (English texts, built from your own game files). Next to RO3Emu.exe: `data\` (game data and
your characters), `logs\` and `RO3Emu.settings.json`.

> [!WARNING]
> Don't let the Genesis launcher repair a patched game folder: it puts the original DLLs back. Run RO3Emu.exe
> again and it patches the game again.

## Playing on macOS

The Ragnarok 3 game client (`ro3.exe`) is a 64-bit Windows DirectX 11 / Unity (URP) executable, while the server and tools run natively on macOS. To run the client on a Mac using CrossOver or Wine:

### 1. Compatibility Layer Setup (CrossOver / Wine)
- **Bottle Type:** Windows 11 64-bit (`win11_64`) or Windows 10 64-bit.
- **Synchronization Engine (MSync):** Enable **MSync** (`WINEMSYNC=1`). Unity 2022.3 uses 11 multithreaded PhysX and job workers. Without MSync, traditional wineserver IPC locks cause heavy stuttering and frame drops. With MSync enabled, it runs smoothly at 100+ FPS on Apple Silicon.
- **Graphics Backend (`wined3d`):** Use Wine's built-in Direct3D backend (`CX_GRAPHICS_BACKEND=wined3d`). 
  > [!NOTE]
  > Apple's GPTK (`D3DMetal`) and `DXMT` translate Direct3D to Metal, but currently fail to evaluate Unity URP terrain and character shaders (due to `min16float` / half-precision register translation limitations), causing the 3D world to render solid black while the 2D UI draws. `wined3d` properly renders all 3D geometry and terrain without shader glitches.

### 2. Client Engine Configuration (`boot.config`)
In the game directory (`ro3_Data/boot.config`), disable multi-threaded graphics jobs:
```ini
gfx-enable-gfx-jobs=0
gfx-enable-native-gfx-jobs=0
```
*Why this is required:* The D3D11 translation layer on macOS does not support device multithread protection (`error: 0x80004002`). When Unity worker threads attempt concurrent command buffer recording with graphics jobs enabled, the 3D render pipeline halts and renders a blank black screen. Disabling graphics jobs (also enforced via the `-nographicsjobs` launch flag) ensures stable synchronous 3D rendering.

### 3. Launch Proof Requirement & `Ro3Launcher`
The client's anti-tamper check expects an inherited anonymous Win32 pipe handle delivering `RO3_LAUNCH_PROOF_V1` within 1 second of process launch, or it terminates with exit code 70.
- Build or use the native launcher tool: `tools/Ro3Launcher` (`Ro3Launcher.exe`).
- When launching through Wine/CrossOver, run `Ro3Launcher.exe`, which creates the anonymous pipe, starts `ro3.exe`, and pipes the launch proof before proxying command-line parameters.

### 4. Retina Display & Resolution Capping
Apple Silicon Liquid Retina displays have high native resolutions (e.g., 3024×1964), which can overwhelm translation layers if unscaled:
- **High Resolution Mode:** Keep disabled in CrossOver bottle settings so the display buffer uses 1x logical coordinates (`1512×982`).
- **Resolution Cap Flag:** Launch with `-screen-width 1920 -screen-height 1080` (or 16:9 / 16:10 standard resolutions) to cap the internal backbuffer.

### 5. In-Game FPS & Performance Monitoring
- **Built-in HUD:** By default, Ragnarok 3 displays real-time performance statistics in the top-left corner above the character portrait: `[Quality Preset] [Local Time] [Ping ms] [FPS]` (e.g., `Medium 22:39 0ms 104FPS`).
- **Target Frame Rate Cap:** Press `Esc` (or bottom-right Menu) -> **Settings** -> **Picture Quality / 画面** -> **Frame Rate Cap / 帧率上限** to toggle between 30 FPS, 60 FPS, or High / Unlimited.
- **macOS Metal Performance HUD:** To view Apple's native GPU metrics overlay (frame times, frame graphs, and draw calls), run the launch script with `MTL_HUD_ENABLED=1`:
  ```bash
  MTL_HUD_ENABLED=1 ./launch-ro3.sh
  ```

### 6. Launch Scripts (`launch-ro3.sh` / `launch-ro3.command`)
Use the helper scripts in the game folder to launch with all optimized environment variables:
```bash
#!/usr/bin/env bash
set -e

DIR="$(cd "$(dirname "$0")/ROO_PC" && pwd)"
CX_BIN="/Applications/CrossOver.app/Contents/SharedSupport/CrossOver/bin/wine"

export WINEMSYNC=1
export CX_GRAPHICS_BACKEND="${CX_GRAPHICS_BACKEND:-wined3d}"
export WINEDEBUG=-all
export GST_DEBUG=0
export MVK_CONFIG_LOG_LEVEL=0
export MVK_CONFIG_SYNCHRONOUS_QUEUE_SUBMITS=0

RES_W="${RES_W:-1920}"
RES_H="${RES_H:-1080}"

EXTRA_ARGS=()
if [[ ! "$*" =~ "-screen-width" ]]; then
    EXTRA_ARGS+=("-screen-width" "$RES_W" "-screen-height" "$RES_H")
fi
if [[ ! "$*" =~ "-nographicsjobs" ]]; then
    EXTRA_ARGS+=("-nographicsjobs")
fi

exec "$CX_BIN" --bottle RO3 --workdir "$DIR" "$DIR/Ro3Launcher.exe" "$@" "${EXTRA_ARGS[@]}"
```

### 7. Troubleshooting
- **Black/Blank 3D Screen (UI and minimap visible):** Ensure `gfx-enable-gfx-jobs=0` in `ro3_Data/boot.config` and use `CX_GRAPHICS_BACKEND=wined3d`.
- **`[mvk-error] Vertex attribute v6(6) of type uint4 cannot be read using MTLAttributeFormatFloat4`:** Harmless warning produced by MoltenVK when compiling minor shader permutations (e.g. bone indices). It does not affect gameplay and is silenced by setting `MVK_CONFIG_LOG_LEVEL=0`.
- **GStreamer Assertion (`gst_video_info_from_caps`):** Harmless video decoder debug assertion during intro/cutscene playback. Silenced by `GST_DEBUG=0` and `WINEDEBUG=-all`.
