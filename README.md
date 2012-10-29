# Building
Treat the LSL Syntax Generator as you would a region module; Copy the
OpenSim.Tools.LSLSyntaxGenerator directory to the OpenSim addon-modules
directory, run the appropriate prebuild script then compile.

# Using
1. Run bin/OpenSim.Tools.LSLSyntaxGenerator.exe > path/to/file.json

# Serialisiation
An example of the JSON serialisation:
```javascript
{
	"OSSL" : {
		"osNpcSay" : [{
			":return":"Void",
			"npc":"LSLString",
			"message":"String"
		},{
			":return":"Void",
			"npc":"LSLString",
			"channel":"Int32",
			"message":"String"
		}]
	}
}
```

*	The first-level key indicates the script API
*	The second-level key indicates the function name
*	A function definition consists of an array of one or more maps with each
	map representing a method signature.

## function signatures
*	The return type of the function is specified with the key __:return__
*	The summary of the function is specified with the key __:summary__

# Command-Line Arguments

## --format
*	__--format=json__ is the default
*	__--format=mediawiki__ will output mediawiki syntax suitable for use in
	the OpenSim wiki. __--hide-documented__ will add additional syntax that
	will hide documented methods
*	__--format=llsd__ will output an XML-serialised LLSD document
*	__--format=xml__ is an alias for LLSD output

## --function
*	__--function__ is an optional parameter used to restrict output to only
	the named function. For example, __--function=osListenRegex__ would
	restrict output to just osListenRegex.

## --doxygen
*	__--doxygen__ should be the path to the directory containing
	doxygen-generated XML for OpenSim.

# FAQ

## Why are summaries not included in the output?
The [Doxygen](http://www.stack.nl/~dimitri/doxygen/index.html) config file
must be configured to generate [XML output](http://www.stack.nl/~dimitri/doxygen/config.html#cfg_generate_xml).
If Doxygen cannot generate XML for OpenSim, check the progress of
[OpenSim mantis 6387](http://opensimulator.org/mantis/view.php?id=6387)
before [filing an issue on this project](https://github.com/SignpostMarv/OpenSim.Tools.LSLSyntaxGenerator/issues).
Finally, the method might not actually be documented in the OpenSim source,
preventing any summaries from being generated.

# Support
Support can be obtained from SignpostMarv, either via the 
[GitHub issue tracker](https://github.com/SignpostMarv/OpenSim.Tools.LSLSyntaxGenerator/issues)
or on IRC where he usually hangs out on [Freenode](http://webchat.freenode.net/)
