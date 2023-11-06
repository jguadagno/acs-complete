using System.Collections.Generic;
using System.Threading.Tasks;

namespace ACSDemo.IdentityHelper.Models;

public interface IIdentityRepository
{
    Task<Identity> GetUser(string serviceName, string name);
    Task<bool> SaveUser(string serviceName, string name, string identityId);
    Task<bool> DeleteUser(string serviceName, string name);
    Task<List<Identity>> GetUsers(string serviceName);
    Task<List<Identity>> GetUsers();
    Task<bool> SaveUsers(List<Identity> identities);
}