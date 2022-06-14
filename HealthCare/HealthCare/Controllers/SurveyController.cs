﻿using HealthCare.Domain.Interfaces;
using HealthCare.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SurveyController : Controller
    {
        private ISurveyService _surveyService;
        public SurveyController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
        }

        [HttpGet]
        [Route("hospital/stats")]
        public async Task<ActionResult<IEnumerable<AnswerStatsDomainModel>>> GetHospitalStats()
        {
            IEnumerable<AnswerStatsDomainModel> result = await _surveyService.GetHospitalStats();
            return Ok(result);
        }


        [HttpGet]
        [Route("doctor/stats")]
        public async Task<ActionResult<IEnumerable<AnswerStatsDomainModel>>> GetDoctorStats(decimal doctorId)
        {
            IEnumerable<AnswerStatsDomainModel> result = await _surveyService.GetDoctorStats(doctorId);
            return Ok(result);
        }
    }
}
