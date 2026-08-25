$root="D:\WoWSourcedCode\Tools\WowPacketParser"
$detailDir="$root\TempFiles\forks_detail"
$files=Get-ChildItem "$detailDir\*.json"
function Tag($blob){
  $t=$blob.ToLower()
  $tags=@()
  if($t -match "3\.4\.4" -or $t -match "3\.4\.3" -or $t -match "3\.4\.2" -or $t -match "3\.4\.1" -or $t -match "wotlk_classic" -or $t -match "wotlkclassic" -or $t -match "v3_4"){ $tags += "WotLK-Classic(3.4)" }
  if($t -match "1\.15\.7" -or $t -match "1\.14" -or $t -match "1\.13" -or $t -match "1\.12" -or $t -match "sod" -or $t -match "season of discovery" -or $t -match "classic era" -or $t -match "vanilla"){ $tags += "Classic-Era/SoD(1.x)" }
  if($t -match "4\.4" -or $t -match "cataclysm"){ $tags += "Cata-Classic(4.4)" }
  if($t -match "5\.5\.3" -or $t -match "5\.5" -or $t -match "5\.4\.8" -or $t -match "5\.4\.7" -or $t -match "5\.4\.2" -or $t -match "5\.4\.1" -or $t -match "5\.3" -or $t -match "mop" -or $t -match "pandaria" -or $t -match "v3_8_0" -or $t -match "3\.80\.2" -or $t -match "3\.8\.0"){ $tags += "MoP-Classic(5.4/5.5/3.8-CN)" }
  if($t -match "3\.3\.5" -or $t -match "\b335\b" -or $t -match "wotlk release"){ $tags += "WotLK(3.3.5)" }
  if($t -match "2\.4\.3" -or $t -match "\btbc\b" -or $t -match "burning crusade"){ $tags += "TBC(2.4.3)" }
  if($t -match "skyfire" -or $t -match "5\.4\.8"){ $tags += "SkyFire(5.4.8)" }
  if($t -match "7\.3\.5" -or $t -match "legion"){ $tags += "Legion(7.3.5)" }
  if($t -match "8\.0\.1" -or $t -match "bfa" -or $t -match "battle for azeroth" -or $t -match "warband" -or $t -match "perksprogram" -or $t -match "11\.1" -or $t -match "11\.0" -or $t -match "10\."){ $tags += "Retail(8/10/11)" }
  if($t -match "2\.5\." -or $t -match "bcc" -or $t -match "burningcrusadeclassic"){ $tags += "BCC(2.5)" }
  if($t -match "7\.0\.3" -or $t -match "7\.1\.0" -or $t -match "7\.2\.0" -or $t -match "6\.1" -or $t -match "6\.2" -or $t -match "5\.1" -or $t -match "4\.3" -or $t -match "4\.2"){ $tags += "Old-Retail(4-7)" }
  if($tags.Count -eq 0){ $tags += "Other/Uncat" }
  return ($tags -join "; ")
}
$rows=@()
foreach($f in $files){
  $d=Get-Content $f.FullName -Encoding UTF8 | ConvertFrom-Json
  $blob = ($d.branches -join " ") + " " + ($d.commits | ForEach-Object { $_.msg }) -join " "
  $r=0
  foreach($c in $d.commits){ $m=$c.msg; if($m -match "Merge branch" -or $m -match "Merge remote" -or $m -match "Merge pull" -or $m -match "^Merge " -or $m -match "chore: activity sync" -or $m -match "^\.$" -or $m -match "^init$" -or $m -match "^yea$" -or $m -match "^add$"){ continue } $r++ }
  $rows += "| $($d.full_name) | $($d.ahead_by) | $($d.behind_by) | $($d.pushed_at.Substring(0,10)) | $r | $(Tag $blob) |"
}
$header="| Fork | Ahead | Behind | LastPush | RealFeat | VersionTags |`n|------|-------|--------|----------|----------|------------|"
Set-Content "$root\TempFiles\appendix_table.md" -Value ($header + ($rows -join "`n")) -Encoding UTF8
"rows=$($rows.Count) written"