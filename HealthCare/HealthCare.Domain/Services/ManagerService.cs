using HealthCare.Data.Entities;
using HealthCare.Domain.Interfaces;
using HealthCare.Domain.Models;
using HealthCare.Repositories;

namespace HealthCare.Domain.Services;

public class ManagerService : IManagerService
{
    private IManagerRepository _managerRepository;

    public ManagerService(IManagerRepository managerRepository) 
    {
        _managerRepository = managerRepository;
    }

    // Async awaits info from database
    // GetAll is the equivalent of SELECT *
    public async Task<IEnumerable<ManagerDomainModel>> ReadAll()
    {
        IEnumerable<ManagerDomainModel> managers = await GetAll();
        List<ManagerDomainModel> result = new List<ManagerDomainModel>();
        foreach (ManagerDomainModel item in managers)
        {
            if (!item.IsDeleted) result.Add(item);
        }
        return result;
    } 
    public async Task<IEnumerable<ManagerDomainModel>> GetAll()
    {
        IEnumerable<Manager> data = await _managerRepository.GetAll();
        if (data == null)
            return new List<ManagerDomainModel>();

        List<ManagerDomainModel> results = new List<ManagerDomainModel>();
        ManagerDomainModel managerModel;
        foreach (Manager item in data)
        {
            managerModel = new ManagerDomainModel
            {
                IsDeleted = item.IsDeleted,
                BirthDate = item.BirthDate,
                Email = item.Email,
                Id = item.Id,
                Name = item.Name,
                Phone = item.Phone,
                Surname = item.Surname
            };
            if (item.Credentials != null) 
            {
                managerModel.Credentials = new CredentialsDomainModel 
                {
                    Id = item.Credentials.Id,
                    Username = item.Credentials.Username,
                    Password = item.Credentials.Password,
                    DoctorId = item.Credentials.DoctorId,
                    SecretaryId = item.Credentials.SecretaryId,
                    ManagerId = item.Credentials.ManagerId,
                    PatientId = item.Credentials.PatientId,
                    UserRoleId = item.Credentials.UserRoleId

                };
                if (item.Credentials.UserRole != null) 
                {
                    managerModel.Credentials.UserRole = new UserRoleDomainModel 
                    {
                        Id = item.Credentials.UserRole.Id,
                        RoleName = item.Credentials.UserRole.RoleName,
                        IsDeleted = item.Credentials.UserRole.IsDeleted
                    };
                }
            }
            results.Add(managerModel);
        }

        return results;
    } 
}