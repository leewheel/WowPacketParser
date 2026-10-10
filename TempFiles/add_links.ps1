$root="D:\WoWSourcedCode\Tools\WowPacketParser"
$reportPath="$root\WowPacketParser_Forks_Report.md"
# load the 68 ahead fork full_names
$idx=Get-Content "$root\TempFiles\category_index.json" -Encoding UTF8 | ConvertFrom-Json
$names=@()
foreach($e in $idx){ $names += $e.full_name }
# also include upstream so it's linked too
$names += "TrinityCore/WowPacketParser"

$text=Get-Content $reportPath -Encoding UTF8 -Raw
foreach($n in $names){
  $link="[$n](https://github.com/$n)"
  $text=$text.Replace($n, $link)
}
Set-Content $reportPath -Value $text -Encoding UTF8
"done. links injected for $($names.Count) names."