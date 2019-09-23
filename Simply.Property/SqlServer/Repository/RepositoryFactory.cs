using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Simply.Property.SqlServer
{
    public abstract class RepositoryFactory : IRepositoryFactory
    {
        protected readonly IQueryScope scope;
        protected const int defaultTaskCount = 5;
        protected const int defaultBlockSize = 1000;
        public RepositoryFactory(IQueryScope scope)
        {
            this.scope = scope;
        }
        public async Task ExecuteSqlWithTransactionAsync(params RepositorySqlQuery[] queryList)
        {
            using (var repository = GetRepository())
                await repository.ExecuteSqlWithTransactionAsync(queryList);
        }
        public async Task ExecuteSqlAsync(RepositorySqlQuery query)
        {
            using (var repository = GetRepository())
                await repository.ExecuteSqlAsync(query);
        }
        public async Task ExecuteSqlAsync(string sql)
        {
            using (var repository = GetRepository())
                await repository.ExecuteSqlAsync(sql);
        }
        public async Task ExecuteSqlWithJsonAsync(string sql, string json)
        {
            using (var repository = GetRepository())
                await repository.ExecuteSqlWithJsonAsync(sql, json);
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
                    await semaphore.WaitAsync();
            }
            return blockCount;
        }
        public Task CreateTableAsync<T>() => ExecuteSqlAsync(String.Join(";", scope.query<T>().BuildDropTable(), scope.query<T>().BuildCreateTable(), scope.query<T>().BuildCreateNonClusteredIndexs()));
        public Task TruncateTableAsync<T>() => ExecuteSqlAsync(scope.query<T>().BuildTruncate());
        public Task<int> BulkUpdateAsync<T>(IEnumerable<T> objects, string[] properties) => parallelQueueAsync(split(objects), entities => ExecuteSqlWithJsonAsync(scope.query<T>().BuildUpdate(properties), JsonConvert.SerializeObject(entities, scope.query<T>().JsonSettingsForUpdate(properties))));
        public Task<int> BulkAddAsync<T>(IEnumerable<T> objects) => parallelQueueAsync(split(objects), entities => ExecuteSqlWithJsonAsync(scope.query<T>().BuildInsert(), JsonConvert.SerializeObject(entities, scope.query<T>().JsonSettingsForInsert())));
        public Task<int> BulkRemoveAsync<T>(IEnumerable<T> objects) => parallelQueueAsync(split(objects), entities => ExecuteSqlWithJsonAsync(scope.query<T>().BuildDelete(), JsonConvert.SerializeObject(entities, scope.query<T>().JsonSettingsForRemove())));
        public Task AddAsync<T>(IEnumerable<T> objects) => ExecuteSqlWithJsonAsync(scope.query<T>().BuildInsert(), JsonConvert.SerializeObject(objects, scope.query<T>().JsonSettingsForInsert()));
        public Task UpdateAsync<T>(IEnumerable<T> objects, string[] properties) => ExecuteSqlWithJsonAsync(scope.query<T>().BuildUpdate(properties), JsonConvert.SerializeObject(objects, scope.query<T>().JsonSettingsForUpdate(properties)));
        public Task RemoveAsync<T>(IEnumerable<T> objects) => ExecuteSqlWithJsonAsync(scope.query<T>().BuildDelete(), JsonConvert.SerializeObject(objects, scope.query<T>().JsonSettingsForRemove()));
        public RepositorySqlQuery CreateTableToSql<T>() => new RepositorySqlQuery(String.Join(";", scope.query<T>().BuildDropTable(), scope.query<T>().BuildCreateTable(), scope.query<T>().BuildCreateNonClusteredIndexs()));
        public RepositorySqlQuery AddToSql<T>(IEnumerable<T> entities) => (entities != null) ? new RepositorySqlQuery(scope.query<T>().BuildInsert(), JsonConvert.SerializeObject(entities, scope.query<T>().JsonSettingsForInsert())) : null;
        public RepositorySqlQuery UpdateToSql<T>(IEnumerable<T> entities, string[] updateProperties) => (entities != null) ? new RepositorySqlQuery(scope.query<T>().BuildUpdate(updateProperties), JsonConvert.SerializeObject(entities, scope.query<T>().JsonSettingsForUpdate(updateProperties))) : null;
        public RepositorySqlQuery RemoveToSql<T>(IEnumerable<T> entities) => (entities != null) ? new RepositorySqlQuery(scope.query<T>().BuildDelete(), JsonConvert.SerializeObject(entities, scope.query<T>().JsonSettingsForRemove())) : null;
        public Task AddWithTransactionAsync<T>(IEnumerable<T> objects)
        {
            if (objects == null) return null;
            return ExecuteSqlWithTransactionAsync(
                new RepositorySqlQuery(scope.query<T>().BuildCreateTable()),
                new RepositorySqlQuery(scope.query<T>().BuildInsert(), JsonConvert.SerializeObject(objects, scope.query<T>().JsonSettingsForInsert())));
        }
        public Task UpdateWithTransactionAsync<T>(IEnumerable<T> objects, string[] properties)
        {
            if (objects == null) return null;
            if (properties == null) return null;
            return ExecuteSqlWithTransactionAsync(
                new RepositorySqlQuery(scope.query<T>().BuildCreateTable()),
                new RepositorySqlQuery(scope.query<T>().BuildUpdate(properties), JsonConvert.SerializeObject(objects, scope.query<T>().JsonSettingsForUpdate(properties))));
        }
        public Task RemoveWithTransactionAsync<T>(IEnumerable<T> objects)
        {
            if (objects == null) return null;
            return ExecuteSqlWithTransactionAsync(
                new RepositorySqlQuery(scope.query<T>().BuildCreateTable()),
                new RepositorySqlQuery(scope.query<T>().BuildDelete(), JsonConvert.SerializeObject(objects, scope.query<T>().JsonSettingsForRemove())));
        }
        public abstract IRepository GetRepository();
    }
}
