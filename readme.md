
## NuGet installation

- Install the [Simply.Property NuGet package](https://nuget.org/packages/Simply.Property/)
- Install the [Simply.Property.XmlReader NuGet package](https://nuget.org/packages/Simply.Property.XmlReader/)
- Install the [Simply.Property.SqlServer NuGet package](https://nuget.org/packages/Simply.Property.SqlServer/)

```powershell
PM> Install-Package Simply.Property
PM> Install-Package Simply.Property.XmlReader
PM> Install-Package Simply.Property.SqlServer
```

### Step #1. Implement interface IRepository, Repository and RepositoryFactory

```csharp
public class Repository<T> : IRepository where T : DbContext
{
    protected T ctx;
    public Repository(T context) { ctx = context; ctx.Database.SetCommandTimeout(300); }
    public Task<int> ExecuteSqlAsync(string query, string json = null) => (json == null) ? ctx.Database.ExecuteSqlCommandAsync(query) : ctx.Database.ExecuteSqlCommandAsync(query, new SqlParameter("@json", json));
    public async Task ExecuteSqlAsync(IEnumerable<SqlServerQuery> queries)
    {
        using (var transaction = await ctx.Database.BeginTransactionAsync().ConfigureAwait(false))
        {
            try
            {
                foreach (var query in queries)
                    await ExecuteSqlAsync(query.Query, query.Json).ConfigureAwait(false);
                transaction.Commit();
            }
            catch(SqlException)
            {
                transaction.Rollback();
            }
        }
    }
    public Task<List<TEntity>> GetAsync<TEntity>() where TEntity : class => ctx.Set<TEntity>().ToListAsync();
    public void Add<TEntity>(TEntity entity) where TEntity : class => ctx.Add(entity);
    public Task<int> SaveAsync() => ctx.SaveChangesAsync();
    public void Dispose() => ctx.Dispose();
}

public class SampleRepository : Repository<SampleDbContext>, ISampleRepository
{
    public SampleRepository(SampleDbContext context) : base(context) { }
    // Here you code to access Database
}

public class SampleRepositoryFactory : RepositoryFactory, ISampleRepositoryFactory
{
    public SampleRepositoryFactory() : base() { }
    public ISampleRepository GetSampleRepository() => new SampleRepository(new SampleDbContext());
    public override IRepository GetRepository() => GetSampleRepository();
}
```

### Step #2. Data annotation

```csharp
[Table("OStatementBabyFile")]
[XmlSchema("BABIES", "tempId")]
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
[XmlSchema("BABY", "tempId", "BABIES", "upperId")]
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
[XmlSchema("DOC", "tempId", "BABY", "upperId")]
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
ISampleRepositoryFactory repositoryFactory = new SampleRepositoryFactory();
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

// Example #1
// Create table in database and insert data
await repositoryFactory.Database.ExecuteSqlAsync(
    repositoryFactory.Database.CreateTableToSql<StatementBabyFileOriginalObject>(),
    repositoryFactory.Database.CreateTableToSql<StatementBabyEntryOriginalObject>(),
    repositoryFactory.Database.AddToSql(fileList),
    repositoryFactory.Database.AddToSql(entryList)
);

// Example #2
// Create table in database and insert data
await repositoryFactory.Database.CreateTableAsync<StatementBabyFileOriginalObject>();
await repositoryFactory.Database.CreateTableAsync<StatementBabyEntryOriginalObject>();
await repositoryFactory.Database.AddAsync(fileList);
await repositoryFactory.Database.AddAsync(entryList);
```

### Step #4. Read from xml file

```csharp
// Create table in database
await repositoryFactory.Database.CreateTableAsync<StatementBabyFileOriginalObject>();
await repositoryFactory.Database.CreateTableAsync<StatementBabyEntryOriginalObject>();
await repositoryFactory.Database.CreateTableAsync<StatementBabyDocumentOriginalObject>();
using (var objectReader = new ObjectReader(scope))
{
    // Read data from xml file and insert data into the table
    objectReader
        .HandleString<StatementBabyFileOriginalObject>(json => repositoryFactory.AddAsync<StatementBabyFileOriginalObject>(json))
        .HandleString<StatementBabyEntryOriginalObject>(json => repositoryFactory.AddAsync<StatementBabyEntryOriginalObject>(json));
        .HandleString<StatementBabyDocumentOriginalObject>(json => repositoryFactory.AddAsync<StatementBabyDocumentOriginalObject>(json));
    objectReader.GetObject(fileName);
}

```
