// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;

namespace R2RTest
{
    internal static class CommandLineOptions
    {
        public static CommandLineBuilder Build()
        {
            var parser = new CommandLineBuilder()
                .AddCommand(CompileFolder())
                .AddCommand(CompileSubtree())
                .AddCommand(CompileFramework())
                .AddCommand(CompileNugetPackages())
                .AddCommand(CompileCrossgenRsp())
                .AddCommand(CompileSerp());

            return parser;

            Command CreateCommand(string name, string description, Option[] options, Func<BuildOptions, int> action)
            {
                Command command = new Command(name, description);
                foreach (var option in options)
                    command.AddOption(option);
                command.Handler = CommandHandler.Create<BuildOptions>(action);
                return command;
            }

            Command CompileFolder() =>
                CreateCommand("compile-directory", "Compile all assemblies in directory",
                    new Option[]
                    {
                        InputDirectory(),
                        OutputDirectory(),
                        CoreRootDirectory(),
                        Crossgen(),
                        CrossgenPath(),
                        NoJit(),
                        NoCrossgen2(),
                        Exe(),
                        NoExe(),
                        NoEtw(),
                        NoCleanup(),
                        Map(),
                        DegreeOfParallelism(),
                        Sequential(),
                        Framework(),
                        UseFramework(),
                        Release(),
                        LargeBubble(),
                        Composite(),
                        Crossgen2Parallelism(),
                        ReferencePath(),
                        IssuesPath(),
                        CompilationTimeoutMinutes(),
                        ExecutionTimeoutMinutes(),
                        R2RDumpPath(),
                        MeasurePerf(),
                        InputFileSearchString(),
                    },
                    CompileDirectoryCommand.CompileDirectory);

            Command CompileSubtree() =>
                CreateCommand("compile-subtree", "Build each directory in a given subtree containing any managed assemblies as a separate app",
                    new Option[]
                    {
                        InputDirectory(),
                        OutputDirectory(),
                        CoreRootDirectory(),
                        Crossgen(),
                        CrossgenPath(),
                        NoJit(),
                        NoCrossgen2(),
                        Exe(),
                        NoExe(),
                        NoEtw(),
                        NoCleanup(),
                        Map(),
                        DegreeOfParallelism(),
                        Sequential(),
                        Framework(),
                        UseFramework(),
                        Release(),
                        LargeBubble(),
                        Composite(),
                        Crossgen2Parallelism(),
                        ReferencePath(),
                        IssuesPath(),
                        CompilationTimeoutMinutes(),
                        ExecutionTimeoutMinutes(),
                        R2RDumpPath(),
                        GCStress(),
                    },
                    CompileSubtreeCommand.CompileSubtree);

            Command CompileFramework() =>
                CreateCommand("compile-framework", "Compile managed framework assemblies in Core_Root",
                    new Option[]
                    {
                        CoreRootDirectory(),
                        Crossgen(),
                        CrossgenPath(),
                        NoCrossgen2(),
                        NoCleanup(),
                        DegreeOfParallelism(),
                        Sequential(),
                        Release(),
                        LargeBubble(),
                        Composite(),
                        ReferencePath(),
                        IssuesPath(),
                        CompilationTimeoutMinutes(),
                        R2RDumpPath(),
                        MeasurePerf(),
                        InputFileSearchString(),
                    },
                    CompileFrameworkCommand.CompileFramework);

            Command CompileNugetPackages() =>
                CreateCommand("compile-nuget", "Restore a list of Nuget packages into an empty console app, publish, and optimize with Crossgen / CPAOT",
                    new Option[]
                    {
                        R2RDumpPath(),
                        InputDirectory(),
                        OutputDirectory(),
                        PackageList(),
                        CoreRootDirectory(),
                        Crossgen(),
                        NoCleanup(),
                        DegreeOfParallelism(),
                        CompilationTimeoutMinutes(),
                        ExecutionTimeoutMinutes(),
                    },
                    CompileNugetCommand.CompileNuget);

            Command CompileCrossgenRsp() =>
                CreateCommand("compile-crossgen-rsp", "Use existing Crossgen .rsp file(s) to build assemblies, optionally rewriting base paths",
                    new Option[]
                    {
                        InputDirectory(),
                        CrossgenResponseFile(),
                        OutputDirectory(),
                        CoreRootDirectory(),
                        Crossgen(),
                        NoCleanup(),
                        DegreeOfParallelism(),
                        CompilationTimeoutMinutes(),
                        RewriteOldPath(),
                        RewriteNewPath(),
                    },
                    CompileFromCrossgenRspCommand.CompileFromCrossgenRsp);

            Command CompileSerp() =>
                CreateCommand("compile-serp", "Compile existing application",
                    new Option[]
                    {
                        InputDirectory(),
                        OutputDirectory(),
                        DegreeOfParallelism(),
                        CoreRootDirectory(),
                        AspNetPath(),
                        Composite(),
                        PartialComposite(),
                    },
                    CompileSerpCommand.CompileSerpAssemblies);

            // Todo: Input / Output directories should be required arguments to the command when they're made available to handlers
            // https://github.com/dotnet/command-line-api/issues/297
            Option InputDirectory() =>
                new Option<DirectoryInfo>(new[] { "--input-directory", "-in" }, "Folder containing assemblies to optimize").ExistingOnly();

            Option OutputDirectory() =>
                new Option<DirectoryInfo>(new[] { "--output-directory", "-out" }, "Folder to emit compiled assemblies").LegalFilePathsOnly();

            Option CoreRootDirectory() =>
                new Option<DirectoryInfo>(new[] { "--core-root-directory", "-cr" }, "Location of the CoreCLR CORE_ROOT folder").ExistingOnly();

            Option ReferencePath() =>
                new Option<DirectoryInfo[]>(new[] { "--reference-path", "-r" }, "Folder containing assemblies to reference during compilation")
                    { Argument = new Argument<DirectoryInfo[]>() { Arity = ArgumentArity.ZeroOrMore }.ExistingOnly() };

            Option Crossgen() =>
                new Option<bool>(new[] { "--crossgen" }, "Compile the apps using Crossgen in the CORE_ROOT folder");

            Option CrossgenPath() =>
                new Option<FileInfo>(new[] { "--crossgen-path", "-cp" }, "Explicit Crossgen path (useful for cross-targeting)").ExistingOnly();

            Option NoJit() =>
                new Option<bool>(new[] { "--nojit" }, "Don't run tests in JITted mode");

            Option NoCrossgen2() =>
                new Option<bool>(new[] { "--nocrossgen2" }, "Don't run tests in Crossgen2 mode");

            Option Exe() =>
                new Option<bool>(new[] { "--exe" }, "Don't compile tests, just execute them");

            Option NoExe() =>
                new Option<bool>(new[] { "--noexe" }, "Compilation-only mode (don't execute the built apps)");

            Option NoEtw() =>
                new Option<bool>(new[] { "--noetw" }, "Don't capture jitted methods using ETW");

            Option NoCleanup() =>
                new Option<bool>(new[] { "--nocleanup" }, "Don't clean up compilation artifacts after test runs");

            Option Map() =>
                new Option<bool>(new[] { "--map" }, "Generate a map file (Crossgen2)");

            Option DegreeOfParallelism() =>
                new Option<int>(new[] { "--degree-of-parallelism", "-dop" }, "Override default compilation / execution DOP (default = logical processor count)");

            Option Sequential() =>
                new Option<bool>(new[] { "--sequential" }, "Run tests sequentially");

            Option Framework() =>
                new Option<bool>(new[] { "--framework" }, "Precompile and use native framework");

            Option UseFramework() =>
                new Option<bool>(new[] { "--use-framework" }, "Use native framework (don't precompile, assume previously compiled)");

            Option Release() =>
                new Option<bool>(new[] { "--release" }, "Build the tests in release mode");

            Option LargeBubble() =>
                new Option<bool>(new[] { "--large-bubble" }, "Assume all input files as part of one version bubble");

            Option Composite() =>
                new Option<bool>(new[] { "--composite" }, "Compile tests in composite R2R mode");

            Option Crossgen2Parallelism() =>
                new Option<int>(new[] { "--crossgen2-parallelism" }, "Max number of threads to use in Crossgen2 (default = logical processor count)");

            Option IssuesPath() =>
                new Option<FileInfo[]>(new[] { "--issues-path", "-ip" }, "Path to issues.targets")
                    { Argument = new Argument<FileInfo[]>() { Arity = ArgumentArity.ZeroOrMore } };

            Option CompilationTimeoutMinutes() =>
                new Option<int>(new[] { "--compilation-timeout-minutes", "-ct" }, "Compilation timeout (minutes)");

            Option ExecutionTimeoutMinutes() =>
                new Option<int>(new[] { "--execution-timeout-minutes", "-et" }, "Execution timeout (minutes)");

            Option R2RDumpPath() =>
                new Option<FileInfo>(new[] { "--r2r-dump-path", "-r2r" }, "Path to R2RDump.exe/dll").ExistingOnly();;

            Option CrossgenResponseFile() =>
                new Option<FileInfo>(new [] { "--crossgen-response-file", "-rsp" }, "Response file to transpose").ExistingOnly();;

            Option RewriteOldPath() =>
                new Option<DirectoryInfo[]>(new[] { "--rewrite-old-path" }, "Path substring to replace")
                    { Argument = new Argument<DirectoryInfo[]>() { Arity = ArgumentArity.ZeroOrMore } };

            Option RewriteNewPath() =>
                new Option<DirectoryInfo[]>(new[] { "--rewrite-new-path" }, "Path substring to use instead")
                    { Argument = new Argument<DirectoryInfo[]>() { Arity = ArgumentArity.ZeroOrMore } };

            Option MeasurePerf() =>
                new Option<bool>(new[] { "--measure-perf" }, "Print out compilation time");

            Option InputFileSearchString() =>
                new Option<string>(new[] { "--input-file-search-string", "-input-file" }, "Search string for input files in the input directory");

            Option GCStress() =>
                new Option<string>(new[] { "--gcstress" }, "Run tests with the specified GC stress level enabled (the argument value is in hex)");

            //
            // compile-nuget specific options
            //
            Option PackageList() =>
                new Option<FileInfo>(new[] { "--package-list", "-pl" }, "Text file containing a package name on each line").ExistingOnly();;

            //
            // compile-serp specific options
            //
            Option AspNetPath() =>
                new Option<DirectoryInfo>(new[] { "--asp-net-path", "-asp" }, "Path to SERP's ASP.NET Core folder").ExistingOnly();

            Option PartialComposite() =>
                new Option<bool>(new[] { "--partial-composite", "-pc" }, "Add references to framework and asp.net instead of unrooted inputs");
        }
    }
}
