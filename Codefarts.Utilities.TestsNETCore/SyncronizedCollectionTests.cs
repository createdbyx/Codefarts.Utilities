// <copyright file="SyncronizedCollectionTests.cs" company="Codefarts">
// Copyright (c) Codefarts
// contact@codefarts.com
// http://www.codefarts.com
// </copyright>

namespace Codefarts.Utilities.Tests
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SyncronizedCollectionTests
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
            var src = new List<string>();
            var col = new SyncronizedCollection<string>(src);

            col.Add("One");
            Assert.AreEqual(1, col.Count);
            Assert.AreEqual(1, src.Count);
        }

        [TestMethod]
        public void Remove()
        {
            var src = new List<string>();
            var col = new SyncronizedCollection<string>(src);

            col.Add("One");
            col.Add("Two");
            col.Add("Three");
            col.Remove("Two");

            Assert.AreEqual(2, col.Count);
            Assert.AreEqual(2, src.Count);
        }

        [TestMethod]
        public void Clear()
        {
            var src = new List<string>();
            var col = new SyncronizedCollection<string>(src);

            col.Add("One");
            col.Clear();
            Assert.AreEqual(0, col.Count);
            Assert.AreEqual(0, src.Count);
        }

        [TestMethod]
        public void AddToSource()
        {
            var src = new List<string>();
            var col = new SyncronizedCollection<string>(src);

            src.Add("One");
            Assert.AreEqual(1, col.Count);
            Assert.AreEqual(1, src.Count);
        }

        [TestMethod]
        public void AddAndRemoveFromSource()
        {
            var src = new List<string>();
            var col = new SyncronizedCollection<string>(src);

            src.Add("One");
            src.Add("Two");
            src.Add("Three");
            src.Remove("Two");

            Assert.AreEqual(2, col.Count);
            Assert.AreEqual(2, src.Count);
        }

        [TestMethod]
        public void AddToCollectionButClearSource()
        {
            var src = new List<string>();
            var col = new SyncronizedCollection<string>(src);

            col.Add("One");
            src.Clear();
            Assert.AreEqual(0, col.Count);
            Assert.AreEqual(0, src.Count);
        }

        [TestMethod]
        public void AddToSourceAndClear()
        {
            var src = new List<string>();
            var col = new SyncronizedCollection<string>(src);

            src.Add("One");
            src.Clear();
            Assert.AreEqual(0, col.Count);
            Assert.AreEqual(0, src.Count);
        }
    }
}
