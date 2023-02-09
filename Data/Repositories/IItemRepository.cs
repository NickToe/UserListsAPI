using UserListsAPI.Data.Enums;

namespace UserListsAPI.Data.Repositories;

public interface IItemRepository<T>
{
  public Task<T?> GetByIdAsync(string id);
  public Task<T?> GetByTitleAsync(string title);
  public Task<IEnumerable<T>> GetAllByTitleAsync(string title, int maxItems);
  public void Add(T item);
  public Task<bool> AnyAsync(string id);
  public Task UpdateAllAsync(IEnumerable<T> items);
  public Task<int> SaveChangesAsync();
  public Task<T?> FindAsync(string id);
}
