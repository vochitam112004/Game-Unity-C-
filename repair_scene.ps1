$filePath = "Assets/Scenes/01_GocDa/Chuong1_GocDa.unity"
$content = Get-Content $filePath
$newContent = New-Object System.Collections.Generic.List[string]
$headBuffer = New-Object System.Collections.Generic.List[string]
$theirsBuffer = New-Object System.Collections.Generic.List[string]
$inConflict = $false
$inTheirs = $false

foreach ($line in $content) {
    if ($line -match "^<<<<<<<") {
        $inConflict = $true
        $inTheirs = $false
        $headBuffer.Clear()
        $theirsBuffer.Clear()
        continue
    }
    elseif ($line -match "^=======") {
        $inTheirs = $true
        continue
    }
    elseif ($line -match "^>>>>>>>") {
        $inConflict = $false
        
        # LOGIC: Check if it's an object definition or just property changes
        $isObjectDefHead = ($headBuffer | Select-String "--- !u!") -ne $null
        $isObjectDefTheirs = ($theirsBuffer | Select-String "--- !u!") -ne $null
        
        if ($isObjectDefHead -or $isObjectDefTheirs) {
            # It's an addition of an object, keep both
            foreach ($h in $headBuffer) { $newContent.Add($h) }
            foreach ($t in $theirsBuffer) { $newContent.Add($t) }
        } else {
            # It's a property change, favor HEAD (or whichever has content)
            if ($headBuffer.Count -gt 0) {
                foreach ($h in $headBuffer) { $newContent.Add($h) }
            } else {
                foreach ($t in $theirsBuffer) { $newContent.Add($t) }
            }
        }
        continue
    }

    if ($inConflict) {
        if ($inTheirs) { $theirsBuffer.Add($line) }
        else { $headBuffer.Add($line) }
    } else {
        $newContent.Add($line)
    }
}

$newContent | Set-Content "Assets/Scenes/01_GocDa/Chuong1_GocDa_Fixed.unity" -Encoding UTF8
Write-Host "Fixed scene saved to Assets/Scenes/01_GocDa/Chuong1_GocDa_Fixed.unity"
