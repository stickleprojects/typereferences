

using System;
using NExpect;
using NUnit.Framework;
using TypeReferences.Core;
using static NExpect.Expectations;

namespace TypeReferencesd.Core.Tests
{
    [TestFixture]

    public class ProjectAnalyzerTests
    {
        [Test]
        public void can_read_project()
        {
            var filepath = @"D:\code\firefly\Northwind2\Northwind.sln";

            var svc = getService();
            Expect(svc.ReadSLN(filepath)).To.Not.Be.Null();

        }

        private ProjectAnalyzer getService()
        {
            return new ProjectAnalyzer();

        }
    }
}
