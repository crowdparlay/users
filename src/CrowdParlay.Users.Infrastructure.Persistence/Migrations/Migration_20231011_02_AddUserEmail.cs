using FluentMigrator;

namespace CrowdParlay.Users.Infrastructure.Persistence.Migrations;

[Migration(2023101102)]
public class Migration_20231011_02_AddUserEmail : Migration
{
    public override void Up() => Create.Column("email").OnTable("users").AsString(50).NotNullable();

    public override void Down() => Delete.Column("email").FromTable("users");
}