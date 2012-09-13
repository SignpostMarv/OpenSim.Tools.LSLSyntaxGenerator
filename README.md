# Building
Treat the LSL Syntax Generator as you would a region module; Copy the
OpenSim.Tools.LSLSyntaxGenerator directory to the OpenSim addon-modules
directory, run the appropriate prebuild script then compile.

# Using
1. Run bin/OpenSim.Tools.LSLSyntaxGenerator.exe > path/to/file.json

# Command-Line Arguments

## --format
*	__--format=json__ is the default
*	__--format=mediawiki__ will output mediawiki syntax suitable for use in the OpenSim wiki
	*	__--hide-documented__ will add additional syntax that will hide documented methods
*	__--format=llsd__ will output an XML-serialised LLSD document
*	__--format=xml__ is an alias for LLSD output

# Support
Support can be obtained from SignpostMarv, either via the 
[GitHub issue tracker](https://github.com/SignpostMarv/OpenSim.Tools.LSLSyntaxGenerator/issues)
or on IRC where he usually hangs out in 
[#opensim-dev](http://webchat.freenode.net/?channels=#opensim-dev)
