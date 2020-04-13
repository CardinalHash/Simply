using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Simply.Property.SqlServer
{
    internal class QueryDatabase : IQueryDatabase
    {
        private readonly IQueryScope scope;
        private readonly IRepositoryFactory repositoryFactory;

        public int DefaultTaskCount { get; set; } = 3;
        public int DefaultBlockSize { get; set; } = 25000;

        public QueryDatabase(IRepositoryFactory repositoryFactory, IQueryScope scope)
        {
            this.repositoryFactory = repositoryFactory;
            this.scope = scope;
        }
        private IEnumerable<IEnumerable<T>> split<T>(IEnumerable<T> entities)
        {
            for (int count = 0; count < entities.Count(); count += DefaultBlockSize)
            {
                yield return entities.Skip(count).Take(DefaultBlockSize);
            }
        }
        private async Task<int> parallelQueueAsync<T>(IEnumerable<IEnumerable<T>> entities, Func<IEnumerable<T>, Task> blockActionAsync)
        {
            int blockCount = 0;
            using (var semaphore = new SemaphoreSlim(DefaultTaskCount))
            {
                // обрабатываем данные
                foreach (var block in entities)
                {
                    semaphore.Wait();
                    try
                    {
                        await blockActionAsync(block).ConfigureAwait(false);
                        blockCount++;
                    }
                    finally 
                    {
                        semaphore.Release();
                    }
                }
                // ждем освобождения ресурсов
                for (int task = 0; task < DefaultTaskCount; task++)
                {
                    await semaphore.WaitAsync().ConfigureAwait(false);
                }
            }
            return blockCount;
        }
        private async Task<int> ExecuteSqlAsync(string query, string jsonData)
        {
            using (var repository = repositoryFactory.GetRepository())
            {
                return await repository.ExecuteSqlAsync(new SqlServerQuery(query, jsonData));
            }
        }
        public async Task<bool> ExecuteSqlAsync(params SqlServerQuery[] queryList)
        {
            using (var repository = repositoryFactory.GetRepository())
            {
                return await repository.ExecuteSqlAsync(queryList);
            }
        }
        public async Task<int> ExecuteSqlAsync(SqlServerQuery query)
        {
            using (var repository = repositoryFactory.GetRepository())
            {
                return await repository.ExecuteSqlAsync(query);
            }
        }
        public SqlServerQuery CreateSqlQuery(string query, string json = null) => new SqlServerQuery(query, json);

        public SqlServerQuery CreateTableToSql<T>() => new SqlServerQuery(string.Join(";", scope.Query<T>().BuildCreateTable(), scope.Query<T>().BuildCreateNonClusteredIndexs()));
        public SqlServerQuery TruncateTableToSql<T>() => new SqlServerQuery(scope.Query<T>().BuildTruncateTable());
        public SqlServerQuery DropTableToSql<T>() => new SqlServerQuery(scope.Query<T>().BuildDropTable());

        public SqlServerQuery AddToSql<T>(string json) => (json != null) ? new SqlServerQuery(scope.Query<T>().BuildInsert(), json) : null;
        public SqlServerQuery AddToSql<T>(IEnumerable<T> entities) => (entities != null) ? new SqlServerQuery(scope.Query<T>().BuildInsert(), JsonConvert.SerializeObject(entities, scope.Query<T>().JsonSettingsForInsert())) : null;
        public SqlServerQuery UpdateToSql<T>(IEnumerable<T> entities) => (entities != null) ? new SqlServerQuery(scope.Query<T>().BuildUpdate(), JsonConvert.SerializeObject(entities, scope.Query<T>().JsonSettingsForUpdate())) : null;
        public SqlServerQuery UpdateToSql<T>(IEnumerable<T> entities, string[] properties) => (entities != null) ? new SqlServerQuery(scope.Query<T>().BuildUpdate(properties), JsonConvert.SerializeObject(entities, scope.Query<T>().JsonSettingsForUpdate(properties))) : null;
        public SqlServerQuery RemoveToSql<T>(IEnumerable<T> entities) => (entities != null) ? new SqlServerQuery(scope.Query<T>().BuildDelete(), JsonConvert.SerializeObject(entities, scope.Query<T>().JsonSettingsForDelete())) : null;
        public SqlServerQuery RemoveToSql<T>(IEnumerable<T> entities, string[] properties) => (entities != null) ? new SqlServerQuery(scope.Query<T>().BuildDelete(properties), JsonConvert.SerializeObject(entities, scope.Query<T>().JsonSettingsForDelete(properties))) : null;

        public Task CreateTableAsync<T>() => ExecuteSqlAsync(CreateTableToSql<T>());
        public Task TruncateTableAsync<T>() => ExecuteSqlAsync(TruncateTableToSql<T>());
        public Task DropTableAsync<T>() => ExecuteSqlAsync(DropTableToSql<T>());

        public Task AddAsync<T>(string json) => ExecuteSqlAsync(scope.Query<T>().BuildInsert(), json);
        public Task AddAsync<T>(IEnumerable<T> objects) => ExecuteSqlAsync(scope.Query<T>().BuildInsert(), JsonConvert.SerializeObject(objects, scope.Query<T>().JsonSettingsForInsert()));
        public Task UpdateAsync<T>(IEnumerable<T> objects) => ExecuteSqlAsync(scope.Query<T>().BuildUpdate(), JsonConvert.SerializeObject(objects, scope.Query<T>().JsonSettingsForUpdate()));
        public Task UpdateAsync<T>(IEnumerable<T> objects, string[] properties) => ExecuteSqlAsync(scope.Query<T>().BuildUpdate(properties), JsonConvert.SerializeObject(objects, scope.Query<T>().JsonSettingsForUpdate(properties)));
        public Task RemoveAsync<T>(IEnumerable<T> objects) => ExecuteSqlAsync(scope.Query<T>().BuildDelete(), JsonConvert.SerializeObject(objects, scope.Query<T>().JsonSettingsForDelete()));
        public Task RemoveAsync<T>(IEnumerable<T> objects, string[] properties) => ExecuteSqlAsync(scope.Query<T>().BuildDelete(properties), JsonConvert.SerializeObject(objects, scope.Query<T>().JsonSettingsForDelete(properties)));

        public Task<int> BulkAddAsync<T>(IEnumerable<T> objects) => parallelQueueAsync(split(objects), entities => ExecuteSqlAsync(scope.Query<T>().BuildInsert(), JsonConvert.SerializeObject(entities, scope.Query<T>().JsonSettingsForInsert())));
        public Task<int> BulkUpdateAsync<T>(IEnumerable<T> objects) => parallelQueueAsync(split(objects), entities => ExecuteSqlAsync(scope.Query<T>().BuildUpdate(), JsonConvert.SerializeObject(entities, scope.Query<T>().JsonSettingsForUpdate())));
        public Task<int> BulkUpdateAsync<T>(IEnumerable<T> objects, string[] properties) => parallelQueueAsync(split(objects), entities => ExecuteSqlAsync(scope.Query<T>().BuildUpdate(properties), JsonConvert.SerializeObject(entities, scope.Query<T>().JsonSettingsForUpdate(properties))));
        public Task<int> BulkRemoveAsync<T>(IEnumerable<T> objects) => parallelQueueAsync(split(objects), entities => ExecuteSqlAsync(scope.Query<T>().BuildDelete(), JsonConvert.SerializeObject(entities, scope.Query<T>().JsonSettingsForDelete())));
    }
}
