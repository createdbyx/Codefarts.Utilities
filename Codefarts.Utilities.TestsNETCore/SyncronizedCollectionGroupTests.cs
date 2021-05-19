// <copyright file="SyncronizedCollectionGroupTests.cs" company="Codefarts">
// Copyright (c) Codefarts
// contact@codefarts.com
// http://www.codefarts.com
// </copyright>

using System.Collections.ObjectModel;
using System.Security;

namespace Codefarts.Utilities.Tests
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SyncronizedCollectionGroupTests
    {

        [TestInitialize]
        public void InitTest()
        {

        }

        [TestCleanup]
        public void TestCleanup()
        {

        }

        [TestMethod]
        public void Add()
        {
            var src1 = new List<string>();
            var src2 = new List<string>();
            var col1 = new SyncronizedCollection<string>(src1);
            var col2 = new SyncronizedCollection<string>(src2);

            var grp = new SyncronizedCollectionGroup<string>(new[] { col1, col2 });

            grp.Add("One");
            Assert.AreEqual(1, src1.Count);
            Assert.AreEqual(1, src2.Count);
            Assert.AreEqual(1, col1.Count);
            Assert.AreEqual(1, col2.Count);
            Assert.AreEqual(1, grp.Count);
        }

        [TestMethod]
        public void Remove()
        {
            var src1 = new List<string>();
            var src2 = new List<string>();
            var col1 = new SyncronizedCollection<string>(src1);
            var col2 = new SyncronizedCollection<string>(src2);

            var grp = new SyncronizedCollectionGroup<string>(new[] { col1, col2 });

            grp.Add("One");
            grp.Add("Two");
            grp.Add("Three");
            grp.Remove("Two");

            Assert.AreEqual(2, src1.Count);
            Assert.AreEqual(2, src2.Count);
            Assert.AreEqual(2, col1.Count);
            Assert.AreEqual(2, col2.Count);
            Assert.AreEqual(2, grp.Count);
        }

        [TestMethod]
        public void Clear()
        {
            var src1 = new List<string>();
            var src2 = new List<string>();
            var col1 = new SyncronizedCollection<string>(src1);
            var col2 = new SyncronizedCollection<string>(src2);

            var grp = new SyncronizedCollectionGroup<string>(new[] { col1, col2 });

            grp.Add("One");
            grp.Add("Two");
            grp.Add("Three");
            grp.Clear();

            Assert.AreEqual(0, src1.Count);
            Assert.AreEqual(0, src2.Count);
            Assert.AreEqual(0, col1.Count);
            Assert.AreEqual(0, col2.Count);
            Assert.AreEqual(0, grp.Count);
        }

        [TestMethod]
        public void AddToSource()
        {
            var src1 = new List<string>();
            var src2 = new List<string>();
            var col1 = new SyncronizedCollection<string>(src1);
            var col2 = new SyncronizedCollection<string>(src2);

            var grp = new SyncronizedCollectionGroup<string>(new[] { col1, col2 });

            src1.Add("One");

            Assert.AreEqual(1, src1.Count);
            Assert.AreEqual(0, src2.Count);
            Assert.AreEqual(1, col1.Count);
            Assert.AreEqual(0, col2.Count);
            Assert.ThrowsException<CollectionsOutOfSyncException>(() =>
            {
                Assert.AreEqual(0, grp.Count);
            });
        }

        [TestMethod]
        public void IsOutOfSync()
        {
            var src1 = new List<string>();
            var src2 = new List<string>();
            var col1 = new SyncronizedCollection<string>(src1);
            var col2 = new SyncronizedCollection<string>(src2);

            var grp = new SyncronizedCollectionGroup<string>(new[] { col1, col2 });

            src1.Add("One");

            Assert.AreEqual(1, src1.Count);
            Assert.AreEqual(0, src2.Count);
            Assert.AreEqual(1, col1.Count);
            Assert.AreEqual(0, col2.Count);
            Assert.IsTrue(grp.IsOutOfSync());
        }

        [TestMethod]
        public void AddAndRemoveFromSource_OneSource()
        {
            var src1 = new List<string>();
            var col1 = new SyncronizedCollection<string>(src1);

            var grp = new SyncronizedCollectionGroup<string>(new[] { col1 });

            src1.Add("One");
            src1.Add("Two");
            src1.Add("Three");
            src1.Remove("Two");

            Assert.AreEqual(2, src1.Count);
            Assert.AreEqual(2, col1.Count);
            Assert.IsFalse(grp.IsOutOfSync());
        }

        [TestMethod]
        public void AddAndRemoveFromSource_TwoSources()
        {
            var src1 = new ObservableCollection<string>();
            var src2 = new ObservableCollection<string>();
            var col1 = new SyncronizedCollection<string>(src1);
            var col2 = new SyncronizedCollection<string>(src1);

            var grp = new SyncronizedCollectionGroup<string>(new[] { col1, col2 });

            src1.Add("One");
            src1.Add("Two");
            src2.Add("Three");
            src1.Remove("Two");

            Assert.AreEqual(1, src1.Count);
            Assert.AreEqual(1, src2.Count);
            Assert.AreEqual(1, col1.Count);
            Assert.AreEqual(1, col2.Count);
            Assert.IsFalse(grp.IsOutOfSync());
            Assert.AreEqual(1, grp.Count);
        }

        [TestMethod]
        public void AddToCollectionButClearSource()
        {
            var src = new List<string>();
            var col = new SyncronizedCollection<string>(src);
            var grp = new SyncronizedCollectionGroup<string>(new[] { col });

            col.Add("One");
            src.Clear();
            Assert.AreEqual(0, col.Count);
            Assert.AreEqual(0, src.Count);
            Assert.AreEqual(0, grp.Count);
        }

        [TestMethod]
        public void AddToSourceAndClear()
        {
            var src = new List<string>();
            var col = new SyncronizedCollection<string>(src);
            var grp = new SyncronizedCollectionGroup<string>(new[] { col });

            src.Add("One");
            src.Clear();
            Assert.AreEqual(0, col.Count);
            Assert.AreEqual(0, src.Count);
            Assert.AreEqual(0, grp.Count);
        }
    }
}