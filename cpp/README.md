# EDApplication
Embedded debugger PC application.

The Embedded Debugger is a rewrite of the C# application that can be used to debug embedded systems at a high level. 
This tool allows engineers to write to registers, read from, plot and log values from registers. 

The tool is a work in progress and cannot yet be used in projects.
More info about the goal of the application can be found in the ApplicationInformation.pdf

TODO:
- [ ] Fix implementation. Currently the embedded emulator outputs a unreleased debug protocol. Cannot test this application against it at the moment.
- [ ] Make a doxygen output for the brances
- [ ] Auto compile after commit with Appveyor / Travis CI
- [ ] Add Senty.io to this application so we get crash logs from users.
- [ ] Add missing parts that are already available in the C# application.