using FluentMigrator;

namespace CrowdParlay.Users.Infrastructure.Persistence.Migrations;

[Migration(202307190001)]
public class AddUsernameNormalized_202307190001 : Migration
{
    public override void Up() => Create.Column("username_normalized").OnTable("users").AsString().Nullable();
    public override void Down() => Delete.Column("username_normalized").FromTable("users");
}