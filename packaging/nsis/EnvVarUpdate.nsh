; EnvVarUpdate.nsh
; NSIS plugin for updating environment variables
; Download from: https://nsis.sourceforge.io/Environmental_Variables:_append,_prepend,_and_remove_entries

!ifndef _EnvVarUpdate_nsh
!define _EnvVarUpdate_nsh

!include "LogicLib.nsh"

!define Environ 'HKLM "SYSTEM\CurrentControlSet\Control\Session Manager\Environment"'

; AddToPath - Adds dir to PATH
; Usage: ${AddToPath} "C:\path\to\add"
!macro AddToPath dir
  Push "${dir}"
  Call AddToPath
!macroend

Function AddToPath
  Exch $0
  Push $1
  Push $2
  Push $3
  
  ReadRegStr $1 ${Environ} "PATH"
  ${If} $1 == ""
    WriteRegExpandStr ${Environ} "PATH" "$0"
    Goto AddToPath_done
  ${EndIf}
  
  StrCpy $2 "$1;"
  Push $2
  Push "$0;"
  Call StrStr
  Pop $3
  ${If} $3 == ""
    StrCpy $2 "$1;$0"
    WriteRegExpandStr ${Environ} "PATH" "$2"
  ${EndIf}
  
  AddToPath_done:
  Pop $3
  Pop $2
  Pop $1
  Pop $0
FunctionEnd

; RemoveFromPath - Removes dir from PATH
; Usage: ${RemoveFromPath} "C:\path\to\remove"
!macro RemoveFromPath dir
  Push "${dir}"
  Call un.RemoveFromPath
!macroend

Function un.RemoveFromPath
  Exch $0
  Push $1
  Push $2
  Push $3
  Push $4
  Push $5
  
  ReadRegStr $1 ${Environ} "PATH"
  ${If} $1 == ""
    Goto RemoveFromPath_done
  ${EndIf}
  
  StrCpy $2 "$1;"
  StrCpy $3 "$0;"
  StrLen $4 $3
  
  loop:
    Push $2
    Push $3
    Call un.StrStr
    Pop $5
    ${If} $5 == ""
      Goto done
    ${EndIf}
    StrLen $5 $5
    IntOp $5 $5 - $4
    StrCpy $2 $2 $5 0
    StrCpy $5 $2 "" $5
    IntOp $5 $5 + $4
    StrCpy $2 "$2$5"
    Goto loop
  
  done:
    StrCpy $2 $2 -1
    WriteRegExpandStr ${Environ} "PATH" "$2"
  
  RemoveFromPath_done:
  Pop $5
  Pop $4
  Pop $3
  Pop $2
  Pop $1
  Pop $0
FunctionEnd

; StrStr - Find substring
Function StrStr
  Exch $1
  Exch
  Exch $2
  Push $3
  Push $4
  Push $5
  
  StrCpy $3 $1
  StrCpy $4 ""
  StrLen $5 $2
  
  loop:
    StrCpy $4 $3 $5
    ${If} $4 == $2
      StrCpy $1 $3
      Goto done
    ${EndIf}
    ${If} $3 == ""
      StrCpy $1 ""
      Goto done
    ${EndIf}
    StrCpy $3 $3 "" 1
    Goto loop
  
  done:
  Pop $5
  Pop $4
  Pop $3
  Pop $2
  Exch $1
FunctionEnd

Function un.StrStr
  Exch $1
  Exch
  Exch $2
  Push $3
  Push $4
  Push $5
  
  StrCpy $3 $1
  StrCpy $4 ""
  StrLen $5 $2
  
  loop:
    StrCpy $4 $3 $5
    ${If} $4 == $2
      StrCpy $1 $3
      Goto done
    ${EndIf}
    ${If} $3 == ""
      StrCpy $1 ""
      Goto done
    ${EndIf}
    StrCpy $3 $3 "" 1
    Goto loop
  
  done:
  Pop $5
  Pop $4
  Pop $3
  Pop $2
  Exch $1
FunctionEnd

!define EnvVarUpdate '!insertmacro EnvVarUpdateMacro'
!macro EnvVarUpdateMacro ResultVar EnvVarName Action Root PathToAdd
  !insertmacro AddToPath "${PathToAdd}"
!macroend

!define un.EnvVarUpdate '!insertmacro un.EnvVarUpdateMacro'
!macro un.EnvVarUpdateMacro ResultVar EnvVarName Action Root PathToRemove
  !insertmacro RemoveFromPath "${PathToRemove}"
!macroend

!endif
