# README media

[Documentation](../README.md) · [Game landing page](../../README.md)

These exports live outside the Git-ignored Recordings folder so the README works after a fresh clone. They use existing game footage and promotional artwork.

| Export | Original local source | Selection |
| --- | --- | --- |
| banner.png | Krita/Concept/Promo/Banner.png | Existing title artwork. |
| cover.png | Krita/Concept/Promo/Output/LogoItch.png | Promotional cover. |
| city-driving.gif | Recordings/GTT_showcase.mp4 | 00:01–00:07, 800 px wide. |
| steal-a-car.gif | Recordings/GTT_showcase.mp4 | 00:14–00:22, 560 px wide. |
| sell-a-car.gif | Recordings/GTT_showcase.mp4 | 00:24–00:32, 560 px wide. |
| city-overview.png | Recordings/Screenshot 2026-04-20 131553.png | Full screenshot. |
| stolen-vehicle.png | Recordings/Screenshot 2026-04-20 131529.png | Full screenshot. |

GIFs loop at 8 fps with a 128-color palette; they are kept to short clips to limit page weight. The original recording stays local.

## Regenerate

From the repository root with FFmpeg on PATH:

```powershell
./docs/media/export-gifs.ps1
```

Or specify the executable:

```powershell
./docs/media/export-gifs.ps1 -FfmpegPath 'C:\Program Files\Krita (x64)\bin\ffmpeg.exe'
```

The script replaces only the three exported GIFs. Review clip boundaries, animation and file sizes after refreshing footage. No media tools are needed to view the README.


