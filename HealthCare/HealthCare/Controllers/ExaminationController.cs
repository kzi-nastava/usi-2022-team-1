﻿using System.Diagnostics.Eventing.Reader;
using HealthCare.Domain.Interfaces;
using HealthCare.Domain.Models;
using HealthCare.Domain.Models.ModelsForCreate;
using HealthCare.Domain.Models.ModelsForUpdate;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareAPI.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class ExaminationController : ControllerBase {
        private IExaminationService _examinationService;

        public ExaminationController(IExaminationService examinationService) {
            _examinationService = examinationService;
        }

        // https://localhost:7195/api/examination
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExaminationDomainModel>>> GetAll() {
            IEnumerable<ExaminationDomainModel> examinations = await _examinationService.GetAll();
            if (examinations == null) {
                examinations = new List<ExaminationDomainModel>();
            }
            return Ok(examinations);
        }

        // https://localhost:7195/api/examination/patientId=___
        [HttpGet]
        [Route("patientId={id}")]
        public async Task<ActionResult<IEnumerable<ExaminationDomainModel>>> GetAllExaminationsForPatient(decimal id) {
            IEnumerable<ExaminationDomainModel> examinations = await _examinationService.GetAllForPatient(id);
            if (examinations == null) {
                examinations = new List<ExaminationDomainModel>();
            }
            return Ok(examinations);
        }

        [HttpGet]
        [Route("doctorId={id}")]
        public async Task<ActionResult<IEnumerable<ExaminationDomainModel>>> GetAllExaminationsForDoctor(decimal id)
        {
            IEnumerable<ExaminationDomainModel> examinations = await _examinationService.GetAllForDoctor(id);
            if (examinations == null)
            {
                examinations = new List<ExaminationDomainModel>();
            }
            return Ok(examinations);
        }

        // https://localhost:7195/api/examination/delete
        [HttpPut]
        [Route("delete")]
        public async Task<ActionResult<PatientDomainModel>> DeleteExamination([FromBody] DeleteExaminationDomainModel id) {
            var deletedPatientModel = await _examinationService.Delete(id);
            return Ok(deletedPatientModel);
        }


        [HttpPost]
        [Route("create")]
        public async Task<ActionResult<CreateExaminationDomainModel>> CreateExamination([FromBody] CreateExaminationDomainModel examinationModel) {
            var createdExaminationModel = await _examinationService.Create(examinationModel);
            return Ok(createdExaminationModel);
        }

        [HttpPut]
        [Route("update")]
        public async Task<ActionResult<UpdateExaminationDomainModel>> UpdateExamination([FromBody] UpdateExaminationDomainModel examinationModel) {
            var updatedExaminationModel = await _examinationService.Update(examinationModel);
            return Ok(updatedExaminationModel);
        }




    }
}
