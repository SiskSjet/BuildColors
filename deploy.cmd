@echo off
setlocal EnableDelayedExpansion

if not [%1]==[] (
	if [%1] == [dev] (set dev="")
	if [%1] == [beta] (set beta="")
)

set mod_dir=%~dp0Mod
set scripts_dir=%~dp0Scripts
set artifact_dir=%~dp0bin
for %%* in (.) do set mod_name=%%~nx*
if defined dev (	
	call :ToAcronym mod_name
	set mod_name=!mod_name!_DEV
)

if defined beta (
	set mod_name=%mod_name% (BETA^)
)

set se_mods_dir=%appdata%\SpaceEngineers\Mods
set se_mod_namespace=Sisk
set se_mod_path=%se_mods_dir%\%mod_name%
set se_mod_scripts=%se_mod_path%\Data\Scripts\%se_mod_namespace%

if not exist "%se_mods_dir%" goto NO_SE_MODS_DIR
if not exist "%mod_dir%\" goto NO_MOD_DIR
if not exist "%scripts_dir%\" goto NO_SCRIPTS_DIR

::create exclude lists for robocopy
set dirs=dev beta
set file=exclude.txt
for /f "tokens=*" %%L in (%file%) do (
	set line=%%L

	if "!line:~0,1!"=="\" (
		if not "!dirs!"=="" (
        	set dirs=!dirs! "!line:~1!"
    	) else (
        	set dirs="!line:~1!"
    	)
	) else if "!line:~0,1!"=="." (
		if not "!files!"=="" (
        	set files=!files! "*!line!"
    	) else (
        	set files="*!line!"
    	)
	) else (
		if not "!files!"=="" (
        	set files=!files! "!line!"
    	) else (
        	set files="!line!"
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
        		set script_dirs=!script_dirs! "%%G"
    		) else (
        		set script_dirs="%%G"
    		)
		)
	)
)

echo copy files from 'Mod'
robocopy "%mod_dir%" "%se_mod_path%" /MIR /Z /MT:8 /XJD /FFT /XD !dirs! "Scripts" /XF !files! /NC /NDL /NFL /NJH /NP /NS
if defined dev (
	if exist "%se_mod_path%\modinfo.sbmi" del "%se_mod_path%\modinfo.sbmi"
	xcopy "%mod_dir%\dev\*.*" /s /e /f /y "%se_mod_path%\"
)

if defined beta (
	if exist "%se_mod_path%\modinfo.sbmi" del "%se_mod_path%\modinfo.sbmi"
	xcopy "%mod_dir%\beta\*.*" /s /e /f /y "%se_mod_path%\"
)

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

:ToTitleCase <stringVar> (
	for %%i in (" a= A" " b= B" " c= C" " d= D" " e= E" " f= F" " g= G" " h= H" " i= I" " j= J" " k= K" " l= L" " m= M" " n= N" " o= O" " p= P" " q= Q" " r= R" " s= S" " t= T" " u= U" " v= V" " w= W" " x= X" " y= Y" " z= Z") do (
		call set "%1=%%%1:%%~i%%"
	)
)(
	exit /b
)

:ToPascalCase <stringVar> (
	call :ToTitleCase %1
	call set "%1=%%%1: =%%"
)(
	exit /b
)

:ToAcronym <stringVar> (
	call :ToPascalCase %1
	set s=!%1!
	set "%1="
	set pos=0
	:NextChar
		echo(!s:~%pos%,1!| findstr "^[ABCDEFGHIJKLMNOPQRSTUVWXYZ]*$" >nul && (  set "%1=!%1!!s:~%pos%,1!")
    	set /a pos=pos+1
    	if "!s:~%pos%,1!" NEQ "" goto NextChar
)(
	exit /b
)

:EXIT
exit /b