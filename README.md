# ClipX

This repository contains the source code for ClipX, a cross-platform clipboard manager. The application is written in C# and uses the .NET 8.0 framework.

This application is the demo part of a session that demonstrates the principles of cross-platform app development, from the perspectives of design, development, testing and distribution. 

The session covers the following points:

* Cross-platform is a necessity for the kinds of apps that need to be installed locally, because: 
  * People use different platforms
  * People want to install applications locally
  * People expect a familiar UX
  * Platforms expect integration
  * Here, "platform" includes OS and processor architecture
* Cross platform begins at design stage:
  * Differences should be identified and abstracted.
* Cross-platform development considerations: 
  * The importance of cross-platform testing, providing some tooling options
  * The development experience itself also has cross-platform considerations, such as the availability of common tools like make or bash, and providing alternatives such as PowerShell
* Cross-platform distribution
  * The importance of distribution, and how it is not limited to just providing an executable or script


ClipX, the cross-platform clipboard manager, consists of two components:

* ClipX: this is a CLI application that allows:
  * Copying text read from stdin to the clipboard
  * Pasting text read from the clipboard to stdout
  * Saving text read from the clipboard to a named file
  * Maintaining a round-robin history of clipboard contents
  * Syncing this history to a storage service
* ClipX.server: this is a server application that allows:
  * users to log in securely
  * syncing clipboard history to and from the server
