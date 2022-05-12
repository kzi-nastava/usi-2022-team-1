using HealthCare.Data.Entities;
using HealthCare.Domain.Models;

namespace HealthCare.Domain.Services;

public class AllergyService
{
    public static AllergyDomainModel parseToModel(Allergy allergy)
    {
        AllergyDomainModel allergyModel = new AllergyDomainModel
        {
            IngredientId = allergy.IngredientId,
            PatientId = allergy.PatientId
        };
        
        return allergyModel;
    }
    
    public static Allergy parseFromModel(AllergyDomainModel allergyModel)
    {
        Allergy allergy = new Allergy
        {
            IngredientId = allergyModel.IngredientId,
            PatientId = allergyModel.PatientId
        };
        
        return allergy;
    }
}