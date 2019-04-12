using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TypeReferences.Core
{
    /// <summary>
    /// Analyses project to get list of all types defined and 
    /// </summary>
    public class ProjectAnalyzer
    {
        public object ReadSLN (string filepath)
        {
            // Microsoft.CodeAnalysis.Workspace.

            var result = AnalyseSolution(filepath).Result;

            return true;
        }
        private async Task< bool> AnalyseSolution(string solutionUrl)
        {
            bool success = true;
            
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            Solution solution = workspace.OpenSolutionAsync(solutionUrl).Result;

            var results = await Task.WhenAll( solution.Projects.Select(async p => await AnalyseProject(p)));
            return success;
        }

        private async Task<object> AnalyseProject(Project p)
        {
            return await Task.WhenAll(p.Documents.Select(async d => await AnalyseDocument(d)));
                }

        private async Task<object> AnalyseDocument(Document d)
        {
            var root = await d.GetSyntaxRootAsync();

            var types = root.DescendantNodes().OfType<TypeDeclarationSyntax>().ToList();

            var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

            return types.Select(t => t.Identifier.Text).ToArray();
        }
    }
}

