using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.Client.Document;
using Raven.Abstractions.Exceptions;
namespace RavenDBOptimisticConcurencyTest
{
    public class Item
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }


    [TestClass]
    public class OptimisticConcurencyTestBug
    {
        [TestMethod]
        [ExpectedException(typeof(ConcurrencyException))]
        public void CreateNewDocumentAnd_AndMakeChanges()
        {
            using (var docStore = new DocumentStore { Url = "http://localhost:8080", DefaultDatabase = "OptimisticConcurencyTestDB" })
            {
                docStore.Initialize();

                string id = null;

                using (var session = docStore.OpenSession())
                {
                    var item = new Item { Name = "ItemInInitialState" };
                    session.Store(item);
                    session.SaveChanges();
                    id = item.Id;
                }

                using (var sessionFirst = docStore.OpenSession())
                {
                    using (var sessionSecond = docStore.OpenSession())
                    {
                        sessionSecond.Advanced.UseOptimisticConcurrency = true;

                        var itemFirst = sessionFirst.Load<Item>(id);
                        var itemSecond = sessionSecond.Load<Item>(id);

                        itemFirst.Name = "ChangedByItemFirst";
                        sessionFirst.SaveChanges();

                        itemSecond.Name = "ChangedbyItemSecond";
                        sessionSecond.SaveChanges();  // this line should throw if optimistic concurency is enabled.
                    }
                }
            }
        }



        [TestMethod]
        [ExpectedException(typeof(ConcurrencyException))]
        public void LoadExistingDocument_AndMakeChanges()
        {
            using (var docStore = new DocumentStore { Url = "http://localhost:8080", DefaultDatabase = "OptimisticConcurencyTestDB" })
            {
                docStore.Initialize();

                string id = "items/33";

                using (var sessionFirst = docStore.OpenSession())
                {
                    using (var sessionSecond = docStore.OpenSession())
                    {
                        sessionSecond.Advanced.UseOptimisticConcurrency = true;

                        var itemFirst = sessionFirst.Load<Item>(id);
                        var itemSecond = sessionSecond.Load<Item>(id);

                        itemFirst.Name = "ChangedByItemFirst";
                        sessionFirst.SaveChanges();

                        itemSecond.Name = "ChangedbyItemSecond";
                        sessionSecond.SaveChanges();  // this line should throw if optimistic concurency is enabled.
                    }
                }
            }
        }
    }
}
