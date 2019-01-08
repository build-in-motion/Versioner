using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandLine;

namespace Versioner
{
    class Program
    {
        public Program()
        {
        }

        static void Main(string[] args)
        {
            try
            {
                var program = new Program();
                var task = program.Run(args);
                task.Wait();

                if (task.IsFaulted)
                    throw new Exception("An error occurred while running task");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                Environment.ExitCode = -1;
            }
        }


        async Task Run(string[] args)
        {            
            var result = CommandLine.Parser.Default.ParseArguments<Options>(args);            
            if (result.Tag == ParserResultType.NotParsed)
                throw new Exception("Failed to parse command line arguments");

            Options options = null;
            result.WithParsed((o) => options = o);
            string branchName = options.BranchName ?? "";
            if (!string.IsNullOrWhiteSpace(options.GitConfigPath))
                branchName = this.GetBranchName(options) ?? options.BranchName ?? "";

            string version = null;
            if (!string.IsNullOrWhiteSpace(options.VersionFilePath))
            {
                using (var stream = new StreamReader(new FileStream(options.VersionFilePath, FileMode.Open, FileAccess.Read)))
                {
                    version = await stream.ReadToEndAsync();
                    version = version.Replace("\n", string.Empty);
                }
            }
            version = (version.EndsWith(".")) ? $"{version}{options.Minor}" : $"{version}.{options.Minor}";
            

            if (options.UpdateAssemblyInfo)
            {
                await this.SetAssemblyInfo(version);
            }

            var postfix = "";
            if (branchName.Trim().ToLower().Equals("develop"))
            {
                postfix = "-beta";
            }

            using (var stream = new StreamWriter(new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "gitversion.properties"), FileMode.Create, FileAccess.Write)))
            {
                await stream.WriteLineAsync($"GitVersion_NuGetVersion={version}{postfix}");
                await stream.FlushAsync();
            }
        }

        string GetBranchName(Options options)
        {
            try
            {
                var gitPath = (options.GitConfigPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".git/"));
                var headFilePath = Path.Combine(gitPath, "HEAD");
                string line = null;
                using (var reader = new StreamReader(new FileStream(headFilePath, FileMode.Open, FileAccess.Read)))
                {
                     line = reader.ReadToEnd();
                }

                var pattern = new Regex("^(ref:) .(refs/heads/)(.)*$", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                var match = pattern.Match(line);

                if (!match.Success)
                    throw new Exception("Failed to match pattern");

                return line.Substring((match.Groups[3].Index - match.Groups[3].Captures.Count) + 1, match.Groups[3].Captures.Count);                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;
        }

        async Task SetAssemblyInfo(string version)
        {
            var expression = new Regex("Assembly(File)?Version\\(\\\"([a-zA-Z\\.0-9\\*]*)\\\"\\)");

            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "AssemblyInfo.cs", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var found = false;
                var tempPath = $"{file}.tmp";
                using (var reader = new StreamReader(new FileStream(file, FileMode.Open, FileAccess.Read)))
                {
                    using (var writer = new StreamWriter(new FileStream(tempPath, FileMode.Create, FileAccess.Write)))
                    {
                        string line = null;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            var match = expression.Match(line);
                            if (!string.IsNullOrWhiteSpace(match.Value))
                            {
                                var newValue = match.Value.Replace(match.Groups[2].Value, version);
                                await writer.WriteLineAsync(expression.Replace(line, newValue));
                            }
                            else
                                await writer.WriteLineAsync(line);
                            found = true;
                        }
                        await writer.FlushAsync();
                    }
                }
                File.Delete(file);
                File.Move(tempPath, file);
                if (found)
                    Console.WriteLine($"Replaced version info in file {file}");
            }
        }
    }
}
