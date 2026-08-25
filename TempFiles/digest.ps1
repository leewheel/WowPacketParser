$root="D:\WoWSourcedCode\Tools\WowPacketParser"
$detailDir="$root\TempFiles\forks_detail"
$out="$root\TempFiles\digests.txt"
$sw=New-Object System.IO.StreamWriter($out,[System.Text.Encoding]::UTF8)
$files=Get-ChildItem "$detailDir\*.json" | Sort-Object Name
foreach($f in $files){
  $d=Get-Content $f.FullName -Encoding UTF8 | ConvertFrom-Json
  $real=@()
  foreach($c in $d.commits){
    $m=$c.msg
    if($m -match "Merge branch" -or $m -match "Merge remote" -or $m -match "Merge pull" -or $m -match "^Merge " -or $m -match "chore: activity sync" -or $m -match "^\.$" -or $m -match "^init$" -or $m -match "^yea$" -or $m -match "^add$"){ continue }
    $real += "[$($c.date.Substring(0,10))] $m"
  }
  $sw.WriteLine("============================================================")
  $sw.WriteLine("FORK: $($d.full_name)  | ahead=$($d.ahead_by) behind=$($d.behind_by) pushed=$($d.pushed_at)")
  $sw.WriteLine("branches: $($d.branches)")
  $sw.WriteLine("REAL(non-merge) commits: $($real.Count) / total $($d.commit_count)")
  if($real.Count -gt 0){
    $lim=$real.Count
    if($lim -gt 70){ $lim=70; $sw.WriteLine("  (showing first 70 of $($real.Count))" ) }
    for($i=0;$i -lt $lim;$i++){ $sw.WriteLine("  - $($real[$i])") }
  }
  $sw.WriteLine("")
}
$sw.Close()
"wrote $out"