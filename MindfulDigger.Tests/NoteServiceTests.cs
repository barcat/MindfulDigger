using NUnit.Framework;
using MindfulDigger.Services;
using NSubstitute;

namespace MindfulDigger.Tests
{
    [TestFixture]
    public class NoteServiceTests
    {
        private INoteService? _noteService; // Changed to nullable

        [SetUp]
        public void Setup()
        {
            _noteService = Substitute.For<INoteService>(); // Initialize _noteService
        }

        // TODO: Add test methods here
    }
}
