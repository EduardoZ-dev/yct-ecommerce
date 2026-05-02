namespace YCT.Domain.Common;

/// <summary>
/// Constantes de rol y helpers de jerarquía.
/// Jerarquía (alto a bajo): SuperAdmin > Admin > Employee > Customer
/// </summary>
public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Employee = "Employee";
    public const string Customer = "Customer";

    public static readonly string[] All = { SuperAdmin, Admin, Employee, Customer };

    /// <summary>Roles que tienen acceso al panel administrativo</summary>
    public const string AdminPanel = SuperAdmin + "," + Admin + "," + Employee;

    /// <summary>Roles que pueden gestionar usuarios (crear / editar roles)</summary>
    public const string CanManageUsers = SuperAdmin + "," + Admin;

    /// <summary>Roles que pueden borrar productos / categorías</summary>
    public const string CanDelete = SuperAdmin + "," + Admin;

    public static int Rank(string role) => role switch
    {
        SuperAdmin => 4,
        Admin => 3,
        Employee => 2,
        Customer => 1,
        _ => 0
    };

    public static bool IsValid(string role) => All.Contains(role);
}
