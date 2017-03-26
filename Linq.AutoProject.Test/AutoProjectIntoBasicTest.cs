using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Linq.AutoProject;
using Assert = Xunit.Assert;

namespace Linq.AutoProject.Test
{
    [TestClass]
    public class AutoProjectIntoBasicTest
    {
        public class TestType
        {
            public int Foo { get; set; }
            public int Bar { get; set; }
        }

        [TestMethod]
        public void AutoProjectInto_ThrowsIfActivateAutoProjectsWasNotCalled()
        {
            var subject = Enumerable.Range(1, 1).AsQueryable()
                                .Select(x => new object())
                                .Select(x => x.AutoProjectInto(() => new object { }));

            Assert.Throws<NotImplementedException>(() =>
            {
                subject.ToArray();
            });

            subject.ActivateAutoProjects().ToArray();
        }

        [TestMethod]
        public void AutoProjectInto_CanWorkWithoutPropBindingSection()
        {
            var subject = Enumerable.Range(1, 1).AsQueryable()
                                .Select(x => new object())
                                .Select(x => x.AutoProjectInto(() => new object()));

            subject.ActivateAutoProjects().ToArray();
        }

        [TestMethod]
        public void AutoProjectInto_ActivationDoesNothingIfMarkerMethodNotFound()
        {
            var subject = Enumerable.Range(1, 1).AsQueryable()
                                .Select(x => new object())
                                .Select(x =>  new object());

            var transformedSubject = subject.ActivateAutoProjects();

            Assert.True(subject == transformedSubject);
            transformedSubject.ToArray();
        }

        [TestMethod]
        public void AutoProjectInto_ProjectsUntouchedProperties()
        {
            var subject = Enumerable.Range(1, 1).AsQueryable()
                               .Select(x => new TestType()
                               {
                                    Foo = 7,
                                    Bar = 9
                               })
                               .Select(x => x.AutoProjectInto(() => new TestType()));

            var result = subject.ActivateAutoProjects().ToArray().Single();

            Assert.Equal(result.Foo, 7);
            Assert.Equal(result.Bar, 9);
        }

        [TestMethod]
        public void AutoProjectInto_DoesNotProjectsProperties()
        {
            var subject = Enumerable.Range(1, 1).AsQueryable()
                               .Select(x => new TestType()
                               {
                                   Foo = 7,
                                   Bar = 9
                               })
                               .Select(x => x.AutoProjectInto(() => new TestType()
                               {
                                   Bar = 10
                               }));

            var result = subject.ActivateAutoProjects().ToArray().Single();

            Assert.Equal(result.Foo, 7);
            Assert.Equal(result.Bar, 10);
        }

        [TestMethod]
        public void AutoProjectInto_ActivatesMultipleProjections()
        {
            var subject = Enumerable.Range(1, 1).AsQueryable()
                                .Select(x => new TestType()
                                {
                                    Foo = 7,
                                    Bar = 9
                                })
                                .Select(x => x.AutoProjectInto(() => new TestType()
                                {
                                    Bar = 10
                                }))
                                .Select(x => x.AutoProjectInto(() => new TestType()
                                {
                                    Bar = 11
                                }))
                                .Select(x => x.AutoProjectInto(() => new TestType()
                                {
                                    Bar = 12
                                }));

            var result = subject.ActivateAutoProjects().ToArray().Single();

            Assert.Equal(result.Foo, 7);
            Assert.Equal(result.Bar, 12);
        }

        [TestMethod]
        public void AutoProjectInto_CanBeUsedWithLinqQuery()
        {
            var subject = from source in Enumerable.Range(1, 1).AsQueryable()
                               .Select(x => new TestType()
                               {
                                   Foo = 7,
                                   Bar = 9
                               })
                          select source.AutoProjectInto(() => new TestType());

            var result = subject.ActivateAutoProjects().ToArray().Single();

            Assert.Equal(result.Foo, 7);
            Assert.Equal(result.Bar, 9);
        }
    }
}
