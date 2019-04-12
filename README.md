# typereferences

(see also ILStrip (https://github.com/BrokenEvent/ILStrip),that was based on Cecil (mono lib))

Currently uses Roslyn to analyse the hard-coded solution "D:\code\firefly\Northwind2\Northwind.sln"

# to run

Nothing to run just yet, only tests

# to build

visual studio 2017 or 2019 to build the sln
note DO NOT UPDATE nuget refs, as you will get an error with composition packages (roslyn needs specific version of Composition)

# tests

TEsts are in the TypedReferencesd.Core.Tests\ProjectAnalyzerTests.cs file
