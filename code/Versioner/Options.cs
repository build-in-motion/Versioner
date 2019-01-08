using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Versioner
{
    public class Options
    {
        [Option('v', "versionFilePath", Required = true, HelpText = "Input file to be used for the main version string")]
        public virtual string VersionFilePath { get; set; }

        [Option('m', "minor", Required = true, HelpText = "Minor version number to be used")]
        public virtual string Minor { get; set; }

        [Option('u', "updateAssemblyInfo", HelpText = "Update the assembly info files with the version")]
        public virtual bool UpdateAssemblyInfo { get; set; }

        [Option('g', "gitConfigPath", Required = false, HelpText = "The path for the git configuration path that should be parsed")]
        public virtual string GitConfigPath { get; set; }

        [Option('b', "branchName", Required = false, HelpText = "The name of the current branch that is being worked on")]
        public virtual string BranchName { get; set; }
    }
}
