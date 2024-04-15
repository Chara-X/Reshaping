# Reshaping

## Tables & Models

```csharp
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

```csharp
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

Reshaping provide two extension methods, `Unflatten<T>` method of `IQueryable` and `Flatten` method of `IEnumerable<T>` respectively. The former unflatten the queried table rows from `IQueryable` into `IEnumerable<T>`, The latter is its inverse operation. Model object graph can contains cycles. By convention the column names of the table need to match the property paths of the model object graph. For example, if the model object is `User`, the path to the `Id` property of it's `Car` property would be `CarId`.

```csharp
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
// Unflatten's results
[
  {
    "Id": 1,
    "Name": "David",
    "Address": {
      "Id": 1,
      "Province": "HuBei",
      "City": "WuHan"
    },
    "Car": {
      "Id": 1,
      "Name": "Tesla",
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
      "Province": "HuBei",
      "City": "WuHan"
    },
    "Car": {
      "Id": 2,
      "Name": "VW",
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
// Flatten's results
[
  {
    "Id": 1,
    "Name": "David",
    "AddressId": 1,
    "AddressProvince": "HuBei",
    "AddressCity": "WuHan",
    "CarId": 1,
    "CarName": "Tesla",
    "RolesId": 1,
    "RolesName": "Manager"
  },
  {
    "Id": 1,
    "Name": "David",
    "AddressId": 1,
    "AddressProvince": "HuBei",
    "AddressCity": "WuHan",
    "CarId": 1,
    "CarName": "Tesla",
    "RolesId": 2,
    "RolesName": "Worker"
  },
  {
    "Id": 2,
    "Name": "Zira",
    "AddressId": 1,
    "AddressProvince": "HuBei",
    "AddressCity": "WuHan",
    "CarId": 2,
    "CarName": "VW",
    "RolesId": 2,
    "RolesName": "Worker"
  }
]
```
