$proj = "C:\Users\thomas\source\repos\idasen-desk-core\src\Idasen.BluetoothLE.Linak.Tests\Idasen.BluetoothLE.Linak.Tests.csproj"
$dir = "C:\Users\thomas\source\repos\idasen-desk-core\src\Idasen.BluetoothLE.Linak.Tests\ConstructorNullTester"

Write-Host "Building project: $proj"
dotnet build $proj

$files = Get-ChildItem -Path $dir -Filter '*.cs' | Sort-Object Name | ForEach-Object { $_.BaseName }
$results = @()

foreach ($name in $files) {
	Write-Host "\n=== Running $name ==="
	$fq = "Idasen.BluetoothLE.Linak.Tests.ConstructorNullTester.$name.Constructor_ForAnyParameterNullThrows_AllPassing"
	dotnet test $proj --no-build --filter "FullyQualifiedName=$fq" --logger 'console;verbosity=minimal'

	if ($LASTEXITCODE -eq 0) {
		Write-Host "RESULT: $name - Passed"
		$results += "$name:Passed"
	}
	else {
		Write-Host "RESULT: $name - Failed"
		$results += "$name:Failed"
	}
}

Write-Host "\n=== Summary ==="
$results | ForEach-Object { Write-Host $_ }
