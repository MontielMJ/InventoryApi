using System.Data.Common;
namespace Inventory.Application.Abstractions;
public interface IDbConnectionFactory
{
    DbConnection CreateConnection();
}
