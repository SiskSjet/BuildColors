@echo off
setlocal EnableDelayedExpansion

set mod_dir=%~dp0Mod
set scripts_dir=%~dp0Scripts
set artifact_dir=%~dp0bin
for %%* in (.) do set mod_name=%%~nx*
set se_mods_dir=%appdata%\SpaceEngineers\Mods
set se_mod_namespace=Sisk
set se_mod_path=%se_mods_dir%\%mod_name%
set se_mod_scripts=%se_mod_path%\Data\Scripts\%se_mod_namespace%

if not exist "%se_mods_dir%" goto NO_SE_MODS_DIR
if not exist "%mod_dir%\" goto NO_MOD_DIR
if not exist "%scripts_dir%\" goto NO_SCRIPTS_DIR

::create exclude lists for robocopy
set file=exclude.txt
for /f "tokens=*" %%L in (%file%) do (
	set line=%%L

	if "!line:~0,1!"=="\" (
		if not "!dirs!"=="" (
        	Set dirs=!dirs! "!line:~1!"
    	) else (
        	Set dirs="!line:~1!"
    	)
	) else if "!line:~0,1!"=="." (
		if not "!files!"=="" (
        	Set files=!files! "*!line!"
    	) else (
        	Set files="*!line!"
    	)
	) else (
		if not "!files!"=="" (
        	Set files=!files! "!line!"
    	) else (
        	Set files="!line!"
    	)
	)
)

:: create exclude list for findstr
(for /f "tokens=*" %%L in ('FINDSTR "^\\.*" "exclude.txt"') do echo %%~nxL) > "%tmp%\exclude.txt"

:: create additional script exclude list for robocopy
for /F %%G in ('dir /ad /b ^| findstr /l /i /x /v /g:"%tmp%\exclude.txt"') do (
	if not %%~nxG == Mod (
		if not %%~nxG == Scripts (
			if not "!script_dirs!"=="" (
        		Set script_dirs=!script_dirs! "%%G"
    		) else (
        		Set script_dirs="%%G"
    		)
		)
	)
)

echo copy files from 'Mod'
robocopy "%mod_dir%" "%se_mod_path%" /MIR /Z /MT:8 /XJD /FFT /XD !dirs! "Scripts" /XF !files! /NC /NDL /NFL /NJH /NP /NS

echo copy files from 'Scripts'
robocopy "%scripts_dir%" "%se_mod_scripts%" /MIR /Z /MT:8 /XJD /FFT /XD !dirs! !script_dirs! /XF !files! /NC /NDL /NFL /NJH /NP /NS

for /F %%G in ('dir /ad /b ^| findstr /l /i /x /v /g:"%tmp%\exclude.txt"') do (
	if not %%~nxG == Mod (
		if not %%~nxG == Scripts (
			echo copy files from '%%G'
			robocopy "%~dp0%%G" "%se_mod_scripts%\%%G" /MIR /Z /MT:8 /XJD /FFT /XD !dirs! /XF !files! /NC /NDL /NFL /NJH /NP /NS
		)
	)
)

del "%tmp%\exclude.txt"

:: update zip file
for %%F in ("%artifact_dir%\*.zip") do (
	set artifact=%%F
	goto UPDATE_ARTIFACT
)
goto  EXIT


:UPDATE_ARTIFACT
echo update artifact
where 7z >nul 2>&1 && (
	7z u -up0q0r2x2y2z1w2 "%artifact%" "%se_mod_path%""
) || (
	goto EXIT
)

where gitversion >nul 2>&1 && (
	for /f %%R in ('gitversion /showvariable SemVer') do set semver=%%R
	rename "%artifact%" "%mod_name%.!semver!.zip"
) || (
	echo gitversion not installed using default name
	rename "%artifact%" "%mod_name%.zip"
)

goto EXIT

:NO_SE_MODS_DIR
echo Space Engineers Mods folder not found
goto EXIT

:NO_MOD_DIR
echo No 'Mod' directory found
goto EXIT

:NO_SCRIPTS_DIR
echo No 'Scripts' directory found
goto EXIT

:EXIT
