using HealthCare.Domain.Interfaces;
using HealthCare.Domain.Models;
using HealthCare.Repositories;

namespace HealthCare.Domain.Services;

public class EquipmentService : IEquipmentService{
    private IEquipmentRepository _equipmentRepository;

    public EquipmentService(IEquipmentRepository equipmentRepository) {
        _equipmentRepository = equipmentRepository;
    }

    // Async awaits info from database
    // GetAll is the equivalent of SELECT *
    public async Task<IEnumerable<EquipmentDomainModel>> GetAll()
    {
        var data = await _equipmentRepository.GetAll();
        if (data == null)
            return null;

        List<EquipmentDomainModel> results = new List<EquipmentDomainModel>();
        EquipmentDomainModel equipmentModel;
        foreach (var item in data)
        {
            equipmentModel = new EquipmentDomainModel
            {
                Id = item.Id,
                EquipmentType = item.EquipmentType,
                equipmentTypeId = item.equipmentTypeId,
                Inventories = item.Inventories,
                IsDeleted = item.IsDeleted,
                Name = item.Name,
                Transfers = item.Transfers
            };
            results.Add(equipmentModel);
        }

        return results;
    }
}