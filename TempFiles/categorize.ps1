$env:HTTP_PROXY="http://127.0.0.1:7897"; $env:HTTPS_PROXY="http://127.0.0.1:7897"; $env:http_proxy="http://127.0.0.1:7897"; $env:https_proxy="http://127.0.0.1:7897"
$ErrorActionPreference="Continue"
$root="D:\WoWSourcedCode\Tools\WowPacketParser"
$detailDir="$root\TempFiles\forks_detail"
$files=Get-ChildItem "$detailDir\*.json"

# category definitions: name -> regex (case-insensitive)
$cats=@{
  "3.4.x (WotLK经典)" = @("3\.4","wotlk classic","wlk classic")
  "1.x 经典旧世/赛季(SD)" = @("1\.1\.5","1\.12","1\.13","1\.14","1\.15","vanilla","classic era","classicera","season of discovery","\bsod\b")
  "4.4.x (大地的裂变经典)" = @("4\.4","cataclysm")
  "5.4/5.5.x (熊猫人之谜经典)" = @("5\.4","5\.5","\bmop\b","mists","pandaria")
  "3.3.5/3.3 (巫妖王/3.8猜测)" = @("3\.3\.5","3\.3\b","wotlk","\b335\b","3\.8")
  "2.4.3 (TBC)" = @("2\.4\.3","\btbc\b","burning crusade")
  "SkyFire/5.4.8" = @("skyfire","5\.4\.8")
  "零售新版本(10/11.x)" = @("10\.0","10\.1","10\.2","11\.0","11\.1","dragonflight","war within","shadowlands","bfa","legion","retail")
}
function MatchCats($text){
  $t=$text.ToLower()
  $out=@()
  foreach($k in $cats.Keys){
    foreach($pat in $cats[$k]){
      if($t -match $pat){ $out+=$k; break }
    }
  }
  return $out
}

$index=@()
foreach($f in $files){
  $d=Get-Content $f.FullName -Encoding UTF8 | ConvertFrom-Json
  $blob = ($d.branches -join " ") + " " + ($d.commits | ForEach-Object { $_.msg }) -join " "
  $matched=MatchCats $blob
  # version token frequency
  $verHits=@{}
  foreach($c in $d.commits){
    $m=$c.msg
    if($m -match "(\d+\.\d+(?:\.\d+)?)"){ 
      $v=$Matches[1]
      $verHits[$v]=(1+$(if($verHits[$v]){$verHits[$v]}else{0}))
    }
  }
  $recent = ([datetime]::Parse($d.pushed_at) -gt [datetime]"2024-01-01")
  $index += [PSCustomObject]@{
    full_name=$d.full_name
    ahead_by=$d.ahead_by
    behind_by=$d.behind_by
    pushed_at=$d.pushed_at
    recent=$recent
    categories=$matched
    top_versions=($verHits.GetEnumerator() | Sort-Object Value -Descending | Select-Object -First 8 | ForEach-Object { "$($_.Key):$($_.Value)" }) -join ", "
    commit_count=$d.commit_count
    branches=($d.branches -join ", ")
  }
}
$index | ConvertTo-Json -Depth 4 | Set-Content "$root\TempFiles\category_index.json" -Encoding UTF8

# print compact summary grouped by category
Write-Host "==== CATEGORY SUMMARY ===="
foreach($k in $cats.Keys){
  $members=$index | Where-Object { $_.categories -contains $k }
  if($members.Count -gt 0){
    Write-Host "`n### $k  ($(@($members).Count) forks)"
    $members | Sort-Object ahead_by -Descending | ForEach-Object {
      Write-Host ("  {0,-45} ahead={1,-4} recent={2,-5} topVer=[{3}]" -f $_.full_name,$_.ahead_by,$_.recent,$_.top_versions)
    }
  }
}
Write-Host "`n==== RECENT (2024+) AHEAD FORKS ===="
$index | Where-Object { $_.recent } | Sort-Object pushed_at -Descending | ForEach-Object {
  Write-Host ("  {0,-45} ahead={1,-4} behind={2,-5} cats=[{3}]" -f $_.full_name,$_.ahead_by,$_.behind_by,($_.categories -join ";"))
}
Write-Host "`n==== UNCATEGORIZED (no version token) ===="
$index | Where-Object { $_.categories.Count -eq 0 } | Sort-Object ahead_by -Descending | ForEach-Object {
  Write-Host ("  {0,-45} ahead={1,-4} recent={2}" -f $_.full_name,$_.ahead_by,$_.recent)
}