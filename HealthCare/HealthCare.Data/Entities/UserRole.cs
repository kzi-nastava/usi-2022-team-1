﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthCare.Data.Entities
{
    [Table("user_role")]
    public class UserRole
    {
        [Column("id")]
        public decimal Id { get; set; }  

        [Column("name")]
        public string RoleName { get; set; }

        [Column("deleted")]
        public bool IsDeleted { get; set; }

        //public List<Credentials> Credentials { get; set; }
    }
}
