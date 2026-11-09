Add-Type -AssemblyName System.Drawing

$src = 'C:\Users\NIBIR\.qoder\tmp\F--JUNAYED-WEBSITE\images\db60a0ba-04dc-4a5c-93d5-399bd136c651\2df0bdaf-bc96-4092-8dfa-90cfe52c8bb7.jpg'
$dir = 'F:\JUNAYED WEBSITE\wwwroot\images\brand'
New-Item -ItemType Directory -Force -Path $dir | Out-Null

function Save-Resized($sourcePath, $destPath, $size, $quality) {
    $img = [System.Drawing.Image]::FromFile($sourcePath)
    $bmp = New-Object System.Drawing.Bitmap($size, $size)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.DrawImage($img, 0, 0, $size, $size)
    $g.Dispose()
    $img.Dispose()

    if ($destPath.EndsWith('.jpg')) {
        $codec = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { $_.MimeType -eq 'image/jpeg' }
        $params = New-Object System.Drawing.Imaging.EncoderParameters(1)
        $params.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter([System.Drawing.Imaging.Encoder]::Quality, [long]$quality)
        $bmp.Save($destPath, $codec, $params)
    } else {
        $bmp.Save($destPath, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    $bmp.Dispose()
}

Copy-Item $src (Join-Path $dir 'glowy-ore-logo-full.jpg') -Force
Save-Resized $src (Join-Path $dir 'glowy-ore-logo.jpg') 512 90
Save-Resized $src (Join-Path $dir 'apple-touch-icon.png') 180 100
Save-Resized $src (Join-Path $dir 'favicon.png') 64 100

Get-ChildItem $dir | Select-Object Name, Length | Format-Table -AutoSize
