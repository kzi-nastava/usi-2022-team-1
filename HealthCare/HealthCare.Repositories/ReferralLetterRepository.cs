﻿using HealthCare.Data.Context;
using HealthCare.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthCare.Repositories
{
    public interface IReferralLetterRepository : IRepository<ReferralLetter>
    {

    }
    public class ReferralLetterRepository : IReferralLetterRepository
    {
        private readonly HealthCareContext _healthCareContext;

        public ReferralLetterRepository(HealthCareContext healthCareContext)
        {
            _healthCareContext = healthCareContext;
        }

        public async Task<IEnumerable<ReferralLetter>> GetAll()
        {
            return await _healthCareContext.ReferralLetters.ToListAsync();
        }

        public void Save()
        {
            _healthCareContext.SaveChanges();
        }
    }
}
