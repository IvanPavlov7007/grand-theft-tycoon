param([string]$FfmpegPath = 'ffmpeg')
$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$recordingPath = Join-Path $repoRoot 'Recordings/GTT_showcase.mp4'
if (-not (Test-Path -LiteralPath $recordingPath)) {
    throw "Original recording not found: $recordingPath"
}
$clips = @(
    @{ Name = 'city-driving.gif'; Start = 1; Duration = 6; Width = 800 },
    @{ Name = 'steal-a-car.gif'; Start = 14; Duration = 8; Width = 560 },
    @{ Name = 'sell-a-car.gif'; Start = 24; Duration = 8; Width = 560 }
)
foreach ($clip in $clips) {
    $outputPath = Join-Path $PSScriptRoot $clip.Name
    $filter = "[0:v]fps=8,scale=$($clip.Width):-1:flags=lanczos,split[a][b];[a]palettegen=max_colors=128:stats_mode=diff[p];[b][p]paletteuse=dither=bayer:bayer_scale=5"
    & $FfmpegPath -hide_banner -loglevel error -ss $clip.Start -t $clip.Duration `
        -i $recordingPath -filter_complex $filter -loop 0 -y $outputPath
    if ($LASTEXITCODE -ne 0) { throw "FFmpeg failed: $($clip.Name), exit $LASTEXITCODE" }
    Get-Item -LiteralPath $outputPath | Select-Object Name, Length
}


