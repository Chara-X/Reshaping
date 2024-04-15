# Reshaping

Reshaping是一个对象到对象映射器，主要实现改变对象的形状的功能，提供将模型对象扁平化为表对象及其逆过程的功能，实现对关联数据的高效组合。相比于Entity Framework Core提供的`Include`，`ThenInclude`方法，Reshaping更加适合不依赖导航属性的原生查询。使熟悉SQL的人能按他们喜爱的方式工作，并且将从繁琐的数据模型转换中解脱出来，专注于表的操作。

## Tables & Models

关系数据库查询本质上是对表对象的的操作，但是在处理完后，有时需要将它们映射成模型对象，例如需要将查出的数据通过JSON返回。表对象是扁平的，所有属性都是基元类型，模型对象是层级的，属性可以具有集合或复合类型。表对象通过外键关联，有时需要连接多个表来进行查询，并且存在一种特殊表叫做关联表。模型对象通过引用关联，不需要再进行连接，也不存在关联表。下面举例对比表和模型的区别（为了区分，约定以s结尾的为表），其中，`Users`表和`Addresses`表为多对一关系、`Users`和`Cars`表为一对一关系、`Users`和`Roles`表为一对多关系且`UserRoles`为关联表。

```C#
// Tables
internal class Users
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int AddressId { get; set; }
}
internal class Addresses
{
    public int Id { get; set; }
    public string? Province { get; set; }
    public string City { get; set; } = null!;
}
internal class Cars
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int? UserId { get; set; }
}
internal class UserRoles
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
}
internal class Roles
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}
```

```C#
// Models
internal class User
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public Addresses Address { get; set; } = null!;
    public Car? Car { get; set; }
    public IEnumerable<Roles> Roles { get; set; } = null!;
}
internal class Address
{
    public int Id { get; set; }
    public string? Province { get; set; }
    public string City { get; set; } = null!;
    public IEnumerable<User> Users { get; set; } = null!;
}
internal class Car
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public User? User { get; set; }
}
internal class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public IEnumerable<User> Users { get; set; } = null!;
}
```

## Unflatten & Flatten

Reshaping提供两个扩展方法，分别是`IQueryable`的`Unflatten<T>`方法和`IEnumerable<T>`的`Flatten`方法，前者将`IQueryable`中查询好的表数据逆扁平化为`IEnumerable<T>`，后者则是其逆过程。模型对象图可以存在环且按照约定表的列名需要匹配模型对象图的属性路径。例如如果模型对象为`User`，其`Car`属性的`Id`属性路径就为`CarId`。下面的例子中首先将多个具有不同关系的表进行连接，然后逆扁平化再扁平化，选择任意模型类型都行，只要列名和路径匹配即可，例子中使用`User`作为模型类型。

```c#
using var context = new ApplicationDbContext();
IQueryable queryable = context.Users.Join(context.Addresses, x => x.AddressId, x => x.Id, (x, y) => new
{
    x.Id,
    x.Name,
    AddressId = y.Id,
    AddressProvince = y.Province,
    AddressCity = y.City,
}).Join(context.Cars, x => x.Id, y => y.UserId, (x, y) => new
{
    x.Id,
    x.Name,
    x.AddressId,
    x.AddressProvince,
    x.AddressCity,
    CarId = y.Id,
    CarName = y.Name,
}).Join(context.UserRoles, x => x.Id, x => x.UserId, (x, y) => new
{
    x.Id,
    x.Name,
    x.AddressId,
    x.AddressProvince,
    x.AddressCity,
    x.CarId,
    x.CarName,
    UserRoleRoleId = y.RoleId,
}).Join(context.Roles, x => x.UserRoleRoleId, x => x.Id, (x, y) => new
{
    x.Id,
    x.Name,
    x.AddressId,
    x.AddressProvince,
    x.AddressCity,
    x.CarId,
    x.CarName,
    RolesId = y.Id,
    RolesName = y.Name,
});
var users = queryable.Unflatten<User>().ToArray();
users.Dump();
queryable = users.Flatten();
queryable.Dump();
```

```Json
// 逆扁平化的结果
[
  {
    "Id": 1,
    "Name": "David",
    "Address": {
      "Id": 1,
      "Province": "湖北省",
      "City": "武汉市"
    },
    "Car": {
      "Id": 1,
      "Name": "特斯拉",
      "User": null
    },
    "Roles": [
      {
        "Id": 1,
        "Name": "Manager"
      },
      {
        "Id": 2,
        "Name": "Worker"
      }
    ]
  },
  {
    "Id": 2,
    "Name": "Zira",
    "Address": {
      "Id": 1,
      "Province": "湖北省",
      "City": "武汉市"
    },
    "Car": {
      "Id": 2,
      "Name": "大众",
      "User": null
    },
    "Roles": [
      {
        "Id": 2,
        "Name": "Worker"
      }
    ]
  }
]
// 扁平化的结果
[
  {
    "Id": 1,
    "Name": "David",
    "AddressId": 1,
    "AddressProvince": "湖北省",
    "AddressCity": "武汉市",
    "CarId": 1,
    "CarName": "特斯拉",
    "RolesId": 1,
    "RolesName": "Manager"
  },
  {
    "Id": 1,
    "Name": "David",
    "AddressId": 1,
    "AddressProvince": "湖北省",
    "AddressCity": "武汉市",
    "CarId": 1,
    "CarName": "特斯拉",
    "RolesId": 2,
    "RolesName": "Worker"
  },
  {
    "Id": 2,
    "Name": "Zira",
    "AddressId": 1,
    "AddressProvince": "湖北省",
    "AddressCity": "武汉市",
    "CarId": 2,
    "CarName": "大众",
    "RolesId": 2,
    "RolesName": "Worker"
  }
]
```
