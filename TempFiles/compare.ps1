$env:HTTP_PROXY="http://127.0.0.1:7897"; $env:HTTPS_PROXY="http://127.0.0.1:7897"; $env:http_proxy="http://127.0.0.1:7897"; $env:https_proxy="http://127.0.0.1:7897"
$ErrorActionPreference="Continue"
$token="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"
$h=@{"Authorization"="token $token";"Accept"="application/vnd.github+json"}
$root="D:\WoWSourcedCode\Tools\WowPacketParser"
$forks = Get-Content "$root\TempFiles\forks_all.json" -Encoding UTF8 | ConvertFrom-Json
$results=@()
$i=0
foreach($fk in $forks){
    $owner=$fk.full_name.Split('/')[0]
    $branch=$fk.kernel
    $branch=$fk.default_branch
    try {
        $r=Invoke-RestMethod -Uri "https://api.github.com/repos/TrinityCore/WowPacketParser/compare/master...${owner}:${branch}?per_page=1" -Headers $h -TimeoutSec 40
        $results += [PSCustomObject]@{
            full_name=$fk.full_name; default_branch=$branch; pushed_at=$fk.pushed_at
            updated_at=$fk.updated_at; created_at=$fk.created_at; owner_type=$fk.owner.type
            stargazers=$fk.stargazers_count; description=$fk.description
            status=$r.status; ahead_by=$r.ahead_by; behind_by=$r.behind_by; total_commits=$r.total_commits
        }
    } catch {
        $results += [PSCustomObject]@{
            full_name=$fk.full_name; default_branch=$branch; pushed_at=$fk.pushed_at
            updated_at=$fk.updated_at; created_at=$fk.created_at; owner_type=$fk.owner.type
            stargazers=$fk.stargazers_count; description=$fk.description
            status="ERROR"; ahead_by=-1; behind_by=-1; total_commits=-1
        }
    }
    $i++
    if($i % 50 -eq 0){ Write-Host "processed $i / $($forks.Count)" }
}
$results | ConvertTo-Json -Depth 4 | Set-Content "$root\TempFiles\compare_results.json" -Encoding UTF8
$ahead = $results | Where-Object { $_.ahead_by -gt 0 }
Write-Host "==== SUMMARY ===="
Write-Host "Total compared: $($results.Count)"
Write-Host "Ahead (>0): $($ahead.Count)"
Write-Host "Errors: $(($results | Where-Object { $_.status -eq 'ERROR' }).Count)"
$ahead | Sort-Object ahead_by -Descending | Select-Object full_name,ahead_by,behind_by,pushed_at,description | Format-Table -AutoSize | Out-String -Width 250 | Write-Host