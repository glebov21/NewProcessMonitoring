;NSIS Modern User Interface
;Basic Example Script
;Written by Joost Verburg

;--------------------------------
;Include Modern UI

  !include "MUI2.nsh"

;--------------------------------
;General

  ;Name and file
  Name "New Process Monitoring"
  OutFile "NewPMSetup.exe"
  Unicode True

  ;Default installation folder
  InstallDir "$LOCALAPPDATA\NewPm"
  
  ;Get installation folder from registry if available
  InstallDirRegKey HKCU "Software\NewPm" ""

  ;Request application privileges for Windows Vista
  RequestExecutionLevel user 

;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING

;--------------------------------
;Pages

;  !insertmacro MUI_PAGE_LICENSE "${NSISDIR}\Docs\Modern UI\License.txt"
;  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  
;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Installer Sections

Section "Dummy Section" SecDummy

  SetOutPath "$INSTDIR"
  
  ;ADD YOUR OWN FILES HERE...
  File /r "bin\Release\*"
  
  ;Store installation folder
  WriteRegStr HKCU "Software\NewPm" "" $INSTDIR

  ;Autorun
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "NewPm" "$InstDir\NewProcessMonitoring.exe"

  ;Shortcut
  CreateDirectory "$SMPrograms\NewPm"
  CreateShortcut "$SMPrograms\NewPm\NewPm.lnk" "$InstDir\NewProcessMonitoring.exe"

  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  CreateShortcut "$SMPrograms\NewPm\Uninstall NewPm.lnk" "$InstDir\Uninstall.exe"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\NewPm" \
                 "DisplayName" "New Process Monitoring Tool"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\NewPm" \
                 "UninstallString" "$\"$INSTDIR\Uninstall.exe$\""

  System::Call 'shell32.dll::SHChangeNotify(i, i, i, i) v (0x08000000, 0, 0, 0)'
  Exec '"$WINDIR\explorer.exe" "$InstDir\NewProcessMonitoring.exe"'
SectionEnd

;--------------------------------
;Descriptions

  ;Language strings
  LangString DESC_SecDummy ${LANG_RUSSIAN} "A test section."

  ;Assign language strings to sections
  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SecDummy} $(DESC_SecDummy)
  !insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------
;Uninstaller Section

Section "Uninstall"

  ;ADD YOUR OWN FILES HERE...

  Delete "$INSTDIR\Uninstall.exe"

  RMDir /r "$INSTDIR"

  Delete "$SMPrograms\NewPm\NewPm.lnk"
  Delete "$SMPrograms\NewPm\Uninstall NewPm.lnk"

  DeleteRegKey /ifempty HKCU "Software\NewPm"
  DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "NewPm"
  DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\NewPm"

SectionEnd