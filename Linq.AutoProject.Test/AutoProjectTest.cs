using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assert = Xunit.Assert;

namespace Linq.AutoProject.Test
{
    [TestClass]
    public class AutoProjectTest
    {
        [TestMethod]
        public void AutoProject_Works()
        {
            var subject = Enumerable.Range(1, 1).AsQueryable()
                   .Select(x => new TestType()
                   {
                       Foo = 7,
                       Bar = 9
                   })
                   .AutoProject(x => new TestType()
                   {
                       Bar = 10
                   });

            var result = subject.Single();

            Assert.Equal(result.Foo, 7);
            Assert.Equal(result.Bar, 10);
        }


        [TestMethod]
        public void AutoProject_CanWorkWithoutPropBindingSection()
        {
            var subject = Enumerable.Range(1, 1).AsQueryable()
                                .Select(x => new object())
                                .AutoProject(x => new object());

            subject.ToArray();
        }

        [TestMethod]
        public void AutoProject_OnlyProjectsMatchingTypes()
        {
            var subject = Enumerable.Range(1, 1).AsQueryable()
                                .Select(x => new PropTypeMismatchSource()
                                {
                                    Foo = 1
                                })
                                .AutoProject(x => new PropTypeMismatchTarget());

            var result = subject.ToArray().Single();

            Assert.Equal(result.Foo, null);
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
                                .AutoProject(x => new GetterSetterMismatchTarget());

            var result = subject.ToArray().Single();

            Assert.Equal(result.Projected, 5);
            Assert.Equal(result.IgnoredPrivateSetInTarget, 0);
            Assert.Equal(result.IgnoredPrivateGetInSource, 0);
        }

        [TestMethod]
        public void AutoProject_PreservesComplexBindings()
        {
            var subject = Enumerable.Range(1, 1).AsQueryable()
                              .Select(x => new object())
                              .AutoProject(x => new CompleBindingsPreservation()
                              {
                                  Foo = Enumerable.Range(11, 5).AsQueryable().Max()
                              });

            var result = subject.ToArray().Single();

            Assert.Equal(result.Foo, 15);
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
                                .AutoProject(x => new ConstructorParamsPreservationTarget(5));

            var result = subject.ToArray().Single();

            Assert.Equal(result.Foo, 5);
            Assert.Equal(result.Bar, 6);
            Assert.Equal(result.Baz, 7);

            subject = Enumerable.Range(1, 1).AsQueryable()
                    .Select(x => new ConstructorParamsPreservationSource()
                    {
                        Bar = 6,
                        Baz = 7
                    })
                    .AutoProject(x => new ConstructorParamsPreservationTarget(5)
                    {
                        Bar = 10
                    });

            result = subject.ToArray().Single();
            Assert.Equal(result.Foo, 5);
            Assert.Equal(result.Bar, 10);
            Assert.Equal(result.Baz, 7);
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
                                .AutoProject(x => new ComplexTypeProjectionTarget());

            var result = subject.ToArray().Single();

            Assert.NotNull(result.Token);
            Assert.Equal(result.Token.Foo, 5);
        }

        [TestMethod]
        public void AutoProject_ProjectsUntouchedProperties()
        {
            var subject = Enumerable.Range(1, 1).AsQueryable()
                               .Select(x => new TestType()
                               {
                                   Foo = 7,
                                   Bar = 9
                               })
                               .AutoProject(x => new TestType());

            var result = subject.ToArray().Single();

            Assert.Equal(result.Foo, 7);
            Assert.Equal(result.Bar, 9);
        }

        [TestMethod]
        public void AutoProject_DoesNotInterfiereWithAutoProjectInto()
        {
            var subject = Enumerable.Range(1, 1).AsQueryable()
                               .Select(x => new TestType()
                               {
                                   Foo = 7,
                                   Bar = 9
                               })
                               .AutoProject(x => new TestType());

            var result = subject.ActivateAutoProjects().ToArray().Single();

            Assert.Equal(result.Foo, 7);
            Assert.Equal(result.Bar, 9);
        }

        [TestMethod]
        public void AutoProject_CanBeCombinedWithAutoProjectInto()
        {
            var subject = Enumerable.Range(1, 1).AsQueryable()
                               .Select(x => new TestType()
                               {
                                   Foo = 7,
                                   Bar = 9
                               })
                               .AutoProject(x => new TestType())
                               .Select(x => x.AutoProjectInto(() => new TestType()))
                               .AutoProject(x => new TestType())
                               .Select(x => x.AutoProjectInto(() => new TestType()));

            var result = subject.ActivateAutoProjects().ToArray().Single();

            Assert.Equal(result.Foo, 7);
            Assert.Equal(result.Bar, 9);
        }

        public class TestType
        {
            public int Foo { get; set; }
            public int Bar { get; set; }
        }

        public class PropTypeMismatchSource
        {
            public int Foo { get; set; }
        }

        public class PropTypeMismatchTarget
        {
            public string Foo { get; set; }
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

        public class CompleBindingsPreservation
        {
            public int Foo { get; set; }
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

        public class ComplexTypeProjectionSource
        {
            public ComplexTypeProjectionToken Token { get; set; }
        }

        public class ComplexTypeProjectionTarget
        {
            public ComplexTypeProjectionToken Token { get; set; }
        }

        public class ComplexTypeProjectionToken
        {
            public int Foo { get; set; }
        }
    }
}
