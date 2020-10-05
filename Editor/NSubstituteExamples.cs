using System;
using NSubstitute;
using NUnit.Framework;

namespace NSubstitute.Example
{
    public interface IDataBase
    {
        event Action OnDBReady;
        event Action OnDBFailed;

        void Connect();
        void AddEntry(int index, string entry);
        string GetEntry(int index);

        bool Contains(int index);
    }

    public class ClientsCatalog
    {
        private IDataBase database;

        public bool IsReady { get; private set; } = false;
        public bool Failed { get; private set; } = false;

        public ClientsCatalog(IDataBase database)
        {
            this.database = database;
            this.database.OnDBReady += () => IsReady = true;
            this.database.OnDBFailed += () => Failed = true;
        }

        public void Initialize() => database.Connect();
        public void AddClient(int index, string client) => database.AddEntry(index, client);
        public string GetClient(int index) => database.GetEntry(index);
        public bool HasClient(int index) => database.Contains(index);
    }

    public class ClientCatalogTest
    {
        //[Test]
        public void CatalogConnectsToDatabaseSuccessfully()
        {
            //Arrange
            IDataBase database = Substitute.For<IDataBase>();
            ClientsCatalog catalog = new ClientsCatalog(database);
            database.When(x => x.Connect()).Do(x => database.OnDBReady += Raise.Event<Action>());

            //Act
            catalog.Initialize();

            //Assert
            database.Received().Connect();
            Assert.IsTrue(catalog.IsReady);
            Assert.IsFalse(catalog.Failed);
        }

        //[Test]
        public void CatalogConnectsToDatabaseFailed()
        {
            //Arrange
            IDataBase database = Substitute.For<IDataBase>();
            ClientsCatalog catalog = new ClientsCatalog(database);
            database.When(x => x.Connect()).Do(x => database.OnDBFailed += Raise.Event<Action>());

            //Act
            catalog.Initialize();

            //Assert
            database.Received().Connect();
            Assert.IsTrue(catalog.Failed);
            Assert.IsFalse(catalog.IsReady);
        }

        //[Test]
        public void CatalogRetrievesData()
        {
            //Arrange
            IDataBase database = Substitute.For<IDataBase>();
            ClientsCatalog catalog = new ClientsCatalog(database);
            catalog.Initialize();
            database.GetEntry(0).Returns("client0");
            database.GetEntry(1).Returns("client1");
            database.GetEntry(2).Returns("client2");

            //Act
            var client0 = catalog.GetClient(0);
            var client1 = catalog.GetClient(1);
            var client2 = catalog.GetClient(2);

            //Assert
            database.Received().GetEntry(0);
            Assert.AreEqual("client0", client0);
            database.Received().GetEntry(1);
            Assert.AreEqual("client1", client1);
            database.Received().GetEntry(2);
            Assert.AreEqual("client2", client2);
        }

        //[Test]
        public void CatalogReturnsHasClientProperly()
        {
            //Arrange
            IDataBase database = Substitute.For<IDataBase>();
            ClientsCatalog catalog = new ClientsCatalog(database);
            catalog.Initialize();
            database.Contains(0).Returns(true);
            database.Contains(1).Returns(false);
            database.Contains(2).Returns(true);

            //Act
            bool containsClient0 = catalog.HasClient(0);
            bool containsClient1 = catalog.HasClient(1);
            bool containsClient2 = catalog.HasClient(2);

            //Assert
            database.Received().Contains(0);
            Assert.IsTrue(containsClient0);
            database.Received().Contains(1);
            Assert.IsFalse(containsClient1);
            database.Received().Contains(2);
            Assert.IsTrue(containsClient2);
        }
    }
}