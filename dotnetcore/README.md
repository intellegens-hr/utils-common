# Intellegens Utils: Commons, .NET Core

## Api Commons

### CrudApiControllerAbstract
REST API controller abstract containing default routes and parameters for basic (CRUD) operations.

## Database Commons
### Base entites
When modeling database, in most cases models have:
- Id (of defined primitive type)
- Tracking fields
   - User who created entity
   - User who updated entity
   - Time created
   - Time updated
   - Tenant Id (optionally)

Database commons contain common interface all such entites should implement, along with abstract classes which already implement it.

### DbContext extensions
#### TrackingDbContextExtensions
TrackingDbContextExtensions contain useful extension methods for any DbContext:
- *SetEntityTrackingData* looks for all added/modified entites and automatically modifies tracking fields
- *GetDbContextEntitiesWithProperty* looks for all context entites which have specific property
- *SetGlobalQueryFilter* sets global query filter to context based on given parameters (property name, target value, operator)

#### TrackingDbContextAbstract
TrackingDbContextAbstract can be inherited to provide entity change tracking features. Beneath the hood, it overrides all "SaveChanges" method to call TrackingDbContextExtensions.SetEntityTrackingData before each save.
In case this base class can't be inherited (e.g. due to multiple inheritance), use it as template to see how tracking features are implemented.

## Rest client
Rest client provides simple way to do Rest API calls:
- methods for all common Rest methods (Get/Post/Put/Delete)
- automatic error handling for JSON errors - useful when call is successful but contains invalid data
- possibility to provide custom HttpClient
- possibility to override HttpMessage builder method and implement custom logic (headers, ...)

## Result classes
All services and APIs shouldn't return just raw data, they should:
- handle all known errors and throw exceptions only when error which can't be handled occurs
- return success result (and optionally data) if everything went ok
- return error result if anything went wrong

Basic results are Result class used by services and ApiResult used by controller classes. Various extension methods are provided to transform Result to ApiResult.

To avoid writing these classes (and their extension methods) in all project, most common result forms can be found here and used anywhere.

## Search service

Search service provides easy way to provide filtering features on any IQueryable/IEnumerable data source. Also, it's simple structure makes it great for API usage.

### Basic strucutres
```csharp
public enum Operators
{
    EQUALS,
    STRING_CONTAINS,
    STRING_WILDCARD,
    LESS_THAN,
    LESS_THAN_OR_EQUAL_TO,
    GREATER_THAN,
    GREATER_THAN_OR_EQUAL_TO
}

public enum LogicOperators{
  ANY,
  ALL
}

public class SearchCriteria {
   // Keys and what operator to place between them (key1 == xyz) AND/OR (key2 == xyz)
   public List<string> Keys { get; set; }
   public LogicOperators KeysLogic { get; set; }
   
   // Values and what operator to place between them ((key1 == xyz AND/OR key1 == xyz2) AND/OR (key2 == xyz))
   public List<string> Values { get; set; }
   public LogicOperators ValuesLogic { get; set; }
   
   public Operators Operator { get; set; }
   public bool Negate { get; set; }
   
   // Operator to place between filters (if any)
   public List<SearchCriteria> Criteria { get; set; }
   public LogicOperators CriteriaLogic { get; set; }
}

public class SearchRequest: SearchCriteria {
   public int Offset { get; set; } = 0;
   public int Limit { get; set; } = 10;
   
   public List<SearchOrder> Order { get; set; }
   public bool OrderByMatchCount { get; set; }
}

public class SearchOrder
{
    public string Key { get; set; }
    public bool Ascending { get; set; } = true;
}
```

As seen above, basic search request consists of following elements:
- offset - number of records to skip when paging
- limit - number of records to return when paging
- keys - keys to search, multiple keys have AND/OR between them depending on keys logic
- values - values to match, multiple values for single key have AND/OR berween them depending on value logic
- criteria - multiple filters have AND/OR between them depending on criteria logic
- ordering - define orderBy

### Full-text search

If keys are not specified for criteria/request, but values are - this is treated as full-text search. 

Full text search works in following way:
- when requested, service will look for all properties which have specified `FullTextSearch` attribute and automatically do filter/search by these properties
- if no `FullTextSearch` attributes are defined on class, all string properties will be taken

#### FullTextSearchAttribute
`FullTextSearchAttribute` can be defined on any property. 
If target property is class, this attribute optionally takes string argument which represents comma-separated list of properties which should be used when doing full-text search

Example:
```csharp
public class ParentClass
{
    [FullTextSearch]
    public string Name { get; set; }

    public string Title { get; set; }

    [FullTextSearch("Title")]
    public ChildClass Child { get; set; }
}

public class ChildClass
{
    public string Name { get; set; }
    public string Title { get; set; }
}
```

When full-text search is requeste on `ParentClass`, following properties will be searched:
- `Name`
- `Child.Title`

In case `FullTextSearch` attribute on `Child` property didn't have argument specified, following properties would be searched:
- `Name`
- `Child.Name`
- `Child.Title`

### Examples

Following entities will be used for demonstration:
```csharp
public class Role
{
    public string Id { get; set; }
    public string Title { get; set; }
}

public class User
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string FullName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public virtual ICollection<Role> Roles { get; set; }
}
```

#### Filter all users in tenant 1 with role "A" or "B", ordered by fullname
```json
{
    "criteria": [
        {
            "keys": ["tenantId"],
            "operator": "EQUALS",
            "values": ["1"]
        },
        {
            "keys": ["roles.id"],
            "operator": "EQUALS",
            "values": ["A", "B"],
            "valuesLogic": "ANY"
        }
    ],
    "criteriaLogic": "ALL",
    "order": [{
        "key": "fullName",
        "ascending": true
    }]
}
```

#### Filter all users that have "John" in firstName or lastName
```json
{
    "criteria": [{
        "keys": ["firstName", "lastName"],
        "keysLogic": "ANY",
        "operator": "EQUALS",
        "values": ["John"]
    }]
}
```

#### Filter all users that have "John" in firstName and lastName
```json
{
    "criteria": [{
        "keys": ["firstName", "lastName"],
        "keysLogic": "ALL",
        "operator": "EQUALS",
        "values": ["John"]
    }]
}
```

#### Filter all users born before 1980 or after 2000
```json
{
    "criteria": [{
        "keys": ["dateOfBirth"],
        "operator": "LESS_THAN",
        "values": ["1980-01-01"]
    },
    {
        "keys": ["dateOfBirth"],
        "operator": "GREATER_THAN_OR_EQUAL_TO",
        "values": ["2001-01-01"]
    }],
    "criteriaLogic": "ANY"
}
```