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
using Microsoft.CodeAnalysis.FindSymbols;
using System.Diagnostics;
using Morts.SpecificationPattern;
using System.Linq.Expressions;

namespace TypeReferences.Core
{
    /// <summary>
    /// Analyses project to get list of all types defined and 
    /// </summary>
    public class ProjectAnalyzer
    {
        public object ReadSLN(string filepath)
        {
            // Microsoft.CodeAnalysis.Workspace.

            var result = AnalyseSolution(filepath).Result;

            return true;
        }
        private async Task<bool> AnalyseSolution(string solutionUrl)
        {
            bool success = true;

            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            
            var solution =await  workspace.OpenSolutionAsync(solutionUrl);
            
           
            var results = await Task.WhenAll(solution.Projects.Select(async p => await AnalyseProject(solution, p)));
            foreach(var tr in results)
            {
                foreach (var r in (object[])tr)
                {
                    Debug.WriteLine(r);
                }
            }
            return success;
        }

        private async Task<object> AnalyseProject(Solution sln, Project p)
        {
            return await Task.WhenAll(p.Documents.Where(this.ShouldAnalyzeDocument).Select(async d => await AnalyseDocument(sln, d)));
        }

        class Prg : IFindReferencesProgress
        {
            public void OnCompleted()
            {
                throw new NotImplementedException();
            }

            public void OnDefinitionFound(ISymbol symbol)
            {
                throw new NotImplementedException();
            }

            public void OnFindInDocumentCompleted(Document document)
            {
                throw new NotImplementedException();
            }

            public void OnFindInDocumentStarted(Document document)
            {
                throw new NotImplementedException();
            }

            public void OnReferenceFound(ISymbol symbol, ReferenceLocation location)
            {
                throw new NotImplementedException();
            }

            public void OnStarted()
            {
                throw new NotImplementedException();
            }

            public void ReportProgress(int current, int maximum)
            {
                throw new NotImplementedException();
            }
        }
        private async Task<object> AnalyseDocument(Solution sln, Document d)
        {
            var root = await d.GetSyntaxRootAsync();
            var model = await d.GetSemanticModelAsync();

            return await AnalyzeModel(sln,d, root, model);
        }
        public Func<TypeDeclarationSyntax, bool > ShouldAnalyzeType = new Func<TypeDeclarationSyntax, bool >(tds =>
        {
            // ignore designers
            var bl = tds.BaseList;
          
            if (bl != null && bl.Types != null)
            {
                foreach (var b in bl.Types)
                {
                    if (b.Parent?.ToString().Contains("ApplicationSettingsBase") == true) return false;

                    

                }
            }
            return true;
        });
        private Func<Document, bool> ShouldAnalyzeDocument = new Func<Document, bool>(d => !d.Name.Contains(".Designer.cs"));

        private async Task<string> AnalyzeModel(Solution sln, Document d, SyntaxNode root, SemanticModel model)
        {
            var types = root.DescendantNodes().OfType<TypeDeclarationSyntax>().Where(this.ShouldAnalyzeType).ToList();
            var ret = new StringBuilder();

            // ignore designers

            var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

           ret.AppendLine("Analyzing document " + d.Name);

            // find refs to types
            foreach (var t in types)
            {
                var symbol = model.GetDeclaredSymbol(t);

                var refs = SymbolFinder.FindReferencesAsync(symbol, sln).Result;
                

                var txt = await AnalyzeRefs(symbol, t, refs);
                ret.AppendLine(txt);
            }
            return await Task.FromResult(ret.ToString());

            //return types.Select(t => t.Identifier.Text).ToArray();
        }

        private async Task<string> AnalyzeRefs(ISymbol symbol, TypeDeclarationSyntax t, IEnumerable<ReferencedSymbol> refs)
        {
            // Dump all refs
            return await DumpRefs(symbol, refs);


            // now analyze the refs to ignore self-refs
            //var isSelfRef =
            //    new IsNotSelfRef(symbol);
            //var isInType = new IsNotDeclaredInThisType(symbol);
            //var validRefs = refs.Where(isSelfRef.And(isInType) ).ToList();

            //if (!validRefs.Any())
            //{
            //    Debug.WriteLine("No external refs found for type " + symbol.ToString());
            //} else
            //{
            //    Debug.WriteLine("Found some refs " + string.Join(",", validRefs.Select(x => x.Locations.First().Location.ToString())));
            //}
           // throw new NotImplementedException();


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="referencedSymbol">The place within the declaringType that is being refd</param>
        /// <returns></returns>
        private  string DumpToString(ISymbol declaringType, ReferencedSymbol referencedSymbol)
        {
            //if(IsConstructor(referencedSymbol.Definition ) )
            //{
            //    if(AreEqual(declaringType, referencedSymbol.Definition ))
            //    {
            //        return "Its own ctor is being referenced!";
            //    } 
            //}
            
            
            if (!referencedSymbol.Locations.Any())
            {
                return "NONE";
            }

           
            var ret = new List<string>();

            foreach(var l in referencedSymbol.Locations)
            {
                var ls = l.Location.GetMappedLineSpan();
                ret.Add($"{l.Document.Name} ({ls.StartLinePosition}-{ ls.EndLinePosition})");
            }
            return string.Join("\n,", ret) ;

        }

        private bool IsConstructor(ISymbol definition)
        {
            var m = definition as IMethodSymbol;
            if(m!=null)
            {
                return m.MethodKind == MethodKind.Constructor;
            }
            return false;

        }

        private async Task<string> DumpRefs(ISymbol symbol, IEnumerable<ReferencedSymbol> refs)
        {
            var ret = new StringBuilder();
            ret.AppendLine(symbol.ToString() + " {");
            foreach (var r in refs) {
               
                ret.AppendLine("   " + symbol.ToString() + " ->" + DumpToString(symbol, r));
                    }
            ret.AppendLine("}");

            return await Task.FromResult(ret.ToString());
        }
        public Func<ISymbol, ISymbol, bool> AreEqual = new Func<ISymbol, ISymbol, bool>((a, b) => a.ToString() == b.ToString() || a.ToString() == b.ContainingType.ToString());

    }

    public class IsNotDeclaredInThisType : IsNotSelfRef
    {
        public IsNotDeclaredInThisType(ISymbol tgt) : base(tgt)
        {
        }

        protected override Expression<Func<ReferencedSymbol, bool>> CreateExpression()
        {
            return r => (r.Definition.ContainingType != null && r.Definition.ContainingType.ToString() != this.tgt.ToString());
        }
    }
    public class IsNotSelfRef : Specification<ReferencedSymbol>
    {
        protected readonly ISymbol tgt;

        

        public IsNotSelfRef(ISymbol tgt)
        {
            this.tgt = tgt;
        }

        protected override Expression<Func<ReferencedSymbol, bool>> CreateExpression()
        {
            return r =>

                 r.Definition.ToString() != this.tgt.ToString()
                 
            ;
        }
    }
}

