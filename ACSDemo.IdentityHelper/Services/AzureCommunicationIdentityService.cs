using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Azure;
using Azure.Communication;
using Azure.Communication.Identity;
using Azure.Core;

namespace ACSDemo.IdentityHelper.Services;

public class AzureCommunicationIdentityService
{
    private readonly CommunicationIdentityClient _client;

    public AzureCommunicationIdentityService(string connectionString)
    {
        _client = new CommunicationIdentityClient(connectionString);
    }

    public AzureCommunicationIdentityService(string endpoint, string accessKey)
    {
        _client = new CommunicationIdentityClient(new Uri(endpoint), new AzureKeyCredential(accessKey));
    }

    public async Task<string> CreateIdentity()
    {
        try
        {
            var identityResponse = await _client.CreateUserAsync();
            var identity = identityResponse.Value;
            return identity?.Id;
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync($"Failed to create the identity. {e.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteIdentity(string identityValue)
    {
        try
        {
            var identity = new CommunicationUserIdentifier(identityValue);
            await _client.DeleteUserAsync(identity);
            return true;
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync($"Failed to delete the identity: '{identityValue}. {e.Message}");
            return false;
        }
    }

    public async Task<AccessToken?> GetToken(string identity, IEnumerable<CommunicationTokenScope> scopes)
    {
        var user = new CommunicationUserIdentifier(identity);
        var tokenResponse = await _client.GetTokenAsync(user, scopes);
        return tokenResponse?.Value;
    }

    public async Task<bool> RevokeToken(string identity)
    {
        var user = new CommunicationUserIdentifier(identity);
        var response = await _client.RevokeTokensAsync(user);
        return response.Status == (int)HttpStatusCode.NoContent;
    }
}