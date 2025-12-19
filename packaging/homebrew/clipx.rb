# ClipX Homebrew Formula
# This formula allows macOS users to install ClipX via Homebrew

class Clipx < Formula
  desc "Cross-platform clipboard manager with history"
  homepage "https://github.com/rajware/clipx"
  version "__VERSION__"
  
  on_macos do
    if Hardware::CPU.arm?
      url "https://github.com/rajware/clipx/releases/download/v__VERSION__/clipx-macos-arm64.tar.gz"
      sha256 "__SHA256_ARM64__"
    else
      url "https://github.com/rajware/clipx/releases/download/v__VERSION__/clipx-macos-x64.tar.gz"
      sha256 "__SHA256_X64__"
    end
  end

  def install
    bin.install "clipx"
    
    # Install man pages
    man1.install "docs/man/clipx.1"
    man1.install "docs/man/clipx-history.1"
    man1.install "docs/man/clipx-restore.1"
    man1.install "docs/man/clipx-clear.1"
    
    # Install zsh completion
    zsh_completion.install "completions/zsh/_clipx"
    
    # Install bash completion
    bash_completion.install "completions/bash/clipx"
  end

  test do
    assert_match "ClipX", shell_output("#{bin}/clipx --version")
  end
end
