# PowerShell completion for clipx
# Add to your PowerShell profile: . path\to\clipx-completion.ps1

Register-ArgumentCompleter -Native -CommandName clipx -ScriptBlock {
    param($wordToComplete, $commandAst, $cursorPosition)
    
    $commands = @(
        [System.Management.Automation.CompletionResult]::new('copy', 'copy', 'ParameterValue', 'Copy text from stdin to clipboard')
        [System.Management.Automation.CompletionResult]::new('paste', 'paste', 'ParameterValue', 'Paste text from clipboard to stdout')
        [System.Management.Automation.CompletionResult]::new('history', 'history', 'ParameterValue', 'View clipboard history')
        [System.Management.Automation.CompletionResult]::new('restore', 'restore', 'ParameterValue', 'Restore entry from history')
        [System.Management.Automation.CompletionResult]::new('clear', 'clear', 'ParameterValue', 'Clear clipboard history')
        [System.Management.Automation.CompletionResult]::new('--help', '--help', 'ParameterValue', 'Show help information')
        [System.Management.Automation.CompletionResult]::new('--version', '--version', 'ParameterValue', 'Show version information')
    )
    
    $tokens = $commandAst.ToString() -split '\s+'
    
    if ($tokens.Count -eq 1) {
        # Complete main commands
        $commands | Where-Object { $_.CompletionText -like "$wordToComplete*" }
    }
    elseif ($tokens.Count -eq 2) {
        # Complete options for specific commands
        switch ($tokens[1]) {
            'history' {
                [System.Management.Automation.CompletionResult]::new('--limit', '--limit', 'ParameterValue', 'Maximum entries to show')
            }
            'restore' {
                1..5 | ForEach-Object {
                    [System.Management.Automation.CompletionResult]::new($_.ToString(), $_.ToString(), 'ParameterValue', "Entry position $_")
                }
            }
            'clear' {
                @(
                    [System.Management.Automation.CompletionResult]::new('--all', '--all', 'ParameterValue', 'Clear all history')
                    [System.Management.Automation.CompletionResult]::new('--before', '--before', 'ParameterValue', 'Clear entries before date')
                    [System.Management.Automation.CompletionResult]::new('--force', '--force', 'ParameterValue', 'Skip confirmation')
                )
            }
        }
    }
    elseif ($tokens.Count -eq 3 -and $tokens[1] -eq 'history' -and $tokens[2] -eq '--limit') {
        # Suggest common limit values
        5, 10, 25, 50, 0 | ForEach-Object {
            [System.Management.Automation.CompletionResult]::new($_.ToString(), $_.ToString(), 'ParameterValue', "$_ entries")
        }
    }
}
