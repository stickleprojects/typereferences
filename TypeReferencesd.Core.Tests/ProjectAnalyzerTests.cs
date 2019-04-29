

using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NExpect;
using NUnit.Framework;
using TypeReferences.Core;
using static NExpect.Expectations;

namespace TypeReferencesd.Core.Tests
{
    [TestFixture]

    public class ProjectAnalyzerTests
    {
        private string GetTestSLNPath(string filename)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory,@"..\..\..\TestSLNs\",filename);
        }
        [Test]
        public void can_read_full_sln()
        {
            var filepath = GetTestSLNPath(@"FormWithController\FormWithController.sln");

            var svc = getService();
            Expect(svc.ReadSLN(filepath)).To.Not.Be.Null();
        }
        //[Test]
        //public void can_read_project()
        //{
        //    var filepath = @"D:\code\firefly\Northwind2\Northwind.sln";

        //    var svc = getService();
        //    Expect(svc.ReadSLN(filepath)).To.Not.Be.Null();

        //}

        private ProjectAnalyzer getService()
        {
            return new ProjectAnalyzer();

        }
    }
}
