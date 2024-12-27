using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace DocumentDBClient
{
    public class Mongo<TEntity> : IDocument<TEntity> where TEntity : IBaseEntity
    {
        #region Variable & Constructor
        private readonly IMongoCollection<TEntity> _collection;

        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }

        public string CollectionName { get; set; }

        public Mongo()
        {
            var client = new MongoClient(ConnectionString);
            var database = client.GetDatabase(DatabaseName);
            _collection = database.GetCollection<TEntity>(CollectionName);
        }

        public Mongo(string connectionString, string databaseName, string collectionName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<TEntity>(collectionName);
        }
        #endregion

        #region Public Operation Methods
        public TEntity Insert(TEntity entity)
        {
            _collection.InsertOne(entity);
            return entity;
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            await _collection.InsertOneAsync(entity);
            return entity;
        }

        public TEntity Update(TEntity entity)
        {
            _collection.ReplaceOne(document => document.Id == entity.Id, entity);
            return entity;
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            await _collection.ReplaceOneAsync(document => document.Id == entity.Id, entity);
            return entity;
        }

        public void Update(UpdateSet updateSet)
        {
            if (MyConvert.ToString(updateSet.FilterId) != string.Empty)
                _collection.UpdateOne(document => document.Id == updateSet.FilterId, updateSet.GetUpdateDefinition<TEntity>());
            else if (updateSet.Filter != null)
                _collection.UpdateMany(updateSet.Filter.ToString(), updateSet.GetUpdateDefinition<TEntity>());
            else
                _collection.UpdateMany(Builders<TEntity>.Filter.Empty, updateSet.GetUpdateDefinition<TEntity>());
        }

        public async Task UpdateAsync(UpdateSet updateSet)
        {
            if (MyConvert.ToString(updateSet.FilterId) != string.Empty)
                await _collection.UpdateOneAsync(document => document.Id == updateSet.FilterId, updateSet.GetUpdateDefinition<TEntity>());
            else if (updateSet.Filter != null)
                await _collection.UpdateManyAsync(updateSet.Filter.ToString(), updateSet.GetUpdateDefinition<TEntity>());
            else
                await _collection.UpdateManyAsync(Builders<TEntity>.Filter.Empty, updateSet.GetUpdateDefinition<TEntity>());
        }

        public void Delete(string id)
        {
            _collection.DeleteOne(document => document.Id == id);
        }

        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(document => document.Id == id);
        }

        public void Delete(TEntity entity)
        {
            _collection.DeleteOne(document => document.Id == entity.Id);
        }

        public async Task DeleteAsync(TEntity entity)
        {
            await _collection.DeleteOneAsync(document => document.Id == entity.Id);
        }

        public void Delete(Filter filter)
        {
            _collection.DeleteMany(filter.ToString());
        }
        
        public async Task DeleteAsync(Filter filter)
        {
            await _collection.DeleteManyAsync(filter.ToString());
        }
        #endregion

        #region Public Get Methods
        public bool IsExist(string id)
        {
            return (_collection.CountDocuments(document => document.Id == id) > 0);
        }

        public async Task<bool> IsExistAsync(string id)
        {
            return ((await _collection.CountDocumentsAsync(document => document.Id == id)) > 0);
        }

        public bool IsExist(Filter filter)
        {
            return (_collection.CountDocuments(filter.ToString()) > 0);
        }

        public async Task<bool> IsExistAsync(Filter filter)
        {
            return ((await _collection.CountDocumentsAsync(filter.ToString())) > 0);
        }

        public TEntity GetById(string id)
        {
            return _collection.Find<TEntity>(document => document.Id == id).FirstOrDefault();
        }

        public async Task<TEntity> GetByIdAsync(string id)
        {
            var result = await _collection.FindAsync<TEntity>(document => document.Id == id);
            return result.SingleOrDefault();
        }

        public List<TEntity> GetAll()
        {
            return _collection.Find(document => true).ToList();
        }

        public async Task<List<TEntity>> GetAllAsync()
        {
            var result = await _collection.FindAsync(document => true);
            return result.ToList();
        }

        public List<TEntity> GetAll(Filter filter)
        {
            return _collection.Find(filter.ToString()).ToList();
        }

        public async Task<List<TEntity>> GetAllAsync(Filter filter)
        {
            var result = await _collection.FindAsync(filter.ToString());
            return result.ToList();
        }

        public List<TEntity> GetAll(Filter filter, List<Sort> sorts)
        {
            if (sorts.Any())
            {
                var comineSortDefinitions = Sort.GetSortDefinitions<TEntity>(sorts);
                return _collection.Find(filter.ToString()).Sort(comineSortDefinitions).ToList();
            }
            else
            {
                return _collection.Find(filter.ToString()).ToList();
            }
        }

        public List<TEntity> GetAll(Filter filter, Sort sort)
        {
            List<Sort> sorts = new List<Sort>();
            if (sort != null && MyConvert.ToString(sort.Name) != string.Empty)
                sorts.Add(sort);
            return GetAll(filter, sorts);
        }

        public async Task<List<TEntity>> GetAllAsync(Filter filter, Sort sort)
        {
            var result = await _collection.FindAsync(filter.ToString(), filter.GetOptions<TEntity>(sort));
            return result.ToList();
        }

        public List<TEntity> GetPageData(int pageNo, int pageSize, Filter filter, List<Sort> sorts)
        {
            int skipRecords = (pageNo - 1) * pageSize;
            if (sorts.Any())
            {
                var comineSortDefinitions = Sort.GetSortDefinitions<TEntity>(sorts);
                return _collection.Find(filter.ToString()).Sort(comineSortDefinitions).Skip(skipRecords).Limit(pageSize).ToList();
            }
            else
            {
                return _collection.Find(filter.ToString()).Skip(skipRecords).Limit(pageSize).ToList();
            }
        }

        public List<TEntity> GetPageData(int pageNo, int pageSize, Filter filter, Sort sort)
        {
            List<Sort> sorts = new List<Sort>();
            if (sort != null && MyConvert.ToString(sort.Name) != string.Empty)
                sorts.Add(sort);
            return GetPageData(pageNo, pageSize, filter, sorts);
        }

        public async Task<List<TEntity>> GetPageDataAsync(int pageNo, int pageSize, Filter filter, Sort sort)
        {
            var result = await _collection.FindAsync(filter.ToString(), filter.GetOptions<TEntity>(sort, pageNo, pageSize));
            return result.ToList();
        }

        public long GetCount(Filter filter)
        {
            return _collection.CountDocuments(filter.ToString());
        }

        public async Task<long> GetCountAsync(Filter filter)
        {
            return await _collection.CountDocumentsAsync(filter.ToString());
        }
        #endregion
    }

    public class BaseEntity : IBaseEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

    }
}
