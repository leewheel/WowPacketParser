$env:HTTP_PROXY="http://127.0.0.1:7897"; $env:HTTPS_PROXY="http://127.0.0.1:7897"; $env:http_proxy="http://127.0.0.1:7897"; $env:https_proxy="http://127.0.0.1:7897"
$ErrorActionPreference="Continue"
$token="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"
$h=@{"Authorization"="token $token";"Accept"="application/vnd.github+json"}
$root="D:\WoWSourcedCode\Tools\WowPacketParser"
$detailDir="$root\TempFiles\forks_detail"
New-Item -ItemType Directory -Force -Path $detailDir | Out-Null

$cmp = Get-Content "$root\TempFiles\compare_results.json" -Encoding UTF8 | ConvertFrom-Json
$ahead = $cmp | Where-Object { $_.ahead_by -gt 0 }

function Sanitize($n){ return $n -replace '/','__' }

$total=$ahead.Count; $i=0
foreach($fk in $ahead){
    $i++
    $owner=$fk.full_name.Split('/')[0]
    $branch=$fk.default_branch
    $safe=Sanitize $fk.full_name
    # branches
    $branches=@()
    try {
        $bpage=Invoke-RestMethod -Uri "https://api.github.com/repos/$($fk.full_name)/branches?per_page=100" -Headers $h -TimeoutSec 40
        $branches = $bpage | ForEach-Object { $_.name }
    } catch { $branches=@("__ERR__") }
    # ahead commits via compare pagination
    $commits=@()
    $page=1; $cap=500
    while($true){
        try {
            $r=Invoke-RestMethod -Uri "https://api.github.com/repos/TrinityCore/WowPacketParser/compare/master...${owner}:${branch}?per_page=100&page=$page" -Headers $h -TimeoutSec 40
        } catch { break }
        foreach($c in $r.commits){
            if($commits.Count -ge $cap){ break }
            $msg=$c.commit.message
            if($msg.Contains("`n")){ $msg=$msg.Split("`n")[0] }
            $commits += [PSCustomObject]@{ sha=$c.sha.Substring(0,[Math]::Min(10,$c.sha.Length)); date=$c.commit.author.date; msg=$msg.Trim() }
        }
        if($r.commits.Count -lt 100 -or $commits.Count -ge $cap){ break }
        $page++
    }
    $obj=[PSCustomObject]@{
        full_name=$fk.full_name
        ahead_by=$fk.ahead_by
        behind_by=$fk.behind_by
        pushed_at=$fk.pushed_at
        description=$fk.description
        default_branch=$branch
        branches=$branches
        commit_count=$commits.Count
        commits=$commits
    }
    $obj | ConvertTo-Json -Depth 4 | Set-Content "$detailDir\$safe.json" -Encoding UTF8
    if($i % 10 -eq 0){ Write-Host "detail $i/$total : $($fk.full_name) (commits=$($commits.Count), branches=$($branches.Count))" }
}
Write-Host "DONE. detail files written: $i"