Add-Type -AssemblyName System.Drawing

$src = "F:\JUNAYED WEBSITE\vibe_images"
$out = "F:\JUNAYED WEBSITE\wwwroot\images\seed"
New-Item -ItemType Directory -Force -Path $out | Out-Null

$map = [ordered]@{
    'glowy-hero'                  = 'hero.jpg'
    'glowy-story'                 = 'story.jpg'
    'glowy-cat-necklaces'         = 'category-necklaces.jpg'
    'glowy-cat-earrings'          = 'category-earrings.jpg'
    'glowy-cat-rings'             = 'category-rings.jpg'
    'glowy-cat-bracelets'         = 'category-bracelets.jpg'
    'glowy-product-aurora'        = 'product-aurora-pendant.jpg'
    'glowy-product-rose-necklace' = 'product-rose-bloom-necklace.jpg'
    'glowy-product-petal-earrings'= 'product-blush-petal-earrings.jpg'
    'glowy-product-crystal-studs' = 'product-crystal-whisper-studs.jpg'
    'glowy-product-rose-ring'     = 'product-velvet-rose-ring.jpg'
    'glowy-product-halo-ring'     = 'product-golden-halo-ring.jpg'
    'glowy-product-charm-bracelet'= 'product-dainty-charm-bracelet.jpg'
    'glowy-product-pearl-bangle'  = 'product-pink-pearl-bangle.jpg'
}

$codec = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { $_.MimeType -eq 'image/jpeg' }
$encoderParams = New-Object System.Drawing.Imaging.EncoderParameters(1)
$encoderParams.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter([System.Drawing.Imaging.Encoder]::Quality, [long]86)

foreach ($key in $map.Keys) {
    $file = Get-ChildItem -Path $src -Filter "$key`_*.png" | Select-Object -First 1
    if (-not $file) { Write-Host "MISSING $key"; continue }

    $img = [System.Drawing.Image]::FromFile($file.FullName)
    $max = if ($key -eq 'glowy-hero') { 1400 } else { 1000 }
    $scale = [Math]::Min(1.0, $max / [Math]::Max($img.Width, $img.Height))
    $w = [int]($img.Width * $scale)
    $h = [int]($img.Height * $scale)

    $bmp = New-Object System.Drawing.Bitmap($w, $h)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.DrawImage($img, 0, 0, $w, $h)

    $dest = Join-Path $out $map[$key]
    $bmp.Save($dest, $codec, $encoderParams)

    $g.Dispose(); $bmp.Dispose(); $img.Dispose()
    Write-Host ("OK {0}  {1}x{2}" -f $map[$key], $w, $h)
}
