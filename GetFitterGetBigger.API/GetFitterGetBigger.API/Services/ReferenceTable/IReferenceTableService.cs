namespace GetFitterGetBigger.API.Services.ReferenceTable;

public interface IReferenceTableService<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(string id);
    Task<T?> GetByNameAsync(string name);
    Task<T?> GetByValueAsync(string value);
    Task<T> CreateAsync(object createDto);
    Task<T> UpdateAsync(string id, object updateDto);
    Task DeleteAsync(string id);
}