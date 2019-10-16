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

        protected const int defaultTaskCount = 5;
        protected const int defaultBlockSize = 1000;

        public QueryDatabase(IRepositoryFactory repositoryFactory, IQueryScope scope)
        {
            this.repositoryFactory = repositoryFactory;
            this.scope = scope;
        }
        private IEnumerable<IEnumerable<T>> split<T>(IEnumerable<T> entities)
        {
            for (int count = 0; count < entities.Count(); count += defaultBlockSize)
                yield return entities.Skip(count).Take(defaultBlockSize);
        }
        private async Task<int> parallelQueueAsync<T>(IEnumerable<IEnumerable<T>> entities, Func<IEnumerable<T>, Task> blockActionAsync)
        {
            int blockCount = 0;
            using (var semaphore = new SemaphoreSlim(defaultTaskCount))
            {
                // обрабатываем данные
                foreach (var block in entities)
                {
                    semaphore.Wait();
                    await blockActionAsync(block).ConfigureAwait(false);
                    blockCount++;
                    semaphore.Release();
                }
                // ждем освобождения ресурсов
                for (int task = 0; task < defaultTaskCount; task++)
                    await semaphore.WaitAsync().ConfigureAwait(false);
            }
            return blockCount;
        }
        private async Task ExecuteSqlAsync(string query, string json)
        {
            using (var repository = repositoryFactory.GetRepository())
                await repository.ExecuteSqlAsync(query, json);
        }
        public async Task ExecuteSqlAsync(params SqlQuery[] queryList)
        {
            using (var repository = repositoryFactory.GetRepository())
                await repository.ExecuteSqlAsync(queryList);
        }
        public async Task ExecuteSqlAsync(SqlQuery query)
        {
            using (var repository = repositoryFactory.GetRepository())
                await repository.ExecuteSqlAsync(query.Query, query.Json);
        }
        public SqlQuery CreateTableToSql<T>() => new SqlQuery(String.Join(";", scope.query<T>().BuildCreateTable(), scope.query<T>().BuildCreateNonClusteredIndexs()));
        public SqlQuery TruncateTableToSql<T>() => new SqlQuery(scope.query<T>().BuildTruncateTable());
        public SqlQuery DropTableToSql<T>() => new SqlQuery(scope.query<T>().BuildDropTable());
        public SqlQuery AddToSql<T>(IEnumerable<T> entities) => (entities != null) ? new SqlQuery(scope.query<T>().BuildInsert(), JsonConvert.SerializeObject(entities, scope.query<T>().JsonSettingsForInsert())) : null;
        public SqlQuery UpdateToSql<T>(IEnumerable<T> entities) => (entities != null) ? new SqlQuery(scope.query<T>().BuildUpdate(), JsonConvert.SerializeObject(entities, scope.query<T>().JsonSettingsForUpdate())) : null;
        public SqlQuery UpdateToSql<T>(IEnumerable<T> entities, string[] properties) => (entities != null) ? new SqlQuery(scope.query<T>().BuildUpdate(properties), JsonConvert.SerializeObject(entities, scope.query<T>().JsonSettingsForUpdate(properties))) : null;
        public SqlQuery RemoveToSql<T>(IEnumerable<T> entities) => (entities != null) ? new SqlQuery(scope.query<T>().BuildDelete(), JsonConvert.SerializeObject(entities, scope.query<T>().JsonSettingsForDelete())) : null;
        public SqlQuery RemoveToSql<T>(IEnumerable<T> entities, string[] properties) => (entities != null) ? new SqlQuery(scope.query<T>().BuildDelete(properties), JsonConvert.SerializeObject(entities, scope.query<T>().JsonSettingsForDelete(properties))) : null;

        public Task AddAsync<T>(IEnumerable<T> objects) => ExecuteSqlAsync(scope.query<T>().BuildInsert(), JsonConvert.SerializeObject(objects, scope.query<T>().JsonSettingsForInsert()));
        public Task UpdateAsync<T>(IEnumerable<T> objects, string[] properties) => ExecuteSqlAsync(scope.query<T>().BuildUpdate(properties), JsonConvert.SerializeObject(objects, scope.query<T>().JsonSettingsForUpdate(properties)));
        public Task RemoveAsync<T>(IEnumerable<T> objects) => ExecuteSqlAsync(scope.query<T>().BuildDelete(), JsonConvert.SerializeObject(objects, scope.query<T>().JsonSettingsForDelete()));
        public Task<int> BulkAddAsync<T>(IEnumerable<T> objects) => parallelQueueAsync(split(objects), entities => ExecuteSqlAsync(scope.query<T>().BuildInsert(), JsonConvert.SerializeObject(entities, scope.query<T>().JsonSettingsForInsert())));
        public Task<int> BulkUpdateAsync<T>(IEnumerable<T> objects, string[] properties) => parallelQueueAsync(split(objects), entities => ExecuteSqlAsync(scope.query<T>().BuildUpdate(properties), JsonConvert.SerializeObject(entities, scope.query<T>().JsonSettingsForUpdate(properties))));
        public Task<int> BulkRemoveAsync<T>(IEnumerable<T> objects) => parallelQueueAsync(split(objects), entities => ExecuteSqlAsync(scope.query<T>().BuildDelete(), JsonConvert.SerializeObject(entities, scope.query<T>().JsonSettingsForDelete())));
    }
}
