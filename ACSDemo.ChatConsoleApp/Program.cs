// See https://aka.ms/new-console-template for more information

using ACSDemo.IdentityHelper;
using Azure;
using Azure.Communication;
using Azure.Communication.Chat;
using Azure.Communication.Identity;

Console.WriteLine("Chat Client Started!");

// TODO: For the talk, remove _Video, we will use the paid account.
var acsDemoInfo = new DemoAcsInfo(DemoAcsInfo.VideoAcs);
var identityRepository = acsDemoInfo.GetRepository();
var identityService = acsDemoInfo.GetService(acsDemoInfo.ConnectionString);

// Create the Chat Client
Uri endpoint = new Uri(acsDemoInfo.EndpointUri);
// Get a token - Step 1 - Add Azure.Communication.Chat

// Part of the Demo, line below, that we will comment out
//var token = new CommunicationTokenCredential("AccessToken"); // Access Token is Needed
// Step 2: To get a token we need to request a token for a user to 'chat'
//  Lets do that,  add Azure.Communication.Identity
// Here is where we would have a token/user store. However we don't yet :)

// Add the ACSDemo.IdentityHelper project to the solution (Maybe show?)
// Add a reference to the ACSDemo.IdentityHelper project

// Assume the following is coming from an identity store (Maybe an Azure function) :)
var joeIdentity = await identityRepository.GetUser(acsDemoInfo.Resource, "Joe");
var joeToken = await identityService.GetToken(joeIdentity!.Id, new[] { CommunicationTokenScope.Chat, });
if (joeToken is null)
{
    await Console.Error.WriteLineAsync("Failed to get chat token for Joe");
    return -1;
}

var chatTokenForJoe = new CommunicationTokenCredential(joeToken.Value.Token); // Now we have the chat Token

// Let's build the client
var chatClient = new ChatClient(endpoint, chatTokenForJoe);  // Chat client created

// Now we have to create a 'Chat Thread'
// First a ChatParticipant is needed
var joeParticipant = new ChatParticipant(new CommunicationUserIdentifier(joeIdentity.Id)) { DisplayName = "Joe" };
// Create the ChatThread
var topicId = $"{DateTime.Now:s}-{Guid.NewGuid()}";
var chatThreadResult = await chatClient.CreateChatThreadAsync(topic: topicId, new[] { joeParticipant });
// Now get a reference to ChatThreadClient - This is needed for any messages that need to be sent
var chatThreadClient = chatClient.GetChatThreadClient(threadId: chatThreadResult.Value.ChatThread.Id);
// Save off the thread id (for ease of use)
var chatThreadId = chatThreadClient.Id;

// Now that we have a thread id, we'll use that to send a message
// Show SendMessageAsync();
// SendMessage needs
// the Content
// The type: 'Text' or 'HTML`
// Sender Display Name
// Optional metadata
var chatMessage = new SendChatMessageOptions
{
    Content = "Sample message for TechBash",
    MessageType = ChatMessageType.Text
};
var chatMessageResponse = await chatThreadClient.SendMessageAsync(chatMessage);
var chatMessageId = chatMessageResponse.Value.Id;

// Now let's see the message
AsyncPageable<ChatMessage> chatMessages = chatThreadClient.GetMessagesAsync(DateTimeOffset.Now.AddHours(-1));
await foreach (var message in chatMessages)
{
    Console.WriteLine($"Message Id: {message.Id}, Message:'{message.Content.Message}'");
}

// There are three message, but I only sent 1.
// Talk to the different message types
// * Text: Regular chat message sent by a thread member.
// * Html: A formatted text message. Note that Communication Services users currently can't send RichText messages.
//      This message type is supported by messages sent from Teams users to Communication Services users in Teams Interop scenarios.
// * TopicUpdated: System message that indicates the topic has been updated. (readonly)
// * ParticipantAdded: System message that indicates one or more participants have been added to the chat thread.(readonly)
// * ParticipantRemoved: System message that indicates a participant has been removed from the chat thread.

// Update the message loop
chatMessages = chatThreadClient.GetMessagesAsync(DateTimeOffset.Now.AddHours(-1));
await foreach (var message in chatMessages)
{
    Console.WriteLine($"Message Id: {message.Id}, Message:'{message.Content.Message}', Type: '{message.Type.ToString()}'");
}

// Let's see how we look up for a list of threads
// Look for all the Threads - For the demo, let's filter it to the last hour
AsyncPageable<ChatThreadItem> chatThreadItems = chatClient.GetChatThreadsAsync(DateTimeOffset.Now.AddHours(-1));
await foreach (var chatThreadItem in chatThreadItems)
{
    Console.WriteLine(
        $"ChatThreadId: {chatThreadItem.Id}, Topic: '{chatThreadItem.Topic}', DeletedOn: '{chatThreadItem.DeletedOn:G}'");
}

// Now let's add some people to our 'chat'
// Create the GetUser method
var deeParticipant = new ChatParticipant(await GetUser("Dee", acsDemoInfo.Resource)) { DisplayName = "Dee" };
var jjParticipant = new ChatParticipant(await GetUser("JJ", acsDemoInfo.Resource)) { DisplayName = "JJ" };
var emilyParticipant = new ChatParticipant(await GetUser("Emily", acsDemoInfo.Resource)) { DisplayName = "Emily" };

var participants = new[] { deeParticipant, jjParticipant, emilyParticipant };
await chatThreadClient.AddParticipantsAsync(participants);

// Let's validate that all the participants are in the chat
AsyncPageable<ChatParticipant> chatParticipants = chatThreadClient.GetParticipantsAsync();
await foreach (var participant in chatParticipants)
{
    Console.WriteLine($"Participant: '{participant.DisplayName}'");
}

// Clean up the thread for the demo
await chatClient.DeleteChatThreadAsync(chatThreadId);

Console.WriteLine("Done");
return 0;

async Task<CommunicationUserIdentifier> GetUser(string name, string serviceName)
{
    var identity = await identityRepository.GetUser(serviceName, name);
    var token = await identityService.GetToken(identity!.Id, new[] { CommunicationTokenScope.Chat, });
    if (token is null)
    {
        await Console.Error.WriteLineAsync($"Failed to get chat token for {name}");
        throw new ApplicationException($"Failed to get chat token for {name}");
    }

    return new CommunicationUserIdentifier(identity.Id);
}