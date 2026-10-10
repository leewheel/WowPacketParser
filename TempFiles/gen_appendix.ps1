$root="D:\WoWSourcedCode\Tools\WowPacketParser"
$idx=Get-Content "$root\TempFiles\category_index.json" -Encoding UTF8 | ConvertFrom-Json
$detailDir="$root\TempFiles\forks_detail"
$rows=@()
foreach($e in $idx){
  # compute real (non-merge) commit count from detail file
  $safe=$e.full_name -replace '/','__'
  $real="-"
  try {
    $d=Get-Content "$detailDir\$safe.json" -Encoding UTF8 | ConvertFrom-Json
    $r=0
    foreach($c in $d.commits){ $m=$c.msg; if($m -match "Merge branch" -or $m -match "Merge remote" -or $m -match "Merge pull" -or $m -match "^Merge " -or $m -match "chore: activity sync" -or $m -match "^\.$" -or $m -match "^init$" -or $m -match "^yea$" -or $m -match "^add$"){ continue } $r++ }
    $real=$r
  } catch { $real="?" }
  $cats = if($e.categories.Count -eq 0){ "未归类/其他" } else { $e.categories -join "; " }
  $rows += "| $($e.full_name) | $($e.ahead_by) | $($e.behind_by) | $($e.pushed_at.Substring(0,10)) | $real | $cats |"
}
$header="| Fork | 超前提交 | 落后提交 | 最后推送 | 实质功能提交数 | 目标版本归类 |`n|------|--------|--------|----------|--------------|------------|"
$out=$header + ($rows -join "`n")
Set-Content "$root\TempFiles\appendix_table.md" -Value $out -Encoding UTF8
"rows=$($rows.Count) -> TempFiles/appendix_table.md"