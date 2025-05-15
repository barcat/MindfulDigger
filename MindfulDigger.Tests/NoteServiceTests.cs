using MindfulDigger.Services;
using MindfulDigger.DTOs;
using MindfulDigger.Model;
using NSubstitute;
using Microsoft.Extensions.Logging;
using MindfulDigger.Data.Supabase;

namespace MindfulDigger.Tests
{
    [TestFixture]
    public class NoteServiceTests
    {
        private INoteService? _noteService;

        [SetUp]
        public void Setup()
        {
            _noteService = Substitute.For<INoteService>(); 
        }

        [Test]
        public async Task CreateNoteAsync_WhenUserBelowLimit_ReturnsCreateNoteResponse()
        {
            // Arrange
            var noteRepository = Substitute.For<INoteRepository>();
            var logger = Substitute.For<ILogger<NoteService>>();
            var noteService = new NoteService(noteRepository, logger);

            var userId = Guid.NewGuid();
            var jwt = "jwt";
            var refreshToken = "refresh";
            var request = new CreateNoteRequest { Content = "Test content" };
            var createdNote = new Note
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Content = request.Content,
                CreationDate = DateTime.UtcNow
            };

            noteRepository.GetUserNotesCountAsync(userId, jwt, refreshToken).Returns(0);
            noteRepository.InsertNoteAsync(request, userId, jwt, refreshToken).Returns(createdNote);

            // Act
            var result = await noteService.CreateNoteAsync(request, userId, jwt, refreshToken);

            // Asserts
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(createdNote.Id));
            Assert.That(result.UserId, Is.EqualTo(userId));
            Assert.That(result.Content, Is.EqualTo(request.Content));
            Assert.That(result.ContentSnippet, Is.EqualTo(request.Content));
        }
    }
}
