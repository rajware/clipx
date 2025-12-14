# Fish completion for clipx
# Install to ~/.config/fish/completions/

# Remove any existing completions
complete -c clipx -e

# Global options
complete -c clipx -s h -l help -d 'Show help information'
complete -c clipx -s v -l version -d 'Show version information'

# Commands
complete -c clipx -f -n __fish_use_subcommand -a copy -d 'Copy text from stdin to clipboard'
complete -c clipx -f -n __fish_use_subcommand -a paste -d 'Paste text from clipboard to stdout'
complete -c clipx -f -n __fish_use_subcommand -a history -d 'View clipboard history'
complete -c clipx -f -n __fish_use_subcommand -a restore -d 'Restore entry from history'
complete -c clipx -f -n __fish_use_subcommand -a clear -d 'Clear clipboard history'

# history command options
complete -c clipx -f -n '__fish_seen_subcommand_from history' -l limit -d 'Maximum entries to show' -a '5 10 25 50 0'

# restore command arguments
complete -c clipx -f -n '__fish_seen_subcommand_from restore' -a '1 2 3 4 5' -d 'Entry position'

# clear command options
complete -c clipx -f -n '__fish_seen_subcommand_from clear' -l all -d 'Clear all history'
complete -c clipx -f -n '__fish_seen_subcommand_from clear' -l before -d 'Clear entries before date'
complete -c clipx -f -n '__fish_seen_subcommand_from clear' -l force -d 'Skip confirmation'
