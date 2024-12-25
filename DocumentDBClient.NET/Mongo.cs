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

        #region Public Methods
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
            _collection.DeleteMany(getSearchCriteriaString(filter));
        }
        
        public async Task DeleteAsync(Filter filter)
        {
            await _collection.DeleteManyAsync(getSearchCriteriaString(filter));
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
            return _collection.Find(getSearchCriteriaString(filter)).ToList();
        }

        public async Task<List<TEntity>> GetAllAsync(Filter filter)
        {
            var result = await _collection.FindAsync(getSearchCriteriaString(filter));
            return result.ToList();
        }

        public List<TEntity> GetAll(Filter filter, List<Sort> sorts)
        {
            if (sorts.Any())
            {
                var comineSortDefinitions = getSortDefinitions(sorts);
                return _collection.Find(getSearchCriteriaString(filter)).Sort(comineSortDefinitions).ToList();
            }
            else
            {
                return _collection.Find(getSearchCriteriaString(filter)).ToList();
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
            var result = await _collection.FindAsync(getSearchCriteriaString(filter), getFindOptions(sort));
            return result.ToList();
        }

        public List<TEntity> GetPageData(int pageNo, int pageSize, Filter filter, List<Sort> sorts)
        {
            int skipRecords = (pageNo - 1) * pageSize;
            if (sorts.Any())
            {
                var comineSortDefinitions = getSortDefinitions(sorts);
                return _collection.Find(getSearchCriteriaString(filter)).Sort(comineSortDefinitions).Skip(skipRecords).Limit(pageSize).ToList();
            }
            else
            {
                return _collection.Find(getSearchCriteriaString(filter)).Skip(skipRecords).Limit(pageSize).ToList();
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
            var result = await _collection.FindAsync(getSearchCriteriaString(filter), getFindOptions(sort, pageNo, pageSize));
            return result.ToList();
        }

        public long GetCount(Filter filter)
        {
            return _collection.CountDocuments(getSearchCriteriaString(filter));
        }

        public async Task<long> GetCountAsync(Filter filter)
        {
            return await _collection.CountDocumentsAsync(getSearchCriteriaString(filter));
        }
        #endregion

        #region Private Methods
        private FindOptions<TEntity, TEntity> getFindOptions(Sort sort, int pageNo = 0, int pageSize = 0)
        {
            var findOptions = new FindOptions<TEntity, TEntity>();
            if (sort != null && MyConvert.ToString(sort.Name) != string.Empty)
            {
                var sortBuilders = Builders<TEntity>.Sort.Ascending(sort.Name);
                if (sort.Direction == SortDirection.Descending)
                    sortBuilders = Builders<TEntity>.Sort.Descending(sort.Name);
                findOptions = new FindOptions<TEntity, TEntity>()
                {
                    Sort = sortBuilders
                };
            }
            if (pageNo > 0 && pageSize > 0) 
            {
                int skipRecords = (pageNo - 1) * pageSize;
                findOptions.Skip = skipRecords;
                findOptions.Limit = pageSize;
            }
            return findOptions;
        }

        private SortDefinition<TEntity> getSortDefinitions(List<Sort> sorts)
        {
            if (sorts.Any())
            {
                var sortBuilders = Builders<TEntity>.Sort;
                var sortDefinitions = sorts.Select(x =>
                {
                    SortDefinition<TEntity> sortDefination;
                    if (x.Direction == SortDirection.Descending)
                        sortDefination = sortBuilders.Descending(x.Name);
                    else
                        sortDefination = sortBuilders.Ascending(x.Name);
                    return sortDefination;
                });
                return sortBuilders.Combine(sortDefinitions);
            }
            return null;
        }

        private string getSearchCriteriaString(Filter filter)
        {
            if (filter == null)
                return string.Empty;

            if ((filter.GroupConditions.Count > 0 && filter.GroupOperator != GroupOperator.NONE) && filter.Condition != null)
                throw new Exception("Parameter should not have single condition and group condition both.");

            if (!((filter.GroupConditions.Count > 0 && filter.GroupOperator != GroupOperator.NONE) || filter.Condition != null))
                throw new Exception("Parameter should have atleast single condition or group condition.");

            return getSearchCriteriaStringStartProcess(filter);
        }

        private string getSearchCriteriaStringStartProcess(Filter filter)
        {
            string searchCriteria = string.Empty;
            if (filter.Condition != null)
                searchCriteria = getSearchStringBySingleCondition(filter.Condition);
            else if (filter.GroupConditions.Count > 0 && filter.GroupOperator != GroupOperator.NONE)
                searchCriteria = getSearchStringByGroupCondition(filter.GroupConditions, filter.GroupOperator);
            return searchCriteria;
        }

        private string getOperationString(GroupOperator groupOperator)
        {
            switch (groupOperator)
            {
                case GroupOperator.AND:
                    return "$and";
                case GroupOperator.OR:
                    return "$or";
                case GroupOperator.NOT:
                    return "$not";
                case GroupOperator.NOR:
                    return "$nor";
            }
            return string.Empty;
        }

        private string getSearchStringByGroupCondition(List<Condition> groupConditions, GroupOperator groupOperator)
        {
            string searchCriteria = "{ " + getOperationString(groupOperator) + " : [ ";

            foreach (var groupCondition in groupConditions)
                searchCriteria += getSearchStringBySingleCondition(groupCondition) + ",";

            searchCriteria = searchCriteria.TrimEnd(',') + " ] }";

            return searchCriteria;
        }

        private string getSearchStringBySingleCondition(Condition condition)
        {
            string searchCriteria = "";

            if ((condition.GroupConditions.Count > 0 && condition.GroupOperator != GroupOperator.NONE) && MyConvert.ToString(condition.Parameter) != String.Empty)
                throw new Exception("Condition should not have single condition and group condition both.");

            if (!((condition.GroupConditions.Count > 0 && condition.GroupOperator != GroupOperator.NONE) || MyConvert.ToString(condition.Parameter) != String.Empty))
                throw new Exception("Condition should have atleast single condition or group condition.");

            if (condition.GroupConditions.Count > 0 && condition.GroupOperator != GroupOperator.NONE)
            {
                searchCriteria += getSearchStringByGroupCondition(condition.GroupConditions, condition.GroupOperator);
            }
            else
            {
                if (condition.Parameter != string.Empty)
                {
                    if (condition.Compare == CompareOperator.Equal)
                        searchCriteria += "{ \"" + condition.Parameter + "\" : { $eq: " + condition.DocumentValue + " } } ";
                    else if (condition.Compare == CompareOperator.NotEqual)
                        searchCriteria += "{ \"" + condition.Parameter + "\" : { $ne: " + condition.DocumentValue + " } } ";
                    else if (condition.Compare == CompareOperator.GreaterThan)
                        searchCriteria += "{ \"" + condition.Parameter + "\" : { $gt: " + condition.DocumentValue + " } } ";
                    else if (condition.Compare == CompareOperator.GreaterThanEqual)
                        searchCriteria += "{ \"" + condition.Parameter + "\" : { $gte: " + condition.DocumentValue + " } } ";
                    else if (condition.Compare == CompareOperator.LessThan)
                        searchCriteria += "{ \"" + condition.Parameter + "\" : { $lt: " + condition.DocumentValue + " } } ";
                    else if (condition.Compare == CompareOperator.LessThanEqual)
                        searchCriteria += "{ \"" + condition.Parameter + "\" : { $lte: " + condition.DocumentValue + " } } ";
                    else if (condition.Compare == CompareOperator.Contains)
                        searchCriteria += "{ \"" + condition.Parameter + "\" : { $regex: /.*" + condition.Value + ".*/, $options: 'im' } } ";
                    else if (condition.Compare == CompareOperator.BeginWith)
                        searchCriteria += "{ \"" + condition.Parameter + "\" : { $regex: /^" + condition.Value + "/, $options: 'im' } } ";
                    else if (condition.Compare == CompareOperator.EndWith)
                        searchCriteria += "{ \"" + condition.Parameter + "\" : { $regex: /" + condition.Value + "$/, $options: 'im' } } ";
                    else if (condition.Compare == CompareOperator.In)
                        searchCriteria += "{ \"" + condition.Parameter + "\" : { $in: " + condition.Value + " } } ";
                    else if (condition.Compare == CompareOperator.NotIn)
                        searchCriteria += "{ \"" + condition.Parameter + "\" : { $nin: " + condition.Value + " } } ";
                }
            }
            return searchCriteria;
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
