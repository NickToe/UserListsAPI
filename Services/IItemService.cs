namespace UserListsAPI.Services;

public interface IItemService<T>
{
  public Task<T?> GetByIdAsync(string id);
  public Task<IEnumerable<T>> GetAllByIdsAsync(IEnumerable<string> ids);
  public Task<T?> GetByExactTitleAsync(string title);
  public Task<IEnumerable<T>> GetAllByTitleAsync(string title, int maxItems);
  public Task UpdateAllAsync();
}
