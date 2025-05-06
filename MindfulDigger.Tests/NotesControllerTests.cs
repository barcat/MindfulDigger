using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace MindfulDigger.Tests
{
    [TestFixture]
    public class NotesControllerTests
    {
        private WebApplicationFactory<Program> _factory;

        [SetUp]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Program>();
        }

        // TODO: Add test methods here

        [TearDown]
        public void Teardown()
        {
            _factory.Dispose();
        }
    }
}
