$trx = "C:\Users\thomas\source\repos\idasen-desk-core\src\Idasen.BluetoothLE.Linak.Tests\TestResults\full_parallel.trx"
$proj = "C:\Users\thomas\source\repos\idasen-desk-core\src\Idasen.BluetoothLE.Linak.Tests\Idasen.BluetoothLE.Linak.Tests.csproj"

Write-Host "Parsing TRX: $trx"
[xml]$doc = Get-Content $trx
$ns = New-Object System.Xml.XmlNamespaceManager($doc.NameTable)
$ns.AddNamespace('t','http://microsoft.com/schemas/VisualStudio/TeamTest/2010')

$failed = $doc.SelectNodes("//t:UnitTestResult[@outcome='Failed']", $ns)
if (-not $failed -or $failed.Count -eq 0) {
	Write-Host "No failed tests found in TRX"
	exit 0
}

foreach ($r in $failed) {
	$testId = $r.getAttribute('testId')
	$testNode = $doc.SelectSingleNode("//t:UnitTest[@id='$testId']", $ns)
	if ($null -eq $testNode) {
		Write-Host "Could not find UnitTest node for id $testId"
		continue
	}

	$m = $testNode.SelectSingleNode('.//t:TestMethod', $ns)
	$class = $m.getAttribute('className')
	$name = $m.getAttribute('name')
	$fqn = "$class.$name"

	Write-Host "`n=== Re-running failed test: $fqn ==="

	# Create a safe TRX filename
	$safeName = ($class -replace '[^a-zA-Z0-9_.]','_') + '_' + ($name -replace '[^a-zA-Z0-9_.]','_') + '.trx'
	dotnet test $proj --no-build --filter "FullyQualifiedName=$fqn" --logger "trx;LogFileName=$safeName"
}

Write-Host "Done."