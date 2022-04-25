﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthCare.Domain.Models.ModelsForUpdate {
    public class UpdateExaminationDomainModel {
        public decimal oldDoctorId { get; set; }

        public decimal oldRoomId { get; set; }

        public decimal oldPatientId { get; set; }

        public DateTime oldStartTime { get; set;}

        public decimal newDoctorId { get; set; }

        public decimal newRoomId { get; set; }

        public decimal newPatientId { get; set; }

        public DateTime newStartTime { get; set; }

        public bool isPatient { get; set; }
    }
}
        