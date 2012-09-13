/*
 * Copyright (c) Contributors, Teesside University Centre for Construction Innovation and Research
 * See CONTRIBUTORS.md for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSimulator Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Nini.Util;

using OpenMetaverse.StructuredData;

using OpenSim.Region.ScriptEngine.Shared.ScriptBase;
using OpenSim.Region.ScriptEngine.Shared.Api.Interfaces;

namespace TeessideUniversity.CCIR.OpenSim.Tools
{
    class LSLSyntaxGenerator
    {
        /// <summary>
        /// Generates and writes the LSL Syntax helper data to stdout
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            List<string> argsList = new List<string>(args);
            ArgvParser argv = new ArgvParser(args);

            string format = argv["format"].ToLower();
            string function = argv["function"].ToLower();

            switch (format)
            {
                case "json":
                    Console.Write(JSON(function));
                    break;
                case "xml":
                case "llsd":
                    Console.Write(LLSD(function));
                    break;
                case "mediawiki":
                    if (function == string.Empty)
                    {
                        Console.Write(MediaWiki(2,
                                argsList.Contains("--hide-documented")));
                    }
                    else
                    {
                        Console.Write(MediaWiki(function));
                    }
                    break;
                default:
                    Console.Error.Write("Unsupported format specified");
                    break;
            }
        }

        public static readonly Dictionary<string, Type> ScriptAPIs = new Dictionary<string, Type>{
            {"LSL", typeof(ILSL_Api)},
            {"OSSL", typeof(IOSSL_Api)},
            {"LS", typeof(ILS_Api)},
            {"MOD", typeof(IMOD_Api)}
        };

        public static readonly Dictionary<string, List<string>> ExcludedFromOutput = new Dictionary<string, List<string>>{
            {"LSL", new List<string>{
                "GetPrimitiveParamsEx",
                "SetPrimitiveParamsEx",
                "state"
            }},
            {"OSSL", new List<string>{
                "CheckThreatLevel"
            }},
            {"LS", new List<string>{

            }},
            {"MOD", new List<string>{

            }}
        };

        /// <summary>
        /// Gets the LSL Syntax helper data
        /// </summary>
        /// <returns>an OSDMap of script methods and constants</returns>
        public static Dictionary<string, IEnumerable> SyntaxHelpers()
        {
            Dictionary<string, IEnumerable> output;
            output = new Dictionary<string, IEnumerable>();

            foreach (string API in ScriptAPIs.Keys)
                output[API] = GetMethods(API);

            output["ScriptConstants"] = ScriptConstants();

            return output;
        }

        public static Dictionary<string, IEnumerable> SyntaxHelpers(string function)
        {
            Dictionary<string, IEnumerable> syntax = SyntaxHelpers();

            if (function != string.Empty)
            {
                foreach (KeyValuePair<string, IEnumerable> kvp in syntax)
                {
                    if (kvp.Value is Dictionary<string, List<Dictionary<string, string>>>)
                    {
                        foreach (string kvpFunc in ((Dictionary<string, List<Dictionary<string, string>>>)kvp.Value).Keys)
                        {
                            if (kvpFunc.ToLower() == function.ToLower())
                            {
                                return new Dictionary<string, IEnumerable>{
                                    {kvp.Key, new Dictionary<string, List<Dictionary<string,string>>>{
                                        {kvpFunc, ((Dictionary<string, List<Dictionary<string, string>>>)kvp.Value)[kvpFunc]}
                                    }}
                                };
                            }
                        }
                    }
                }

                return new Dictionary<string, IEnumerable>(0);
            }

            return syntax;
        }

        /// <summary>
        /// Deduplicates the code for converting a string List to an OSDArray
        /// </summary>
        /// <param name="input">The list to be converted</param>
        /// <returns>The OSDArray representing the input</returns>
        private static OSDArray ToOSDArray(List<string> input)
        {
            return (new OSDArray(input.ConvertAll<OSD>(x =>
            {
                return (OSDString)x;
            })));
        }

        private static Dictionary<string, List<Dictionary<string, string>>> GetMethods(string typeLabel)
        {
            Dictionary<string, List<Dictionary<string, string>>> resp;
            resp = new Dictionary<string, List<Dictionary<string, string>>>();

            if (!ScriptAPIs.ContainsKey(typeLabel))
                return resp;

            Type type = ScriptAPIs[typeLabel];

            List<string> excludes = new List<string>(0);
            if (ExcludedFromOutput.ContainsKey(typeLabel))
                excludes = ExcludedFromOutput[typeLabel];

            foreach (MethodInfo method in type.GetMethods())
            {
                if (excludes.Contains(method.Name))
                    continue;

                Dictionary<string, string> temp;
                temp = new Dictionary<string, string>{
                    // ":return" isn't a valid lazy JSON object key nor is it
                    // a valid c# parameter name, so we're using that to
                    // indicate the return type. Any future non-argument
                    // metadata will be added in this fashion in future.
                    {":return", method.ReturnType.Name}
                };

                foreach (ParameterInfo param in method.GetParameters())
                    temp[param.Name] = param.ParameterType.Name;

                if (!resp.ContainsKey(method.Name))
                    resp[method.Name] = new List<Dictionary<string, string>>();

                resp[method.Name].Add(temp);
            }

            return resp;
        }

        private static OSDMap ToOSDMap(
                Dictionary<string, IEnumerable> dictionary)
        {
            OSDMap resp = new OSDMap();

            foreach (KeyValuePair<string, IEnumerable> kvpA in dictionary)
            {
                if (kvpA.Value is IEnumerable<string>)
                {
                    resp[kvpA.Key] = ToOSDArray(new List<string>(
                            (IEnumerable<string>)(kvpA.Value)));
                }
                else if (kvpA.Value is Dictionary<string, List<Dictionary<string, string>>>)
                {
                    OSDMap temp = new OSDMap();
                    foreach (KeyValuePair<string, List<Dictionary<string, string>>> kvpB in kvpA.Value)
                    {
                        List<OSD> temp2 = new List<OSD>();
                        foreach (Dictionary<string, string> kvpC in kvpB.Value)
                        {
                            OSDMap temp3 = new OSDMap();
                            foreach (KeyValuePair<string, string> kvpD in kvpC)
                                temp3[kvpD.Key] = kvpD.Value;

                            temp2.Add(temp3);
                        }

                        temp[kvpB.Key] = new OSDArray(temp2);
                    }

                    resp[kvpA.Key] = temp;
                }
                else
                {
                    throw new Exception("Cannot convert IEnumerable of type " + kvpA.Value.GetType().Name);
                }
            }

            return resp;
        }

        /// <summary>
        /// Gets the constants defined on ScriptBaseClass
        /// </summary>
        /// <returns>A list of script constants</returns>
        private static List<string> ScriptConstants()
        {
            List<string> resp = new List<string>();

            foreach (FieldInfo field in typeof(ScriptBaseClass).GetFields(
                    BindingFlags.Public | BindingFlags.Static))
            {
                if (field.IsLiteral && !field.IsInitOnly)
                    resp.Add(field.Name);
            }

            resp.Sort();

            return resp;
        }

        /// <summary>
        /// JSON serialisation
        /// </summary>
        /// <returns>JSON serialisation of functions and constants</returns>
        public static string JSON(string function)
        {
            Dictionary<string, IEnumerable> syntax = SyntaxHelpers(function);
            if (syntax.Count() < 1)
            {
                Console.Error.Write(string.Format(
                        "No function named \"{0}\" could be found."));
                return "";
            }

            return OSDParser.SerializeJson(ToOSDMap(
                    syntax), true).ToJson().ToString();
        }

        /// <summary>
        /// LLSD serialisation
        /// </summary>
        /// <returns>LLSD serialisation of functions and constants</returns>
        public static string LLSD(string function)
        {
            Dictionary<string, IEnumerable> syntax = SyntaxHelpers(function);
            if (syntax.Count() < 1)
            {
                Console.Error.Write(string.Format(
                        "No function named \"{0}\" could be found."));
                return "";
            }

            return OSDParser.SerializeLLSDXmlString(
                    ToOSDMap(syntax));
        }

        private static List<string> FunctionSyntax(KeyValuePair<string, List<Dictionary<string, string>>> functionSignatures)
        {
            List<string> resp = new List<string>();

            foreach(Dictionary<string, string> signature  in functionSignatures.Value)
            {
                List<string> args = new List<string>();
                foreach(string arg in signature.Keys)
                {
                    if(!arg.StartsWith(":"))
                        args.Add(string.Format("{1} {0}", arg, signature[arg]));
                }

                resp.Add(string.Format("{1} {0}({2})",
                        functionSignatures.Key,
                        signature.ContainsKey(":return") ? signature[":return"] : "void",
                        string.Join(", ", args.ToArray())));
            }

            return resp;
        }

        public static string MediaWiki(string function)
        {
            Dictionary<string, IEnumerable> syntax = SyntaxHelpers(function);
            if (syntax.Count() < 1)
            {
                Console.Error.Write(string.Format(
                        "No function named \"{0}\" could be found."));
                return "";
            }
            else if (syntax.Keys.First<string>() != "OSSL")
            {
                Console.Error.Write(string.Format(
                        "{0} is not a supported API for single-function MediaWiki output"));
            }

            List<string> output = new List<string>();

            Dictionary<string, List<Dictionary<string, string>>> functionDefinitions = (Dictionary<string, List<Dictionary<string, string>>>)syntax[syntax.Keys.First<string>()];

            // correcting the casing
            function = functionDefinitions.Keys.First<string>();

            List<string> signatures = FunctionSyntax(
                    new KeyValuePair<string, List<Dictionary<string, string>>>(
                    function, functionDefinitions[function]));

            foreach (string signature in signatures)
            {
                output.AddRange(new string[]{
                    "{{osslfunc",
                    "|function_syntax=" + signature,
                    "|",
                    "}}"
                });
            }

            return string.Join("\n", output.ToArray()).Trim();
        }

        /// <summary>
        /// MediaWiki syntax
        /// </summary>
        /// <param name="headerLevel">
        /// -1 ommits API headers, 0 includes the API name but omits header
        /// syntax, 1-6 will use the appropriate header syntax
        /// </param>
        /// <param name="hideDocumented">
        /// if TRUE, uses the ParserFunctions extension ifexist function to
        /// hide links, as well as omitting the ILSL_Api section entirely.
        /// </param>
        /// <returns>
        /// MediaWiki syntax list of functions and contstants
        /// </returns>
        public static string MediaWiki(int headerLevel, bool hideDocumented)
        {
            headerLevel = Math.Max(-1, Math.Min(6, headerLevel));
            Dictionary<string, IEnumerable> input = SyntaxHelpers();

            List<string> output = new List<string>();

            string headerTags = new string('=', headerLevel);
            string header = headerTags + " {0} " + headerTags;

            string headerLSL = string.Format(header,
                    "[[LSL_Status/Functions|LSL]]");
            string headerOSSL = string.Format(header,
                    "[[:Category:OSSL Functions|OSSL]]");

            string link = "* [[{0}]]";
            string linkHideDocumented = "{{{{#ifexist: {0}||" + link + "\n" + "}}}}";
            string linkLSL = "* [https://wiki.secondlife.com/wiki/{0} {0}]";

            foreach (string API in input.Keys)
            {
                if (headerLevel > 0)
                {
                    if (API == "LSL" && !hideDocumented)
                        output.Add(headerLSL.Trim());
                    else if (API == "OSSL")
                        output.Add(headerOSSL.Trim());
                    else if (API == "ScriptConstants")
                        output.Add(string.Format(header, "Script Constants"));
                    else if (API != "LSL")
                        output.Add(string.Format(header, API).Trim());

                    if (hideDocumented)
                        output.Add("\n\n");
                }

                if (API == "ScriptConstants")
                {
                    foreach (string constant in input[API])
                    {
                        output.Add(string.Format(
                                hideDocumented ? linkHideDocumented : link,
                                constant));
                    }
                }
                else
                {
                    Dictionary<string, List<Dictionary<string, string>>> foo;
                    foo = (Dictionary<string, List<Dictionary<string, string>>>)input[API];
                    List<string> funcs = foo.Keys.ToList<string>();
                    funcs.Sort();

                    if (API == "LSL")
                    {
                        if (!hideDocumented)
                        {
                            foreach (string func in funcs)
                            {
                                output.Add(string.Format(linkLSL, func));
                            }
                        }
                    }
                    else
                    {
                        foreach (string func in funcs)
                        {
                            output.Add(string.Format(
                                hideDocumented ? linkHideDocumented : link,
                                func));
                        }
                    }

                    output.Add("\n");
                }
            }

            return string.Join(hideDocumented ? "" : "\n", output.ToArray()).Trim();
        }
    }
}
