namespace UserListsAPI.ServiceLayer;

public interface IItemService<T>
{
  public Task<T?> GetById(string id);
  public Task<IEnumerable<T>> GetAllByIds(IEnumerable<string> ids);
  public Task<T?> GetByExactTitle(string title);
  public Task<IEnumerable<T>> GetAllByTitle(string title, int maxItems);
  public Task UpdateAll();
}
