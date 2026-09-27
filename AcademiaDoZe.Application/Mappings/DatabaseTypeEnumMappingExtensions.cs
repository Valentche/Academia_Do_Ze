using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Infrastructure.Data;
namespace AcademiaDoZe.Application.Mappings;

// Converte entre o enum de SGBD da Aplicação (AppDatabaseType) e o da Infraestrutura (DatabaseType).
// Os valores dos dois enums são idênticos, então o cast direto é seguro.
public static class DatabaseTypeEnumMappingExtensions
{
    public static DatabaseType ToInfrastructure(this AppDatabaseType appDatabaseType)
    {
        return (DatabaseType)appDatabaseType;
    }

    public static AppDatabaseType ToApplication(this DatabaseType databaseType)
    {
        return (AppDatabaseType)databaseType;
    }
}
