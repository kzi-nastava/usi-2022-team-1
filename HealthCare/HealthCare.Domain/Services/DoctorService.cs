using HealthCare.Data.Entities;
using HealthCare.Domain.Interfaces;
using HealthCare.Domain.Models;
using HealthCare.Repositories;

namespace HealthCare.Domain.Services;

public class DoctorService : IDoctorService
{
    private IDoctorRepository _doctorRepository;

    public DoctorService(IDoctorRepository doctorRepository) 
    {
        _doctorRepository = doctorRepository;
    }

    private Doctor parseFromModel(DoctorDomainModel doctorModel)
    {
        Doctor doctor = new Doctor 
        {
            IsDeleted = doctorModel.IsDeleted,
            BirthDate = doctorModel.BirthDate,
            Email = doctorModel.Email,
            Id = doctorModel.Id,
            Name = doctorModel.Name,
            Phone = doctorModel.Phone,
            Surname = doctorModel.Surname,
            SpecializationId = doctorModel.SpecializationId
        };
        
        if (doctorModel.Credentials != null)
            doctor.Credentials = CredentialsService.parseFromModel(doctorModel.Credentials);
        
        if(doctorModel.Specialization != null)
            doctor.Specialization = SpecializationService.parseFromModel(doctorModel.Specialization);
        
        doctor.Examinations = new List<Examination>();
        doctor.Operations = new List<Operation>();
        
        if (doctorModel.Examinations != null) 
            foreach (ExaminationDomainModel examinationModel in doctorModel.Examinations)
                doctor.Examinations.Add(ExaminationService.parseFromModel(examinationModel));
        
        if(doctorModel.Operations != null) 
            foreach (OperationDomainModel operationModel in doctorModel.Operations) 
                doctor.Operations.Add(OperationService.parseFromModel(operationModel));
        
        return doctor;
    }
    private DoctorDomainModel parseToModel(Doctor doctor)
    {
        DoctorDomainModel doctorModel = new DoctorDomainModel 
        {
            IsDeleted = doctor.IsDeleted,
            BirthDate = doctor.BirthDate,
            //Credentials = item.Credentials,
            Email = doctor.Email,
            Id = doctor.Id,
            Name = doctor.Name,
            Phone = doctor.Phone,
            Surname = doctor.Surname,
            SpecializationId = doctor.SpecializationId
        };
        if (doctor.Credentials != null)
            doctorModel.Credentials = CredentialsService.parseToModel(doctor.Credentials);

        if (doctor.Specialization != null)
            doctorModel.Specialization = SpecializationService.parseToModel(doctor.Specialization); 
            
        doctorModel.Examinations = new List<ExaminationDomainModel>();
        doctorModel.Operations = new List<OperationDomainModel>();
        if (doctor.Examinations != null) 
            foreach (Examination examination in doctor.Examinations) 
                doctorModel.Examinations.Add(ExaminationService.parseToModel(examination));
                
        if(doctor.Operations != null) 
            foreach (Operation operation in doctor.Operations) 
                doctorModel.Operations.Add(OperationService.parseToModel(operation));
        return doctorModel;
    }
    
    public async Task<IEnumerable<DoctorDomainModel>> GetAll()
    {
        IEnumerable<Doctor> data = await _doctorRepository.GetAll();
        if (data == null)
            return new List<DoctorDomainModel>();

        List<DoctorDomainModel> results = new List<DoctorDomainModel>();
        foreach (Doctor item in data) 
        {
            results.Add(parseToModel(item));
        }
        return results;
    }


    public async Task<IEnumerable<DoctorDomainModel>> GetAllBySpecialization(decimal id)
    {
        IEnumerable<Doctor> data = await _doctorRepository.GetBySpecialization(id);
        if (data == null)
            return new List<DoctorDomainModel>();

        List<DoctorDomainModel> results = new List<DoctorDomainModel>();
        foreach (Doctor item in data)
        {
            results.Add(parseToModel(item));
        }
        return results;
    }

    public async Task<DoctorDomainModel> GetById(decimal id)
    {
        Doctor data = await _doctorRepository.GetDoctorById(id);
        if (data == null)
            return null;
        return parseToModel(data);
    }

    public async Task<IEnumerable<DoctorDomainModel>> ReadAll()
    {
        IEnumerable<DoctorDomainModel> doctors = await GetAll();
        List<DoctorDomainModel> result = new List<DoctorDomainModel>();
        foreach (DoctorDomainModel doctor in doctors)
        {
            if (!doctor.IsDeleted) result.Add(doctor);
        }
        return result;
    }
    
    private DateTime removeSeconds(DateTime dateTime)
    {
        int year = dateTime.Year;
        int month = dateTime.Month;
        int day = dateTime.Day;
        int hour = dateTime.Hour;
        int minute = dateTime.Minute;
        int second = 0;
        return new DateTime(year, month, day, hour, minute, second);
    }

    public async Task<IEnumerable<KeyValuePair<DateTime, DateTime>>> GetAvailableSchedule(decimal doctorId, decimal duration=15)
    {
        Doctor doctor = await _doctorRepository.GetDoctorById(doctorId);
        DoctorDomainModel doctorModel = parseToModel(doctor);
        List<KeyValuePair<DateTime, DateTime>> schedule = new List<KeyValuePair<DateTime, DateTime>>();
        DateTime timeStart, timeEnd;
        // Go through examinations
        foreach (ExaminationDomainModel item in  doctorModel.Examinations)
        {
            if (item.IsDeleted) continue;
            timeStart = removeSeconds(item.StartTime);
            timeEnd = removeSeconds(item.StartTime).AddMinutes(15);
            schedule.Add(new KeyValuePair<DateTime, DateTime>(timeStart, timeEnd));
        }
        // Go through operations
        foreach (OperationDomainModel item in  doctorModel.Operations)
        {
            if (item.IsDeleted) continue;
            timeStart = removeSeconds(item.StartTime);
            timeEnd = removeSeconds(item.StartTime).AddMinutes((double) item.Duration);
            schedule.Add(new KeyValuePair<DateTime, DateTime>(timeStart, timeEnd));
        }
        // Sort the list
        schedule.Sort((x, y) => x.Key.CompareTo(y.Key));
        // Generate available time
        List<KeyValuePair<DateTime, DateTime>> result = new List<KeyValuePair<DateTime, DateTime>>();
        if (schedule.Count == 0) return result;
        KeyValuePair<DateTime, DateTime> first = schedule[0];
        for (int i = 1; i < schedule.Count; i++)
        {
            var currentFirst = first.Value.AddMinutes((double)duration);
            var currentSecond = schedule[i].Key;
            if (currentFirst <= currentSecond)  result.Add(new KeyValuePair<DateTime, DateTime>(first.Value, currentSecond.AddMinutes((double) -duration)));
            first = schedule[i];
        }
        result.Add(new KeyValuePair<DateTime, DateTime>(schedule[schedule.Count - 1].Value, new DateTime(9999, 12, 31)));
        return result;
    }

    public async Task<IEnumerable<KeyValuePair<DateTime, DateTime>>> GetBusySchedule(decimal doctorId)
    {
        // TODO: This and the above function can work together
        Doctor doctor = await _doctorRepository.GetDoctorById(doctorId);
        DoctorDomainModel doctorModel = parseToModel(doctor);
        List<KeyValuePair<DateTime, DateTime>> schedule = new List<KeyValuePair<DateTime, DateTime>>();
        DateTime timeStart, timeEnd;
        // Go through examinations
        foreach (ExaminationDomainModel item in  doctorModel.Examinations)
        {
            if (item.IsDeleted) continue;
            timeStart = removeSeconds(item.StartTime);
            timeEnd = removeSeconds(item.StartTime).AddMinutes(15);
            schedule.Add(new KeyValuePair<DateTime, DateTime>(timeStart, timeEnd));
        }
        // Go through operations
        foreach (OperationDomainModel item in  doctorModel.Operations)
        {
            if (item.IsDeleted) continue;
            timeStart = removeSeconds(item.StartTime);
            timeEnd = removeSeconds(item.StartTime).AddMinutes((double) item.Duration);
            schedule.Add(new KeyValuePair<DateTime, DateTime>(timeStart, timeEnd));
        }
        // Sort the list
        schedule.Sort((x, y) => x.Key.CompareTo(y.Key));
        // Generate busy time
        return schedule;
    }
}