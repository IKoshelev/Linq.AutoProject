
 Library is available via NuGet prackage https://www.nuget.org/packages/Linq.AutoProject
 
 Inner workings of the library are explained in articles:
 
 http://ikoshelev.azurewebsites.net/search/id/1/Expression-trees-and-advanced-queries-in-CSharp-01-IQueryable-and-Expression-Tree-basics
 
 http://ikoshelev.azurewebsites.net/search/id/3/Expression-trees-and-advanced-queries-in-CSharp-03-Expression-Tree-modification

# Linq.AutoProject

This project came about becuase my team needed to facilitate Linq queries in Enity Framework 
where a base class DTO needs to be projected into subclass DTO by copying all properties 
of base and adding a few more.

```cs
  IQueryable<BaseDto> baseQuery = GetBaseQuery();
  
  IQueryable<SubclassDto> query = from baseDto in baseQuery
                                  let moreData = DataContext.vMoreData.FirstOrDefault(x => x.Id == baseDto.Id)
                                  select new SubclassDto()
                                  {
                                    NewProp1 = moreData.Foo,
                                    NewProp2 = moreData.Baz,
                                    OldProp1 = moreData.SomeOverridingData,
                                    
                                    OldProp2 = baseDto.OldProp2,
                                    OldProp3 = baseDto.OldProp3,
                                    OldProp4 = baseDto.OldProp4,
                                    //... 20 more projections from BaseDto to SubclassDto
                                  };
  
```

Such query is tedious and fragile (it is quite easy to add new properties to BaseDto and forget to add projection).
So, we automated the proecess:

```cs
 IQueryable<BaseDto> baseQuery = GetBaseQuery();
 
 IQueryable<SubclassDto> query = from baseDto in baseQuery                                  
                                 let moreData = DataContext.vMoreData.FirstOrDefault(x => x.Id == baseDto.Id) 
                                 select baseDto.AutoProjectInto(() => new SubclassDto()
                                 {
                                  NewProp1 = moreData.Foo,
                                  NewProp2 = moreData.Baz,
                                  OldProp1 = moreData.SomeOverridingData
                                 });
                                 
 IQueryable<SubclassDto> activateQuery = query.ActivateAutoProjects(); 
 ```
 
 This will rewrite the query:
 1. It will determining all BaseDto public properties that have a public match property in SubclassDto 
 (by name and type) that were not bound by the provided construction inside the lambda and add their projection into the constructor.
 2. It will replace the call to base.AutoProjectInto with that new constuctor expression.
 
 
 There can be many such projections per query, and this works with both Linq query form and Linq lambda form, I.E.:
 
 ```cs
 baseQuery.Select(baseDto => baseDto.AutoProjectInto(() => new SubclassDto(){...})).ActivateAutoProjects()
 ```
 
 1.0.1 Update:
 Added shortcut extension method for when you don't need query form at all.
  ```cs
 baseQuery.AutoProject(baseDto => new SubclassDto(){...})
 ```
 
 
