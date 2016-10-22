using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Linq.AutoProject;
using Assert = Xunit.Assert;

namespace Linq.AutoProject.Test
{
    [TestClass]
    public class AutoProjectAdvancedTest
    {
        public class PropTypeMismatchSource
        {
            public int Foo { get; set; }
        }

        public class PropTypeMismatchTarget
        {
            public string Foo { get; set; }
        }

        [TestMethod]
        public void AutoProject_OnlyProjectsMatchingTypes()
        {
            var subject = Enumerable.Range(1, 1).AsQueryable()
                                .Select(x => new PropTypeMismatchSource()
                                {
                                    Foo = 1
                                })
                                .Select(x => x.AutoProjectInto(() => new PropTypeMismatchTarget()));

            var result = subject.ActivateAutoProjects().ToArray().Single();

            Assert.Equal(result.Foo, null);
        }

        public class GetterSetterMismatchSource
        {
            public int Projected { get; set; }
            public int IgnoredPrivateSetInTarget { get; set; }
            public int IgnoredPrivateGetInSource { private get; set; }

        }

        public class GetterSetterMismatchTarget
        {
            public int Projected { get; set; }
            public int IgnoredPrivateSetInTarget { get; private set; }
            public int IgnoredPrivateGetInSource { get; set; }
        }

        [TestMethod]
        public void AutoProject_OnlyProjectsWhenExitSourceGetterAndTargetSetter()
        {
            var subject = Enumerable.Range(1, 1).AsQueryable()
                                .Select(x => new GetterSetterMismatchSource()
                                {
                                    Projected = 5,
                                    IgnoredPrivateSetInTarget = 6,
                                    IgnoredPrivateGetInSource = 7
                                })
                                .Select(x => x.AutoProjectInto(() => new GetterSetterMismatchTarget()));
 
            var result = subject.ActivateAutoProjects().ToArray().Single();

            Assert.Equal(result.Projected, 5);
            Assert.Equal(result.IgnoredPrivateSetInTarget, 0);
            Assert.Equal(result.IgnoredPrivateGetInSource, 0);
        }

        public class ConstructorParamsPreservationSource
        {
            public int Bar { get; set; }
            public int Baz { get; set; }
        }

        public class ConstructorParamsPreservationTarget
        {
            public ConstructorParamsPreservationTarget(int foo)
            {
                Foo = foo;
            }

            public int Foo { get; set; }
            public int Bar { get; set; }
            public int Baz { get; set; }
        }

        [TestMethod]
        public void AutoProject_PreservesContstructorInvocation()
        {
            var subject = Enumerable.Range(1, 1).AsQueryable()
                                .Select(x => new ConstructorParamsPreservationSource()
                                {
                                    Bar = 6,
                                    Baz = 7
                                })
                                .Select(x => x.AutoProjectInto(() => new ConstructorParamsPreservationTarget(5)));

            var result = subject.ActivateAutoProjects().ToArray().Single();

            Assert.Equal(result.Foo, 5);
            Assert.Equal(result.Bar, 6);
            Assert.Equal(result.Baz, 7);

            subject = Enumerable.Range(1, 1).AsQueryable()
                    .Select(x => new ConstructorParamsPreservationSource()
                    {
                        Bar = 6,
                        Baz = 7
                    })
                    .Select(x => x.AutoProjectInto(() => new ConstructorParamsPreservationTarget(5)
                    {
                        Bar = 10
                    }));

            result = subject.ActivateAutoProjects().ToArray().Single();
            Assert.Equal(result.Foo, 5);
            Assert.Equal(result.Bar, 10);
            Assert.Equal(result.Baz, 7);
        }

        public class ComplexTypeProjectionToken
        {
            public int Foo { get; set; }
        }

        public class ComplexTypeProjectionSource
        {
            public ComplexTypeProjectionToken Token { get; set; }
        }

        public class ComplexTypeProjectionTarget
        {
            public ComplexTypeProjectionToken Token { get; set; }
        }

        [TestMethod]
        public void AutoProject_ProjectsComplexTypeProperties()
        {
            var subject = Enumerable.Range(1, 1).AsQueryable()
                                .Select(x => new ComplexTypeProjectionSource()
                                {
                                    Token = new ComplexTypeProjectionToken()
                                    {
                                        Foo = 5
                                    }
                                })
                                .Select(x => x.AutoProjectInto(() => new ComplexTypeProjectionTarget()));

            var result = subject.ActivateAutoProjects().ToArray().Single();

            Assert.NotNull(result.Token);
            Assert.Equal(result.Token.Foo, 5);
        }

        public class CompleBindingsPreservation
        {
            public int Foo { get; set; }
        }

        [TestMethod]
        public void AutoProject_PreservesComplexBindings()
        {
            var subject = Enumerable.Range(1, 1).AsQueryable()
                              .Select(x => new object())
                              .Select(x => x.AutoProjectInto(() => new CompleBindingsPreservation()
                              {
                                  Foo = Enumerable.Range(11, 5).AsQueryable().Max()
                              }));

            var result = subject.ActivateAutoProjects().ToArray().Single();

            Assert.Equal(result.Foo, 15);
        }

    }
}
