param(
    [string]$Path = "src"
)
$violations = New-Object System.Collections.Generic.List[string]
$files = Get-ChildItem -Path $Path -Recurse -Filter *.cs |
    Where-Object { $_.FullName -notmatch '\\(bin|obj|Migrations)\\' }
foreach ($file in $files) {
    $relativePath = $file.FullName.Substring((Get-Location).Path.Length + 1)
    $lines = Get-Content -LiteralPath $file.FullName -Encoding UTF8
    for ($index = 0; $index -lt $lines.Count; $index++) {
        $line = $lines[$index]
        $lineNumber = $index + 1
        if ([string]::IsNullOrWhiteSpace($line)) {
            $nextLine = if ($index + 1 -lt $lines.Count) { $lines[$index + 1].TrimStart() } else { "" }
            if (-not $nextLine.StartsWith("return")) {
                $violations.Add("${relativePath}:${lineNumber} blank line is only allowed before a return statement")
            }
        }
        if ($line -match '^\s*//' -or $line -match '^\s*/\*' -or $line -match ';\s*//') {
            $violations.Add("${relativePath}:${lineNumber} comments are not allowed")
        }
        if ($line -match '[^\x00-\x7F]') {
            $violations.Add("${relativePath}:${lineNumber} non ASCII character found")
        }
    }
}
if ($violations.Count -gt 0) {
    $violations | ForEach-Object { Write-Host $_ -ForegroundColor Red }
    Write-Host "$($violations.Count) clean code violations found." -ForegroundColor Red
    exit 1
}
Write-Host "Clean code checks passed for $($files.Count) files." -ForegroundColor Green
