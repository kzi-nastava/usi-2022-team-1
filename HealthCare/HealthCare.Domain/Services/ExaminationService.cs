using HealthCare.Data.Entities;
using HealthCare.Domain.Interfaces;
using HealthCare.Domain.Models;
using HealthCare.Domain.Models.ModelsForCreate;
using HealthCare.Domain.Models.ModelsForUpdate;
using HealthCare.Repositories;

namespace HealthCare.Domain.Services;

public class ExaminationService : IExaminationService{
    private IExaminationRepository _examinationRepository;
    private IExaminationApprovalRepository _examinationApprovalRepository;
    private IOperationRepository _operationRepository;
    private IRoomRepository _roomRepository;

    public ExaminationService(IExaminationRepository examinationRepository, IExaminationApprovalRepository examinationApprovalRepository, IOperationRepository operationRepository, IRoomRepository roomRepository) {
        _examinationRepository = examinationRepository;
        _examinationApprovalRepository = examinationApprovalRepository;
        _operationRepository = operationRepository;
        _roomRepository = roomRepository;
    }

    // Async awaits info from database
    // GetAll is the equivalent of SELECT *

    private ExaminationDomainModel parseToModel(Examination examination) {
        ExaminationDomainModel examinationModel = new ExaminationDomainModel {
            StartTime = examination.StartTime,
            doctorId = examination.doctorId,
            IsDeleted = examination.IsDeleted,
            patientId = examination.patientId,
            roomId = examination.roomId
        };
        if (examination.Anamnesis != null) {
            examinationModel.Anamnesis = new AnamnesisDomainModel {
                Description = examination.Anamnesis.Description,
                roomId = examination.Anamnesis.roomId,
                doctorId = examination.Anamnesis.doctorId,
                StartTime = examination.Anamnesis.StartTime,
                isDeleted = examination.Anamnesis.isDeleted
            };
        }
        return examinationModel;
    }
    public async Task<IEnumerable<ExaminationDomainModel>> GetAll()
    {
        var data = await _examinationRepository.GetAll();
        if (data == null)
            return null;

        List<ExaminationDomainModel> results = new List<ExaminationDomainModel>();
        
        foreach (var item in data)
        {           
            results.Add(parseToModel(item));
        }

        return results;
    }

    public async Task<IEnumerable<ExaminationDomainModel>> GetAllForPatient(decimal id) {
        var data = await _examinationRepository.GetAllByPatientId(id);
        if (data == null)
            return null;

        List<ExaminationDomainModel> results = new List<ExaminationDomainModel>();

        foreach (var item in data) {
            results.Add(parseToModel(item));
        }

        return results;
    }

    public async Task<IEnumerable<ExaminationDomainModel>> GetAllForDoctor(decimal id)
    {
        var data = await _examinationRepository.GetAllByDoctorId(id);
        if (data == null)
            return null;

        List<ExaminationDomainModel> results = new List<ExaminationDomainModel>();

        foreach (var item in data)
        {
            results.Add(parseToModel(item));
        }

        return results;
    }

    public async Task<ExaminationDomainModel> Delete(DeleteExaminationDomainModel deleteExamination) {
        var examination = await _examinationRepository.GetExaminationWithoutAnamnesis(deleteExamination.roomId, deleteExamination.doctorId, deleteExamination.patientId, deleteExamination.StartTime);
        var daysUntilExamination = (deleteExamination.StartTime - DateTime.Now).TotalDays;
        if(daysUntilExamination > 1) {
            examination.IsDeleted = true;
            _ = _examinationRepository.Update(examination);
            _examinationRepository.Save();
        } else {
            ExaminationApproval examinationApproval = new ExaminationApproval {
                State = "created",
                RoomId = examination.roomId,
                DoctorId = examination.doctorId,
                PatientId = examination.patientId,
                StartTime = examination.StartTime,
                isDeleted = false,
                Examination = examination
            };
            _ = _examinationApprovalRepository.Post(examinationApproval);
            _examinationApprovalRepository.Save();
        }
        return null;
       
    }

    private async Task<bool> IsPatientOnExaminationAsync(CreateExaminationDomainModel examinationModel)
    {
        var patientsExaminations = await _examinationRepository.GetAllByPatientId(examinationModel.patientId);
        foreach (Examination examination in patientsExaminations)
        {
            double difference = (examinationModel.StartTime - examination.StartTime).TotalMinutes;
            if (difference <= 15 && difference >= -15)
            {
                return true;
            }
        }
        return false;
    }

    private async Task<bool> IsPatientOnOperationAsync(CreateExaminationDomainModel examinationModel)
    {
        var patientsOperations = await _operationRepository.GetAllByPatientId(examinationModel.patientId);
        foreach (Operation operation in patientsOperations)
        {
            double difference = (examinationModel.StartTime - operation.StartTime).TotalMinutes;
            if (difference <= (double)operation.Duration && difference >= -15)
            {
                return true;
            }
        }
        return false;
    }

    private async Task<bool> IsDoctorOnExaminationAsync(CreateExaminationDomainModel examinationModel) {
        var doctorsExaminations = await _examinationRepository.GetAllByDoctorId(examinationModel.doctorId);
        foreach (Examination examination in doctorsExaminations) {
            double difference = (examinationModel.StartTime - examination.StartTime).TotalMinutes;
            if (difference <= 15 && difference >= -15) {
                return true;
            }
        }
        return false;
    }

    private async Task<bool> IsDoctorOnOperationAsync(CreateExaminationDomainModel examinationModel) {
        var doctorsOperations = await _operationRepository.GetAllByDoctorId(examinationModel.doctorId);
        foreach (Operation operation in doctorsOperations) {
            double difference = (examinationModel.StartTime - operation.StartTime).TotalMinutes;
            if (difference <= (double) operation.Duration && difference >= -15) {
                return true;
            }
        }
        return false;
    }

    private async Task<decimal> GetAvailableRoomId(CreateExaminationDomainModel examinationModel) {
        var rooms = await _roomRepository.GetAllExaminationRooms();
        foreach (Room room in rooms) {
            bool isRoomAvailable = true;
            var examinations = await _examinationRepository.GetAllByRoomId(room.Id);
            foreach (Examination examination in examinations) {
                double difference = (examinationModel.StartTime - examination.StartTime).TotalMinutes;
                if (difference <= 15 && difference >= -15) {
                    isRoomAvailable = false;
                    break;
                }
            }
            if (isRoomAvailable) {
                return room.Id;
            }
        }
        return -1;
    }


    public async Task<bool> IsDoctorAvailable(CreateExaminationDomainModel examinationModel)
    {
        bool isDoctorAvailable = true;
        if (await IsDoctorOnExaminationAsync(examinationModel))
            isDoctorAvailable = false;
        if (await IsDoctorOnOperationAsync(examinationModel))
            isDoctorAvailable = false;

        if (!isDoctorAvailable)
        {
            //TODO: Think about the return value if doctor is not available
            return false;
        }

        return true;
    }

    public async Task<bool> IsPatientAvailable(CreateExaminationDomainModel examinationModel)
    {
        bool isPatientAvailable = true;
        if (await IsPatientOnExaminationAsync(examinationModel))
            isPatientAvailable = false;
        if (await IsPatientOnOperationAsync(examinationModel))
            isPatientAvailable = false;

        if (!isPatientAvailable)
        {
            return false;
        }

        return true;
    }

    public async Task<CreateExaminationDomainModel> Create(CreateExaminationDomainModel examinationModel) {

        bool doctorAvailable = await IsDoctorAvailable(examinationModel);
        bool patientAvailable = await IsPatientAvailable(examinationModel);
        if (!doctorAvailable || !patientAvailable)
            //TODO: Think about the return value if doctor is not available
            return null;

        decimal roomId = await GetAvailableRoomId(examinationModel);
        if (roomId == -1) {
            return null;
        }

        Examination newExamination = new Examination {
            patientId = examinationModel.patientId,
            roomId = roomId,
            doctorId = examinationModel.doctorId,
            StartTime = examinationModel.StartTime,
            IsDeleted = false,
            Anamnesis = null,
            ExaminationApproval = null
        };

        _ = _examinationRepository.Post(newExamination);
        _examinationRepository.Save();

        return examinationModel;
    }

    private async Task<bool> IsDoctorOnExaminationAsync(UpdateExaminationDomainModel examinationModel) {
        var doctorsExaminations = await _examinationRepository.GetAllByDoctorId(examinationModel.newDoctorId);
        foreach (Examination examination in doctorsExaminations) {
            double difference = (examinationModel.newStartTime - examination.StartTime).TotalMinutes;
            if (difference <= 15 && difference >= -15) {
                return true;
            }
        }
        return false;
    }

    private async Task<bool> IsDoctorOnOperationAsync(UpdateExaminationDomainModel examinationModel) {
        var doctorsOperations = await _operationRepository.GetAllByDoctorId(examinationModel.newDoctorId);
        foreach (Operation operation in doctorsOperations) {
            double difference = (examinationModel.newStartTime - operation.StartTime).TotalMinutes;
            if (difference <= (double)operation.Duration && difference >= -15) {
                return true;
            }
        }
        return false;
    }



    private async Task<decimal> GetAvailableRoomId(UpdateExaminationDomainModel examinationModel) {
        var rooms = await _roomRepository.GetAllExaminationRooms();
        foreach (Room room in rooms) {
            bool isRoomAvailable = true;
            var examinations = await _examinationRepository.GetAllByRoomId(room.Id);
            foreach (Examination examination in examinations) {
                double difference = (examinationModel.newStartTime - examination.StartTime).TotalMinutes;
                if (difference <= 15 && difference >= -15) {
                    isRoomAvailable = false;
                    break;
                }
            }
            if (isRoomAvailable) {
                return room.Id;
            }
        }
        return -1;
    }

    public async Task<bool> IsDoctorAvailable(UpdateExaminationDomainModel examinationModel) {
        bool isDoctorAvailable = true;
        if (await IsDoctorOnExaminationAsync(examinationModel))
            isDoctorAvailable = false;
        if (await IsDoctorOnOperationAsync(examinationModel))
            isDoctorAvailable = false;

        if (!isDoctorAvailable)
        {
            //TODO: Think about the return value if doctor is not available
            return false;
        }

        return true;
    }

    private async Task<bool> IsPatientOnExaminationAsync(UpdateExaminationDomainModel examinationModel)
    {
        var patientsExaminations = await _examinationRepository.GetAllByPatientId(examinationModel.newPatientId);
        foreach (Examination examination in patientsExaminations)
        {
            double difference = (examinationModel.newStartTime - examination.StartTime).TotalMinutes;
            if (difference <= 15 && difference >= -15)
            {
                return true;
            }
        }
        return false;
    }

    private async Task<bool> IsPatientOnOperationAsync(UpdateExaminationDomainModel examinationModel)
    {
        var patientsOperations = await _operationRepository.GetAllByPatientId(examinationModel.newPatientId);
        foreach (Operation operation in patientsOperations)
        {
            double difference = (examinationModel.newStartTime - operation.StartTime).TotalMinutes;
            if (difference <= (double)operation.Duration && difference >= -15)
            {
                return true;
            }
        }
        return false;
    }

    public async Task<bool> IsPatientAvailable(UpdateExaminationDomainModel examinationModel)
    {
        bool isPatientAvailable = true;
        if (await IsPatientOnExaminationAsync(examinationModel))
            isPatientAvailable = false;
        if (await IsPatientOnOperationAsync(examinationModel))
            isPatientAvailable = false;

        if (!isPatientAvailable)
        {
            return false;
        }

        return true;
    }

    public async Task<UpdateExaminationDomainModel> Update(UpdateExaminationDomainModel examinationModel) {
        bool doctorAvailable = await IsDoctorAvailable(examinationModel);
        bool patientAvailable = await IsPatientAvailable(examinationModel);
        if (!doctorAvailable || !patientAvailable)
            //TODO: Think about the return value if doctor is not available
            return null;


        decimal roomId = await GetAvailableRoomId(examinationModel);
        if (roomId == -1) {
            return null;
        }

        var examination = await _examinationRepository.GetExaminationWithoutAnamnesis(examinationModel.oldRoomId, examinationModel.oldDoctorId, examinationModel.oldPatientId, examinationModel.oldStartTime);
        examination.roomId = roomId;
        examination.doctorId = examinationModel.newDoctorId;
        examination.StartTime = examinationModel.newStartTime;
        _ = _examinationRepository.Update(examination);
        _examinationRepository.Save();

        return examinationModel;
    }
}