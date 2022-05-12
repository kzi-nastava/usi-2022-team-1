﻿using HealthCare.Data.Entities;
using HealthCare.Domain.Interfaces;
using HealthCare.Domain.Models;
using HealthCare.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace HealthCare.Domain.Services
{
    public class AppointmentService : IAppointmentService
    {
        private IExaminationRepository _examinationRepository;
        private IOperationRepository _operationRepository;

        public AppointmentService(IExaminationRepository examinationRepository, IOperationRepository operationRepoaitory)
        {
            _examinationRepository = examinationRepository;
            _operationRepository = operationRepoaitory;
        }

        public async Task<IEnumerable<AppointmentDomainModel>> GetAll()
        {
            return new List<AppointmentDomainModel>();
        }

        public async Task<IEnumerable<AppointmentDomainModel>> GetAllForDoctor(decimal id, DateTime date)
        {
            IEnumerable<Examination> examinationData = await _examinationRepository.GetAllByDoctorId(id, date);
            List<AppointmentDomainModel> results = new List<AppointmentDomainModel>();
            foreach (Examination item in examinationData)
            {
                results.Add(parseToModel(item));
            }

            IEnumerable<Operation> operationData = await _operationRepository.GetAllByDoctorId(id, date);
            foreach (Operation item in operationData)
            {
                results.Add(parseToModel(item));
            }

            return results;
        }

        public static AppointmentDomainModel parseToModel(Examination examination)
        {
            AppointmentDomainModel appointmentModel = new AppointmentDomainModel
            {
                Id = examination.Id,
                StartTime = examination.StartTime,
                Duration = 15,
                DoctorId = examination.DoctorId,
                IsDeleted = examination.IsDeleted,
                PatientId = examination.PatientId,
                RoomId = examination.RoomId,
                Type = Appointment.Examination
            };
            if (examination.Anamnesis != null)
            {
                appointmentModel.Anamnesis = AnamnesisService.parseToModel(examination.Anamnesis);
            }
            return appointmentModel;
        }

        public static AppointmentDomainModel parseToModel(Operation operation)
        {
            AppointmentDomainModel appointmentModel = new AppointmentDomainModel
            {
                Id = operation.Id,
                StartTime = operation.StartTime,
                Duration = operation.Duration,
                DoctorId = operation.DoctorId,
                IsDeleted = operation.IsDeleted,
                PatientId = operation.PatientId,
                RoomId = operation.RoomId,
                Anamnesis = null,
                Type = Appointment.Operation
            };

            return appointmentModel;
        }
    }
}
