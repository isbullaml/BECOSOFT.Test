using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Validation;

namespace BECOSOFT.Data.Services.Interfaces {
    internal interface ISave<T> : IBaseService where T : BaseEntity {
        MultiValidationResult<T> Save(ISaveContainer<T> saveContainer, bool bypassValidation = false);
    }
}