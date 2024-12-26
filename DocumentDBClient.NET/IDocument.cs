namespace DocumentDBClient
{
    public interface IDocument<TEntity>
    {
        #region Property
        string ConnectionString { get; set; }

        string DatabaseName { get; set; }

        string CollectionName { get; set; }
        #endregion

        #region Operation Methods
        TEntity Insert(TEntity entity);

        Task<TEntity> InsertAsync(TEntity entity);

        TEntity Update(TEntity entity);

        Task<TEntity> UpdateAsync(TEntity entity);

        void Delete(string id);

        Task DeleteAsync(string id);

        void Delete(TEntity entity);

        Task DeleteAsync(TEntity entity);

        void Delete(Filter filter);

        Task DeleteAsync(Filter filter);

        #endregion

        #region Get Methods
        bool IsExist(string id);

        Task<bool> IsExistAsync(string id);
        
        bool IsExist(Filter filter);

        Task<bool> IsExistAsync(Filter filter);
        
        TEntity GetById(string id);

        Task<TEntity> GetByIdAsync(string id);

        List<TEntity> GetAll();

        Task<List<TEntity>> GetAllAsync();

        List<TEntity> GetAll(Filter filter);

        Task<List<TEntity>> GetAllAsync(Filter filter);

        List<TEntity> GetAll(Filter filter, List<Sort> sorts);
        
        List<TEntity> GetAll(Filter filter, Sort sort);

        Task<List<TEntity>> GetAllAsync(Filter filter, Sort sort);

        List<TEntity> GetPageData(int pageNo, int pageSize, Filter filter, List<Sort> sorts);

        List<TEntity> GetPageData(int pageNo, int pageSize, Filter filter, Sort sort);

        Task<List<TEntity>> GetPageDataAsync(int pageNo, int pageSize, Filter filter, Sort sort);

        long GetCount(Filter filter);

        Task<long> GetCountAsync(Filter filter);

        #endregion
    }

    public interface IBaseEntity
    {
        string Id { get; set; }
    }

}
