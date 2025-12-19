; ClipX NSIS Installer Script
; Creates a Windows installer for ClipX

!define PRODUCT_NAME "ClipX"
!ifndef PRODUCT_VERSION
  !define PRODUCT_VERSION "0.0.0-dev"
!endif
!define PRODUCT_PUBLISHER "Rajware Services Pvt. Ltd."
!define PRODUCT_WEB_SITE "https://github.com/rajware/clipx"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
!define PRODUCT_UNINST_ROOT_KEY "HKLM"

; Modern UI
!include "MUI2.nsh"
!include "EnvVarUpdate.nsh"

; MUI Settings
!define MUI_ABORTWARNING
!define MUI_ICON "${NSISDIR}\Contrib\Graphics\Icons\modern-install.ico"
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

; Welcome page
!insertmacro MUI_PAGE_WELCOME
; License page
!insertmacro MUI_PAGE_LICENSE "..\..\LICENSE"
; Directory page
!insertmacro MUI_PAGE_DIRECTORY
; Instfiles page
!insertmacro MUI_PAGE_INSTFILES
; Finish page
!define MUI_FINISHPAGE_RUN "$INSTDIR\clipx.exe"
!define MUI_FINISHPAGE_RUN_PARAMETERS "--version"
!insertmacro MUI_PAGE_FINISH

; Uninstaller pages
!insertmacro MUI_UNPAGE_INSTFILES

; Language
!insertmacro MUI_LANGUAGE "English"

; Installer attributes
Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
OutFile "..\..\dist\clipx-setup-${PRODUCT_VERSION}.exe"
InstallDir "$PROGRAMFILES64\ClipX"
InstallDirRegKey HKLM "${PRODUCT_UNINST_KEY}" "InstallLocation"
ShowInstDetails show
ShowUnInstDetails show

; Request admin privileges
RequestExecutionLevel admin

Section "MainSection" SEC01
  SetOutPath "$INSTDIR"
  SetOverwrite on
  
  ; Main executable
  File "..\..\publish\win-x64\clipx.exe"
  
  ; PowerShell completion
  File "..\..\completions\powershell\clipx-completion.ps1"
  
  ; Documentation
  File "..\..\README.md"
  File "..\..\LICENSE"
  
  CreateDirectory "$INSTDIR\docs"
  SetOutPath "$INSTDIR\docs"
  File "..\..\docs\INSTALLATION.md"
  File "..\..\docs\WINDOWS_INSTALLATION.md"
  
  SetOutPath "$INSTDIR"
SectionEnd

Section -AdditionalIcons
  CreateDirectory "$SMPROGRAMS\ClipX"
  CreateShortCut "$SMPROGRAMS\ClipX\ClipX.lnk" "$INSTDIR\clipx.exe"
  CreateShortCut "$SMPROGRAMS\ClipX\Uninstall.lnk" "$INSTDIR\uninst.exe"
SectionEnd

Section -Post
  WriteUninstaller "$INSTDIR\uninst.exe"
  WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
  WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
  WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$INSTDIR\clipx.exe"
  WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
  
  ; Add to PATH
  ${EnvVarUpdate} $0 "PATH" "A" "HKLM" "$INSTDIR"
SectionEnd

Section Uninstall
  ; Remove from PATH
  ${un.EnvVarUpdate} $0 "PATH" "R" "HKLM" "$INSTDIR"
  
  ; Remove files
  Delete "$INSTDIR\clipx.exe"
  Delete "$INSTDIR\clipx-completion.ps1"
  Delete "$INSTDIR\README.md"
  Delete "$INSTDIR\LICENSE"
  Delete "$INSTDIR\docs\INSTALLATION.md"
  Delete "$INSTDIR\docs\WINDOWS_INSTALLATION.md"
  Delete "$INSTDIR\uninst.exe"
  
  ; Remove directories
  RMDir "$INSTDIR\docs"
  RMDir "$INSTDIR"
  
  ; Remove shortcuts
  Delete "$SMPROGRAMS\ClipX\ClipX.lnk"
  Delete "$SMPROGRAMS\ClipX\Uninstall.lnk"
  RMDir "$SMPROGRAMS\ClipX"
  
  ; Remove registry keys
  DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"
  
  SetAutoClose true
SectionEnd
