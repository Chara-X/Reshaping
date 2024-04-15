using Reshaping.ConsoleApp.Extensions;
using Reshaping.ConsoleApp.Models;
using Reshaping.Extensions;

namespace Reshaping.ConsoleApp;

internal class Program
{
    private static void Main()
    {
        using var context = new ApplicationDbContext();
        //context.Database.EnsureDeleted();
        //context.Database.EnsureCreated();
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
        queryable = context.Addresses.Join(context.Users, x => x.Id, x => x.AddressId, (x, y) => new
        {
            x.Id,
            x.Province,
            x.City,
            UsersId = y.Id,
            UsersName = y.Name,
        }).Join(context.Cars, x => x.UsersId, x => x.UserId, (x, y) => new
        {
            x.Id,
            x.Province,
            x.City,
            x.UsersId,
            x.UsersName,
            UsersCarId = y.Id,
            UsersCarName = y.Name,
        }).Join(context.UserRoles, x => x.UsersId, x => x.UserId, (x, y) => new
        {
            x.Id,
            x.Province,
            x.City,
            x.UsersId,
            x.UsersName,
            x.UsersCarId,
            x.UsersCarName,
            UsersRolesId = y.RoleId
        }).Join(context.Roles, x => x.UsersRolesId, x => x.Id, (x, y) => new
        {
            x.Id,
            x.Province,
            x.City,
            x.UsersId,
            x.UsersName,
            x.UsersCarId,
            x.UsersCarName,
            UsersRolesId = y.Id,
            UsersRolesName = y.Name,
        });
        var addresses = queryable.Unflatten<Address>().ToArray();
        addresses.Dump();
        queryable = addresses.Flatten();
        queryable.Dump();
    }
}