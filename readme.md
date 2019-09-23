
## NuGet installation

Install the [Simply.Property NuGet package](https://nuget.org/packages/Simply.Property/):

```powershell
PM> Install-Package Simply.Property
```

The `Install-Package Simply.Property` is required since NuGet always defaults to the oldest, and most buggy, version of any dependency.

### Step #1. Implement interface IRepository, Repository and RepositoryFactory

```csharp
public class Repository<T> : IRepository where T : DbContext
{
    protected T ctx;
    public Repository(T context) { ctx = context; ctx.Database.SetCommandTimeout(300); }
    public Task<int> ExecuteSqlAsync(string query) => ctx.Database.ExecuteSqlCommandAsync(query);
    public Task<int> ExecuteSqlWithJsonAsync(string query, string json) => ctx.Database.ExecuteSqlCommandAsync(query, new SqlParameter("@json", json));
    public Task<int> ExecuteSqlAsync(RepositorySqlQuery query) => (query.Json != null) ? ExecuteSqlWithJsonAsync(query.Query, query.Json) : ExecuteSqlAsync(query.Query);
    public async Task ExecuteSqlWithTransactionAsync(IEnumerable<RepositorySqlQuery> queries)
    {
        using (var transaction = await ctx.Database.BeginTransactionAsync().ConfigureAwait(false))
        {
            try
            {
                foreach (var query in queries)
                    await ExecuteSqlAsync(query).ConfigureAwait(false);
                transaction.Commit();
            }
            catch(SqlException)
            {
                transaction.Rollback();
            }
        }
    }
    public Task<List<TEntity>> GetAsync<TEntity>() where TEntity : class => ctx.Set<TEntity>().AsNoTracking().ToListAsync();
    public Task<int> AddAsync<TEntity>(TEntity entity) where TEntity : class
    {
        ctx.Add(entity);
        return SaveAsync();
    }
    public Task<int> AddManyAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
    {
        foreach (var entity in entities)
            ctx.Add(entity);
        return SaveAsync();
    }
    public Task<int> SaveAsync() => ctx.SaveChangesAsync();
    public void Dispose() => ctx.Dispose();
}

public class SampleRepository : Repository<SampleDbContext>, ISampleRepository
{
    public SampleRepository(SampleDbContext context) : base(context)
	{ 
	}
	
	// Here you code to access Database
}

public class SampleRepositoryFactory : RepositoryFactory, ISampleRepositoryFactory
{
    public SampleRepositoryFactory(IQueryScope queryScope) : base(queryScope)
	{ 
	}
    public ISampleRepository GetSampleRepository() => new SampleRepository(new SampleDbContext());
    public override IRepository GetRepository() => GetSampleRepository();
}
```

### Step #2. Data annotation

```csharp
[Table("OStatementBabyFile")]
[Schema("BABIES", "tempId")]
public class StatementBabyFileOriginalObject
{
    public StatementBabyFileOriginalObject()
    {
        RowCreated = DateTime.UtcNow;
    }
    [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public Guid tempId { get; set; }
    [XmlProperty("NRECORDS")]
    public int RowCount { get; set; }
    [XmlProperty("SMOCOD")]
    [MaxLength(10)]
    public string Smo { get; set; }
    [XmlProperty("FILENAME")]
    [MaxLength(50)]
    public string Name { get; set; }
    [XmlProperty("VERS")]
    [MaxLength(10)]
    public string Version { get; set; }
    public DateTime? RowCreated { get; set; }
}

[Table("OStatementBabyEntry")]
[Schema("BABY", "tempId", "BABIES", "upperId")]
public class StatementBabyEntryOriginalObject
{
    [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public Guid tempId { get; set; }
    public Guid upperId { get; set; }

    [MaxLength(50)]
    [XmlProperty("FAM")]
    public string LastName { get; set; }
    [MaxLength(50)]
    [XmlProperty("IM")]
    public string FirstName { get; set; }
    [MaxLength(50)]
    [XmlProperty("OT")]
    public string MiddleName { get; set; }
    [XmlProperty("W")]
    public long? SexId { get; set; }
    [XmlProperty("DR")]
    public DateTime? Birthday { get; set; }
    [XmlProperty("MR")]
    [MaxLength(500)]
    public string PlaceOfBirth { get; set; }
}

[Table("OStatementBabyDocument")]
[Schema("DOC", "tempId", "BABY", "upperId")]
public class StatementBabyDocumentOriginalObject
{
    [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public Guid tempId { get; set; }
    public Guid upperId { get; set; }

    [XmlProperty("DOCTYPE")]
    public int? DocumentId { get; set; }
    [XmlProperty("DOCSER")]
    [MaxLength(50)]
    public string DocumentSerial { get; set; }
    [XmlProperty("DOCNUM")]
    [MaxLength(50)]
    public string DocumentNumber { get; set; }
    [XmlProperty("NAME_VP")]
    [MaxLength(500)]
    public string WhoIssue { get; set; }
    [XmlProperty("DOCDATE")]
    public DateTime? DocumentIssue { get; set; }
}
```

### Step #3. Create and execute Sql queries

```csharp
// Create repository factory
IPropertyScope propertyScope = new PropertyScope();
IQueryScope queryScope = new QueryScope(propertyScope);
using (ISampleRepositoryFactory repositoryFactory = new SampleRepositoryFactory(queryScope))
{
	// Create entities
	var fileList = new List<StatementBabyFileOriginalObject>()
	{
		new StatementBabyFileOriginalObject() { Name = "Sample1", Version = "1.0", RowCount = 3, RowCreated = DateTime.UtcNow }
	};
	var entryList = new List<StatementBabyEntryOriginalObject>()
	{
		new StatementBabyEntryOriginalObject() { LastName = "Иванов", FirstName = "Иван", MiddleName = "Иванович", Birthday = new DateTime(1990, 1, 1), SexId = 1 },
		new StatementBabyEntryOriginalObject() { LastName = "Петров", FirstName = "Иван", MiddleName = "Иванович", Birthday = new DateTime(1991, 1, 1), SexId = 1 },
		new StatementBabyEntryOriginalObject() { LastName = "Иванова", FirstName = "Светлана", MiddleName = "Ивановна", Birthday = new DateTime(1992, 1, 1), SexId = 2 }
	};

	// Create table in database and insert data
	await repositoryFactory.ExecuteSqlWithTransactionAsync(
		repositoryFactory.CreateTableToSql<StatementBabyFileOriginalObject>(),
		repositoryFactory.CreateTableToSql<StatementBabyEntryOriginalObject>(),
		repositoryFactory.AddToSql(fileList),
		repositoryFactory.AddToSql(entryList)
	);
}
```

### Step #4. Xml file reader

```csharp

using (var objectReader = new ObjectReader(scope))
{
	objectReader
		.handle<StatementBabyFileOriginalObject>(entities => repositoryFactory.BulkAddAsync(entities))
		.handle<StatementBabyEntryOriginalObject>(entities => repositoryFactory.BulkAddAsync(entities));
    objectReader.getObject(fileName);
}

```
