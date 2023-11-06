using Azure.Communication;
using Azure.Communication.Identity;

Console.WriteLine("Creating ACS Identities and Access Management!");

// Step 1: Create a Communication Identity Client
// Note, the can be done, with Azure Active Directory AD
var videoConnectionString = Environment.GetEnvironmentVariable("Azure_Communication_Video_Connection_String");
if (videoConnectionString is null or "")
{
    return -1;
}
var client = new CommunicationIdentityClient(videoConnectionString);

// Step 2: Create the Identity 
var identityResponse = await client.CreateUserAsync();
var identity = identityResponse.Value.Id;
if (identity is null or "")
{
    await Console.Error.WriteLineAsync("Failed to get an identity");
    return -1;
}
Console.WriteLine($"New Identity with Id: '{identity}'");

// Step 3: Get a Token
var tokenResponse = await client.GetTokenAsync(new CommunicationUserIdentifier(identity),
    new[] { CommunicationTokenScope.Chat, CommunicationTokenScope.VoIP, });
var token = tokenResponse.Value;
Console.WriteLine($"Token: {token.Token}, Expires on: {token.ExpiresOn}");

// Alternative Step 2 & 3 combined
var identityAndTokenResponse = await client.CreateUserAndTokenAsync(new [] {CommunicationTokenScope.Chat, CommunicationTokenScope.VoIP, });
var identityAndToken = identityAndTokenResponse.Value;
// User
Console.WriteLine($"Identity: {identityAndToken.User.Id}");
Console.WriteLine($"Token: {identityAndToken.AccessToken.Token}, Expires on: {identityAndToken.AccessToken.ExpiresOn}");

// Refresh Access Token
var identityToRefresh = new CommunicationUserIdentifier(identity);
tokenResponse = await client.GetTokenAsync(identityToRefresh,
    new[] { CommunicationTokenScope.Chat, CommunicationTokenScope.VoIP, });
token = tokenResponse.Value;
Console.WriteLine($"Refreshed Token: {token.Token}, Expires on: {token.ExpiresOn}");

// Revoke Access Token
await client.RevokeTokensAsync(new CommunicationUserIdentifier(identity));
Console.WriteLine($"Revoked token for '{identity}'");

// Delete an identity
await client.DeleteUserAsync(new CommunicationUserIdentifier(identity));
await client.DeleteUserAsync(new CommunicationUserIdentifier(identityAndToken.User.Id));

Console.WriteLine("Deleted users");
Console.WriteLine("Done");
Console.ReadLine();
return 0;

