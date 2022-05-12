﻿using HealthCare.Data.Entities;
using HealthCare.Domain.Interfaces;
using HealthCare.Domain.Models;
using HealthCare.Domain.DTOs;
using HealthCare.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace HealthCare.Domain.Services
{
    public class ReferralLetterService : IReferralLetterService
    {
        private IReferralLetterRepository _referralLetterRepository;
        private IDoctorRepository _doctorRepository;
        private ISpecializationRepository _specializationRepository;

        public ReferralLetterService(IReferralLetterRepository referralLetterRepository, IDoctorRepository doctorRepository, ISpecializationRepository specializationRepository)
        {
            _referralLetterRepository = referralLetterRepository;
            _doctorRepository = doctorRepository;
            _specializationRepository = specializationRepository;
        }

        public static ReferralLetterDomainModel parseToModel(ReferralLetter referralLetter)
        {
            ReferralLetterDomainModel referralLetterModel = new ReferralLetterDomainModel
            {
                Id = referralLetter.Id,
                FromDoctorId = referralLetter.FromDoctorId,
                ToDoctorId = referralLetter.ToDoctorId,
                PatientId = referralLetter.PatientId,
                SpecializationId = referralLetter.SpecializationId,
                State = referralLetter.State
            };
            if (referralLetter.Specialization != null)
                referralLetterModel.Specialization = SpecializationService.parseToModel(referralLetter.Specialization); 
                    
            return referralLetterModel;
        }

        public static ReferralLetter parseFromModel(ReferralLetterDomainModel referralLetterModel)
        {
            ReferralLetter referralLetter = new ReferralLetter
            {
                Id = referralLetterModel.Id,
                FromDoctorId = referralLetterModel.FromDoctorId,
                ToDoctorId = referralLetterModel.ToDoctorId,
                PatientId = referralLetterModel.PatientId,
                SpecializationId = referralLetterModel.SpecializationId,
                State = referralLetterModel.State
            };
            if (referralLetterModel.Specialization != null)
                referralLetter.Specialization = SpecializationService.parseFromModel(referralLetterModel.Specialization); 
                    
            return referralLetter;
        }

        public async Task<IEnumerable<ReferralLetterDomainModel>> GetAll()
        {
            IEnumerable<ReferralLetter> data = await _referralLetterRepository.GetAll();
            if (data == null)
                return new List<ReferralLetterDomainModel>();

            List<ReferralLetterDomainModel> results = new List<ReferralLetterDomainModel>();
            foreach (ReferralLetter item in data)
            {
                results.Add(parseToModel(item));
            }

            return results;
        }

        public async Task<Boolean> TryCreateExamination(CUExaminationDTO dto, IExaminationService examinationService)
        {
            try
            {
                ExaminationDomainModel createdExamination = await examinationService.Create(dto);
            }
            catch (Exception exception)
            {
                return false;
            }

            return true;
        }

        public async Task<ReferralLetterDomainModel> CreateAppointment(CreateAppointmentDTO dto, IExaminationService examinationService)
        {
            ReferralLetter referralLetter = await _referralLetterRepository.GetById(dto.ReferralId);
            ReferralLetterDomainModel referralLetterModel = parseToModel(referralLetter);
            
            if (!referralLetterModel.State.Equals("created")) throw new ReferralCannotBeUsedException();

            CUExaminationDTO examinationModel = new CUExaminationDTO
            {
                PatientId = referralLetterModel.PatientId,
                StartTime = dto.StartTime,
                IsPatient = false
            };
            
            if (referralLetterModel.ToDoctorId != null)
            {
                examinationModel.DoctorId = referralLetterModel.ToDoctorId.GetValueOrDefault();
                try
                {
                    ExaminationDomainModel createdExamination = await examinationService.Create(examinationModel);
                }
                catch (Exception exception)
                {
                    throw exception;
                }

                referralLetterModel.State = "accepted";
                referralLetter.State = "accepted";
                _ = _referralLetterRepository.Update(referralLetter);
                _referralLetterRepository.Save();
                return referralLetterModel;
            }
            else
            {
                IEnumerable<Doctor> allDoctors = await _doctorRepository.GetAll();
                Boolean created = false;
                foreach (Doctor doctor in allDoctors)
                {
                    if (doctor.Id == referralLetterModel.FromDoctorId) continue;
                    
                    if (doctor.SpecializationId == referralLetterModel.SpecializationId)
                    {
                        examinationModel.DoctorId = doctor.Id;
                        created = await TryCreateExamination(examinationModel, examinationService);
                        if (created)
                        {
                            referralLetterModel.ToDoctorId = doctor.Id;
                            referralLetterModel.State = "accepted";
                            referralLetter.ToDoctorId = doctor.Id;
                            referralLetter.State = "accepted";
                            _ = _referralLetterRepository.Update(referralLetter);
                            _referralLetterRepository.Save();
                            return referralLetterModel;
                        }
                    }
                }
                if (!created) throw new NoAvailableSpecialistsException();
            }

            return referralLetterModel;
        }

        public async Task<ReferralLetterDomainModel> Create(ReferralLetterDTO referralDTO)
        {
            // check if he chose himself as the doctor? front/back?

            ReferralLetter newReferral = new ReferralLetter 
            {
                FromDoctorId = referralDTO.FromDoctorId,
                PatientId = referralDTO.PatientId,
                ToDoctorId = referralDTO.ToDoctorId,
                SpecializationId = referralDTO.SpecializationId,
                State = "created"
            };

            if (referralDTO.SpecializationId != null)
            {
                newReferral.Specialization = await _specializationRepository.GetById(referralDTO.SpecializationId.Value);
            }

            _referralLetterRepository.Post(newReferral);
            _referralLetterRepository.Save();

            return parseToModel(newReferral);
        }
    }
}
