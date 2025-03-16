using AutoFixture.Xunit2;
using Bw.VaultBot.Application.Behaviors;
using Bw.VaultBot.Application.Options;
using Bw.VaultBot.Application.Requests;
using Bw.VaultBot.Data.Abstractions;
using Bw.VaultBot.Model;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;

namespace Bw.VaultBot.UnitTests.Behaviors;

public class AuthChatBehaviourTests
{
    private readonly Mock<IOptions<AdminOptions>> _optionsMock = new();
    private readonly Mock<IAdminChatRepository> _repositoryMock = new();
    private readonly Mock<ITelegramBotClient> _botMock = new();

    private readonly AuthChatBehaviour _behaviour;

    private readonly Mock<RequestHandlerDelegate<Unit>> _handlerMock = new();

    public AuthChatBehaviourTests()
    {
        _behaviour = new AuthChatBehaviour(
            _optionsMock.Object,
            _repositoryMock.Object,
            _botMock.Object,
            NullLogger<AuthChatBehaviour>.Instance);

        _handlerMock.Setup(next => next()).ReturnsAsync(Unit.Value);
    }

    [Theory, AutoData]
    public async Task AuthMessage_WhenChatIsNotAuthorized(Chat messageChat, string messageText)
    {
        _ = await _behaviour.Handle(
            new TelegramMessageRequest(new Message { Chat = messageChat, Text = messageText }),
            _handlerMock.Object,
            CancellationToken.None);

        _handlerMock.Verify(next => next(), Times.Never, "Next handler should not be called");
        _botMock.Verify(b =>
                b.SendRequest(
                    It.Is<SendMessageRequest>(r =>
                        r.ChatId == messageChat.Id
                        && r.Text == "I don't know you. Introduce yourself")
                    , It.IsAny<CancellationToken>())
            , Times.Once
            , "Message should be sent to the chat to demanding authorization");
        _repositoryMock
            .Verify(
                r => r.AddAdminChat(It.IsAny<AdminChat>())
                , Times.Never
                , "Admin chat should not be added");
    }

    [Theory, AutoData]
    public async Task AuthMessage_WhenChatIsAuthorized(Chat messageChat, AdminChat adminChat, string messageText)
    {
        _repositoryMock
            .Setup(r => r.GetAdminChatById(messageChat.Id))
            .ReturnsAsync(adminChat with { ChatId = messageChat.Id });

        _ = await _behaviour.Handle(
            new TelegramMessageRequest(new Message { Chat = messageChat, Text = messageText }),
            _handlerMock.Object,
            CancellationToken.None);

        _handlerMock.Verify(next => next(), Times.Once, "Next handler should be called");
        _botMock.Verify(b =>
                b.SendRequest(
                    It.Is<SendMessageRequest>(r => r.ChatId == messageChat.Id)
                    , It.IsAny<CancellationToken>())
            , Times.Never,
            "Message should not be sent to the chat");
        _repositoryMock
            .Verify(
                r => r.AddAdminChat(It.IsAny<AdminChat>())
                , Times.Never
                , "Admin chat should not be added");
    }

    private class ValidAdminContactData : TheoryData<Chat, Contact, AdminOptions>
    {
        public ValidAdminContactData()
        {
            var fixture = new Fixture();

            var chatId = fixture.Create<long>();
            var phoneNr = fixture.Create<string>();
            var username = fixture.Create<string>();

            var chatBuilder =
                fixture.Build<Chat>()
                    .With(c => c.Id, chatId)
                    .With(c => c.Username, username);

            var contactBuilder =
                fixture.Build<Contact>()
                    .With(c => c.UserId, chatId)
                    .With(c => c.PhoneNumber, phoneNr);

            var optionsBuilder =
                fixture.Build<AdminOptions>()
                    .With(c => c.Username, username)
                    .With(c => c.PhoneNr, phoneNr);

            Add(chatBuilder.Create(), contactBuilder.Create(), optionsBuilder.Create());
        }
    }

    [Theory, ClassData(typeof(ValidAdminContactData))]
    public async Task AuthMessage_WhenValidAdminContactIsSent(Chat messageChat, Contact contact,
        AdminOptions adminOptions)
    {
        _optionsMock
            .SetupGet(o => o.Value)
            .Returns(adminOptions);

        _ = await _behaviour.Handle(
            new TelegramMessageRequest(new Message { Chat = messageChat, Contact = contact }),
            _handlerMock.Object,
            CancellationToken.None);

        _handlerMock.Verify(next => next(), Times.Never, "Next handler should not be called");
        _botMock.Verify(b =>
                b.SendRequest(
                    It.Is<SendMessageRequest>(r =>
                        r.ChatId == messageChat.Id
                        && r.Text == "Welcome to the Digest!!!")
                    , It.IsAny<CancellationToken>())
            , Times.Once,
            "Message should be sent to the chat");
        _repositoryMock
            .Verify(
                r => r.AddAdminChat(It.Is<AdminChat>(c =>
                    c.ChatId == messageChat.Id
                    && c.Username == messageChat.Username
                    && c.PhoneNr == contact.PhoneNumber))
                , Times.Once
                , "Admin chat should be added");
    }

    private class InvalidAdminContactData : TheoryData<Chat, Contact, AdminOptions>
    {
        public InvalidAdminContactData()
        {
            var fixture = new Fixture();

            var chatId = fixture.Create<long>();
            var phoneNr = fixture.Create<string>();
            var username = fixture.Create<string>();

            var chatBuilder = fixture.Build<Chat>();
            var contactBuilder = fixture.Build<Contact>();
            var optionsBuilder = fixture.Build<AdminOptions>();

            Add(
                chatBuilder.Create(),
                contactBuilder.Create(),
                optionsBuilder.Create());
            Add(
                chatBuilder
                    .With(c => c.Username, username)
                    .Create(),
                contactBuilder
                    .With(c => c.UserId, chatId)
                    .With(c => c.PhoneNumber, phoneNr)
                    .Create(),
                optionsBuilder
                    .With(c => c.Username, username)
                    .With(c => c.PhoneNr, phoneNr)
                    .Create());
            Add(
                chatBuilder
                    .With(c => c.Id, chatId)
                    .Create(),
                contactBuilder
                    .With(c => c.UserId, chatId)
                    .With(c => c.PhoneNumber, phoneNr)
                    .Create(),
                optionsBuilder
                    .With(c => c.Username, username)
                    .With(c => c.PhoneNr, phoneNr)
                    .Create());
            Add(
                chatBuilder
                    .With(c => c.Id, chatId)
                    .With(c => c.Username, username)
                    .Create(),
                contactBuilder
                    .With(c => c.PhoneNumber, phoneNr)
                    .Create(),
                optionsBuilder
                    .With(c => c.Username, username)
                    .With(c => c.PhoneNr, phoneNr)
                    .Create());
            Add(
                chatBuilder
                    .With(c => c.Id, chatId)
                    .With(c => c.Username, username)
                    .Create(),
                contactBuilder
                    .With(c => c.UserId, chatId)
                    .Create(),
                optionsBuilder
                    .With(c => c.Username, username)
                    .With(c => c.PhoneNr, phoneNr)
                    .Create());
            Add(
                chatBuilder
                    .With(c => c.Id, chatId)
                    .With(c => c.Username, username)
                    .Create(),
                contactBuilder
                    .With(c => c.UserId, chatId)
                    .With(c => c.PhoneNumber, phoneNr)
                    .Create(),
                optionsBuilder
                    .With(c => c.PhoneNr, phoneNr)
                    .Create());
            Add(
                chatBuilder
                    .With(c => c.Id, chatId)
                    .With(c => c.Username, username)
                    .Create(),
                contactBuilder
                    .With(c => c.UserId, chatId)
                    .With(c => c.PhoneNumber, phoneNr)
                    .Create(),
                optionsBuilder
                    .With(c => c.Username, username)
                    .Create());
        }
    }

    [Theory, ClassData(typeof(InvalidAdminContactData))]
    public async Task AuthMessage_WhenInvalidAdminContactIsSent(Chat messageChat, Contact contact,
        AdminOptions adminOptions)
    {
        _optionsMock
            .SetupGet(o => o.Value)
            .Returns(adminOptions);

        _ = await _behaviour.Handle(
            new TelegramMessageRequest(
                new Message { Chat = messageChat, Contact = contact }),
            _handlerMock.Object,
            CancellationToken.None);

        _handlerMock.Verify(next => next(), Times.Never, "Next handler should not be called");
        _botMock.Verify(b =>
                b.SendRequest(
                    It.Is<SendMessageRequest>(r =>
                        r.ChatId == messageChat.Id
                        && r.Text == "I don't know you. Introduce yourself")
                    , It.IsAny<CancellationToken>())
            , Times.Once,
            "Message should be sent to the chat");
        _repositoryMock
            .Verify(
                r => r.AddAdminChat(It.IsAny<AdminChat>())
                , Times.Never
                , "Admin chat should not be added");
    }
}