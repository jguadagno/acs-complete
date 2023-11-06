using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ACSDemo.IdentityHelper.Models;

namespace ACSDemo.IdentityHelper.Repositories;

public class IdentityFileRepository : IIdentityRepository
{
    public IdentityFileRepository(string fileName)
    {
        FileName = fileName;
    }

    private string FileName { get; }
        
    public async Task<Identity> GetUser(string serviceName, string name)
    {
        try
        {
            var identities = await GetUsers(serviceName);
            return identities.FirstOrDefault(i => i.Name == name);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<bool> SaveUser(string serviceName, string name, string identityId)
    {
        var identities = await GetUsers();
        var identity = identities.FirstOrDefault(i => i.ResourceName == serviceName && i.Name == name);
        if (identity == null)
        {
            identities.Add(new Identity
            {
                ResourceName = serviceName, Name = name, Id = identityId
            });
        }
        else
        {
            identity.Id = identityId;
        }

        return await SaveUsers(identities);
    }

    public async Task<bool> DeleteUser(string serviceName, string name)
    {
        var identities = await GetUsers();
        if (identities.Exists(i => i.ResourceName == serviceName && i.Name == name))
        {
            var identityToRemove = identities.Where(i => i.ResourceName == serviceName && i.Name == name);
            foreach (var identity in identityToRemove)
            {
                identities.Remove(identity);
            }

            return await SaveUsers(identities);
        }

        return true;
    }

    public async Task<List<Identity>> GetUsers(string serviceName)
    {
        var identities = await GetUsers();
        return identities.Where(i => i.ResourceName == serviceName).ToList();
    }

    public async Task<List<Identity>> GetUsers()
    {
        try
        {
            if (File.Exists(FileName) == false)
            {
                return new List<Identity>();
            }

            var fileText = await File.ReadAllTextAsync(FileName);
            var identities = JsonSerializer.Deserialize<List<Identity>>(fileText);
            return identities?? new List<Identity>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<bool> SaveUsers(List<Identity> identities)
    {
        try
        {
            var identitiesAsJson =
                JsonSerializer.Serialize(identities, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(FileName, identitiesAsJson);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}