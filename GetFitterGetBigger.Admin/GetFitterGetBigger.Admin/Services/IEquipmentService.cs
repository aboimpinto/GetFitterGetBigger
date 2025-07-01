using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services
{
    public interface IEquipmentService
    {
        Task<IEnumerable<EquipmentDto>> GetEquipmentAsync();
        Task<EquipmentDto> CreateEquipmentAsync(CreateEquipmentDto dto);
        Task<EquipmentDto> UpdateEquipmentAsync(string id, UpdateEquipmentDto dto);
        Task DeleteEquipmentAsync(string id);
    }
}