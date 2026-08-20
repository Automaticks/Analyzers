param([Parameter(Mandatory=$true)][string]$Name)
$agg=@{}
foreach ($r in (Get-ChildItem -Recurse -Filter 'coverage.cobertura.xml')) {
  [xml]$x = Get-Content $r.FullName
  foreach ($c in $x.coverage.packages.package.classes.class) {
    $f=$c.filename
    if ($f -notlike "*$Name*") { continue }
    if (-not $agg.ContainsKey($f)) { $agg[$f]=@{} }
    foreach ($l in $c.lines.line) {
      $n=[int]$l.number; $cur=$agg[$f][$n]
      if ($null -eq $cur -or [int]$l.hits -gt [int]$cur.h) { $agg[$f][$n]=@{h=[int]$l.hits;cc=$l.'condition-coverage'} }
    }
  }
}
foreach ($f in ($agg.Keys | Sort-Object)) {
  "=== $f"
  $src = Get-Content $f
  foreach ($n in ($agg[$f].Keys | Sort-Object)) {
    $e=$agg[$f][$n]
    $partial = $false
    if ($e.cc -and ($e.cc -match '\((\d+)/(\d+)\)')) { $partial = ($matches[1] -ne $matches[2]) }
    if ($e.h -eq 0 -or $partial) {
      $text = if ($n -le $src.Count) { $src[$n-1].Trim() } else { '' }
      "{0,5}  h={1,-4} {2,-9} {3}" -f $n,$e.h,$e.cc,$text
    }
  }
}
