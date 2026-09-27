namespace AcademiaDoZe.Application.Enums;

// Espelha o enum DatabaseType da Infraestrutura, mas exposto pela camada de Aplicação
// para que a Apresentação escolha o SGBD sem depender diretamente da Infraestrutura.
// A ordem/valores devem bater com Infrastructure.Data.DatabaseType (SqlServer=0, MySql=1, Sqlite=2).
public enum AppDatabaseType
{
    SqlServer,
    MySql,
    Sqlite
}
