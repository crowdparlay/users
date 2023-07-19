using FluentMigrator;

namespace CrowdParlay.Users.Infrastructure.Persistence.Migrations;

[Migration(202307040001)]
public class InitialTables_202307040001 : Migration
{
    public override void Up()
    {
        IfDatabase("Postgres").Execute.Sql("""CREATE EXTENSION "uuid-ossp";""");
        Create.Table("users")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("username").AsString(25).NotNullable()
            .WithColumn("display_name").AsString(50).NotNullable()
            .WithColumn("password_hash").AsString(100).NotNullable()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable();
    }
    
    public override void Down()
    {
        IfDatabase("Postgres").Execute.Sql("""DROP EXTENSION "uuid-ossp";""");
        Delete.Table("users");
    }
}