﻿using HealthCare.Domain.DTOs;
using HealthCare.Domain.Interfaces;
using HealthCare.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DrugSuggestionController : Controller
    {
        private IDrugSuggestionService _drugSuggestionService;
        public DrugSuggestionController(IDrugSuggestionService drugSuggestionService)
        {
            _drugSuggestionService = drugSuggestionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DrugSuggestionDomainModel>>> GetAll()
        {
            IEnumerable<DrugSuggestionDomainModel> anamnesis = await _drugSuggestionService.GetAll();
            return Ok(anamnesis);
        }

        [HttpGet]
        [Route("pending")]
        public async Task<ActionResult<IEnumerable<DrugSuggestionDomainModel>>> GetPending()
        {
            IEnumerable<DrugSuggestionDomainModel> suggestions = await _drugSuggestionService.GetPending();
            return Ok(suggestions);
        }

        [HttpPost]
        [Route("create")]
        public async Task<ActionResult<DrugSuggestionDomainModel>> Create([FromQuery] DrugSuggestionDTO drugSuggestionDTO)
        {
            DrugSuggestionDomainModel DrugSuggestion = await _drugSuggestionService.Create(drugSuggestionDTO);
            return Ok(DrugSuggestion);
        }

        [HttpPut]
        [Route("delete")]
        public async Task<ActionResult<DrugSuggestionDomainModel>> Delete([FromQuery] decimal drugSuggestionId)
        {
            DrugSuggestionDomainModel DrugSuggestion = await _drugSuggestionService.Delete(drugSuggestionId);
            return Ok(DrugSuggestion);
        }

        [HttpPut]
        [Route("approve")]
        public async Task<ActionResult<DrugSuggestionDomainModel>> Approve([FromQuery] decimal drugSuggestionId)
        {
            DrugSuggestionDomainModel DrugSuggestion = await _drugSuggestionService.Approve(drugSuggestionId);
            return Ok(DrugSuggestion);
        }

        [HttpPut]
        [Route("revision")]
        public async Task<ActionResult<DrugSuggestionDomainModel>> Revision([FromQuery] decimal drugSuggestionId, string comment)
        {
            DrugSuggestionDomainModel DrugSuggestion = await _drugSuggestionService.Revision(drugSuggestionId, comment);
            return Ok(DrugSuggestion);
        }
    }
}
