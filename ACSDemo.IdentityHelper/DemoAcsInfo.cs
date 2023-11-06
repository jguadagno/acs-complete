using System;

namespace ACSDemo.IdentityHelper;

public class DemoAcsInfo
{
    private readonly string _resourceVariablePart;

    public DemoAcsInfo(string resource)
    {
        Resource = resource;
        if (resource == VideoAcs)
        {
            _resourceVariablePart = "_" + VideoAcs;
        }
        else
        {
            _resourceVariablePart = string.Empty;
        }
    }

    public string ConnectionString =>
        GetNonNullEnvironmentVariable($"Azure_Communication{_resourceVariablePart}_Connection_String");

    public string EndpointUri =>
        GetNonNullEnvironmentVariable($"Azure_Communication{_resourceVariablePart}_Endpoint");

    public string AccessKey =>
        GetNonNullEnvironmentVariable($"Azure_Communication{_resourceVariablePart}_Access_Key");

    public string Resource { get; }

    public const string VideoAcs = "Video";
    public const string PhoneAcs = "Phone";
    public const string IdentityFileName = @"D:\Projects\AzureCommunicationService\identities.json";

    private string GetNonNullEnvironmentVariable(string variableName)
    {
        if (string.IsNullOrEmpty(variableName))
        {
            return string.Empty;
        }

        var value = Environment.GetEnvironmentVariable(variableName);
        return value ?? string.Empty;
    }

    public Repositories.IdentityFileRepository GetRepository(string fileName = IdentityFileName)
    {
        return new Repositories.IdentityFileRepository(fileName);
    }

    public Services.AzureCommunicationIdentityService GetService(string endpointConnectionString)
    {
        return new Services.AzureCommunicationIdentityService(endpointConnectionString);
    }
}