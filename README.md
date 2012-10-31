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

## With --include-script-modules
An example of the JSON serialisation of a script module, using the
[Math Module](https://github.com/SignpostMarv/TSU.CCIR.OpenSim.Math)
as an example:
```javascript
{
	"TeessideUniversity.CCIR.OpenSim.MathModule" : {
		"mathVecMultiply" : [{
			":return":"Vector3",
			"a":"Vector3",
			"b":"Vector3"
		}],
		"mathVecDivide" : [{
			":return":"Vector3",
			"a":"Vector3",
			"b":"Vector3"
		}],
		"mathVecFloor" : [{
			":return":"Vector3",
			"a":"Vector3"
		}],
		"mathVecRound" : [{
			":return":"Vector3",
			"a":"Vector3"
		}],
		"mathVecCeil" : [{
			":return":"Vector3",
			"a":"Vector3"
		}],
		"mathVecMin" : [{
			":return":"Vector3",
			"a":"Vector3",
			"b":"Single"
		}],
		"mathVecMax" : [{
			":return":"Vector3",
			"a":"Vector3",
			"b":"Single"
		}],
		"mathVecVolume" : [{
			":return":"Single",
			"a":"Vector3"
		}],
		"mathFibonacci" : [{
			":return":"Object[]",
			"n":"Int32",
			"length":"Int32"
		}]
	},
	"ScriptConstants" : [
		"MATH_PHI",
		"MATH_PSI",
		"MATH_TAU",
		"MATH_TAU_BY_TWO",
		"MATH_TWO_TAU"
	]
}
```

*	The first-level key indicates the script API
*	The second-level key indicates the function name
*	A function definition consists of an array of one or more maps with each
	map representing a method signature.

## function signatures
*	The return type of the function is specified with the key __:return__
*	The summary of the function is specified with the key __:summary__
	_The LSL Syntax Generator does not currently include function summaries._

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

## --include-script-modules
*	__--include-script-modules__ is an optional flag used to include functions
	and constants added via script modules in the output.

# Support
Support can be obtained from SignpostMarv, either via the 
[GitHub issue tracker](https://github.com/SignpostMarv/OpenSim.Tools.LSLSyntaxGenerator/issues)
or on IRC where he usually hangs out on [Freenode](http://webchat.freenode.net/)
