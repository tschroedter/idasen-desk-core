$solution = "C:\Users\thomas\source\repos\idasen-desk-core\src\idasen-desk-core.sln"
Write-Host "Running dotnet test on solution: $solution"

dotnet test $solution --logger "trx;LogFileName=full_parallel.trx"

# Find all TRX files under src
$trxs = Get-ChildItem -Path "C:\Users\thomas\source\repos\idasen-desk-core\src" -Recurse -Filter '*.trx' -ErrorAction SilentlyContinue
if (-not $trxs -or $trxs.Count -eq 0) {
	Write-Host "No TRX files found under src"
	exit 0
}

foreach ($t in $trxs) {
	Write-Host "\nParsing $($t.FullName)"
	[xml]$doc = Get-Content $t.FullName
	$ns = New-Object System.Xml.XmlNamespaceManager($doc.NameTable)
	$ns.AddNamespace('t','http://microsoft.com/schemas/VisualStudio/TeamTest/2010')
	$failed = $doc.SelectNodes("//t:UnitTestResult[@outcome='Failed']", $ns)
	if (-not $failed -or $failed.Count -eq 0) {
		Write-Host "No failed tests in $($t.FullName)"
		continue
	}

	foreach ($r in $failed) {
		$testId = $r.getAttribute('testId')
		$testNode = $doc.SelectSingleNode("//t:UnitTest[@id='$testId']", $ns)
		if ($null -eq $testNode) {
			Write-Host "Could not find UnitTest node for id $testId in $($t.FullName)"
			continue
		}

		$m = $testNode.SelectSingleNode('.//t:TestMethod', $ns)
		$class = $m.getAttribute('className')
		$name = $m.getAttribute('name')
		$fqn = "$class.$name"

		Write-Host "Found failed: $fqn"

		# Re-run per-project to get full trace
		# Attempt to build a project path by removing the bin\... suffix from the TRX folder.
		# Use parentheses so the -replace operator gets exactly two operands and then concatenate.
		if ($null -ne $t.Directory) {
			$base = ($t.Directory.FullName -replace '\\bin\\.*$','')
			$projCandidate = Join-Path $base 'Idasen.BluetoothLE.Linak.Tests\Idasen.BluetoothLE.Linak.Tests.csproj'
			if (Test-Path $projCandidate) {
				$proj = $projCandidate
			}
			else {
				# fallback to known Linak test project path
				$proj = "C:\Users\thomas\source\repos\idasen-desk-core\src\Idasen.BluetoothLE.Linak.Tests\Idasen.BluetoothLE.Linak.Tests.csproj"
			}
		}
		else {
			# If Directory is null, fall back to the known path.
			$proj = "C:\Users\thomas\source\repos\idasen-desk-core\src\Idasen.BluetoothLE.Linak.Tests\Idasen.BluetoothLE.Linak.Tests.csproj"
		}

		$safeName = ($class -replace '[^a-zA-Z0-9_.]','_') + '_' + ($name -replace '[^a-zA-Z0-9_.]','_') + '.trx'
		Write-Host "Re-running $fqn against $proj"
		dotnet test $proj --no-build --filter "FullyQualifiedName=$fqn" --logger "trx;LogFileName=$safeName"
	}
}

Write-Host "Done."