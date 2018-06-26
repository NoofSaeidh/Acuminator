
param($solutionDir, $toolsPath, $build_dir)
    cd $build_dir 
    $filename = "Acuminator.Analyzers.dll" 
    Remove-Item "$filename-partial.dll" -ErrorAction SilentlyContinue
    Rename-Item "$build_dir\$filename" "$build_dir\$filename-partial.dll"
	write-host "solutionDir- $solutionDir toolsPath-$toolsPath build_dir-$build_dir"
    write-host "Executing ILMerge" 
    $p = Start-Process "$toolsPath/ILMerge.exe" -argumentlist @( "$build_dir\$filename-partial.dll", "$build_dir\Acuminator.Utils.dll",
   "/lib:$solutionDir\packages\Microsoft.Composition.1.0.27\lib\portable-net45+win8+wp8+wpa81", 
   "/lib:$solutionDir\packages\Microsoft.CodeAnalysis.Workspaces.Common.1.0.1\lib\net452", 
   "/lib:$solutionDir\packages\Microsoft.CodeAnalysis.CSharp.1.0.1\lib\net45",
   "/lib:$solutionDir\packages\Microsoft.CodeAnalysis.Common.1.0.1\lib\net45",
   "/lib:C:\Windows\Microsoft.NET\Framework64\v4.0.30319", 
   "/lib:""C:\Program Files (x86)\Microsoft Visual Studio\Installer\resources\app\ServiceHub\Services\Microsoft.Developer.IdentityService""",
   "/out:$filename", 
   "/internalize", 
   "/target:library", 
   "/targetplatform:v4") -PassThru -Wait
	if ($p.ExitCode -ne 0) {

        throw "Error: Failed to merge assemblies"

    }
	write-host "Successful merge" 
    Remove-Item $filename-partial.dll -ErrorAction SilentlyContinue `
