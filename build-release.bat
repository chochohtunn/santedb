@echo off

set version=%1


if exist "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe" (
        echo will use VS 2017 build tools
        set msbuild="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
) else (
	if exist "c:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" (
        echo will use VS 2017 build tools
        set msbuild="c:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"
	) else ( 
		echo Unable to locate VS 2017 build tools, will use default build tools 
		goto :eof
	)
)

if exist "c:\Program Files (x86)\Inno Setup 5\ISCC.exe" (
	set inno="c:\Program Files (x86)\Inno Setup 5\ISCC.exe"
) else (
	echo Can't Find INNO Setup Tools
	goto :eof
)

set cwd=%cd%
set nuget="%cwd%\.nuget\nuget.exe"
echo Will build version %version%
echo Will use NUGET in %nuget%
echo Will use MSBUILD in %msbuild%

if exist %nuget% (
	%msbuild% santedb-server-ext.sln /t:restore
	%msbuild% santedb-server-ext.sln /t:clean /t:rebuild /p:configuration=Release /m:1

	FOR /R %cwd% %%G IN (*.nuspec) DO (
		echo Packing %%~pG
		pushd %%~pG
		rem %nuget% pack -OutputDirectory %localappdata%\NugetStaging -prop Configuration=Release
		popd
	)

	FOR /R %cwd%\bin\Release %%G IN (*.exe) DO (
		echo Signing %%G
		"C:\Program Files (x86)\Windows Kits\8.1\bin\x86\signtool.exe" sign "%%G"
	)
	FOR /R %cwd%\bin\Release %%G IN (*.dll) DO (
		echo Signing %%G
		"C:\Program Files (x86)\Windows Kits\8.1\bin\x86\signtool.exe" sign "%%G"
	)
	
	%inno% "/o.\bin\dist" ".\installer\SanteDBInstall.iss" /d"MyAppVersion=%version%" /d"x64" /d"BUNDLED"
	%inno% "/o.\bin\dist" ".\installer\SanteDBInstall.iss" /d"MyAppVersion=%version%" /d"BUNDLED"
	%inno% "/o.\bin\dist" ".\installer\SanteDBInstall.iss" /d"MyAppVersion=%version%" /d"x64" 


) else (	
	echo Cannot find NUGET 
)

:eof