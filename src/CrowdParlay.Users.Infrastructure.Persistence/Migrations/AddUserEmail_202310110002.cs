using FluentMigrator;

namespace CrowdParlay.Users.Infrastructure.Persistence.Migrations;

[Migration(202310110002)]
public class AddUserEmail_202310110002 : Migration
{
    public override void Up() => Create.Column("email").OnTable("users").AsString(50).NotNullable();

    public override void Down() => Delete.Column("email").FromTable("users");
}