using HealthCare.Domain.Models;

namespace HealthCare.Domain.Interfaces;

public interface IMedicalRecordService : IService<MedicalRecordDomainModel> {
    public Task<MedicalRecordDomainModel> GetForPatient(decimal id);
}