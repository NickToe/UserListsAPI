using UserListsAPI.DataLayer.Enums;

namespace UserListsAPI.DataLayer.Repo;

public interface IItemRepo<T>
{
  public Task<T?> GetById(string id);
  public Task<T?> GetByTitle(string title);
  public Task<IEnumerable<T>> GetAllByTitle(string title);
  public Task<bool> Add(T item);
  public Task<bool> Any(string id);
  public Task<bool> UpdateSuccess(string id, ItemStatus itemStatus);
  public Task UpdateAll(IEnumerable<T> items);
}
