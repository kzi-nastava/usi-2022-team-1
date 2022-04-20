using HealthCare.Data.Entities;

namespace HealthCare.Domain.Models;

public class SecretaryDomainModel
{
    public decimal Id { get; set; }

    public string Name { get; set; }

    public string Surname { get; set; }

    public string Email { get; set; }

    public DateTime BirthDate { get; set; }

    public string Phone { get; set; }

    public bool isDeleted { get; set; }

    public Credentials Credentials { get; set; }
}