#!/usr/bin/fsharpi --exec
// Mono's implementation is incomplete, so built from Microsoft's open source.
// Build steps:
//   git clone https://github.com/Microsoft/msbuild.git
//   git checkout xplat
//   ./build.pl
// Must reference all the custom assemblies or will fall back to GAC version.
#r "../msbuild/bin/Unix/Debug-MONO/Microsoft.Build.dll"
#r "../msbuild/bin/Unix/Debug-MONO/Microsoft.Build.Framework.dll"
#r "../msbuild/bin/Unix/Debug-MONO/Microsoft.Build.Tasks.Core.dll"
#r "../msbuild/bin/Unix/Debug-MONO/Microsoft.Build.Utilities.Core.dll"

open Microsoft.Build.Evaluation
open Microsoft.Build.Framework

// Testy test
let sample = "Sample/Sample.fsproj"

let proj = Project(sample)

proj.AddItem("Compile", "Test.fs")

proj.Save()

// TODO - rest of the damn project
