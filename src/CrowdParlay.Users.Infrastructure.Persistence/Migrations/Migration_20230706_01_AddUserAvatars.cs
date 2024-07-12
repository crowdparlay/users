using FluentMigrator;

namespace CrowdParlay.Users.Infrastructure.Persistence.Migrations;

[Migration(2023070601)]
public class Migration_20230706_01_AddUserAvatars : Migration
{
    public override void Up() => Create.Column("avatar_url").OnTable("users").AsString(100).Nullable();
    public override void Down() => Delete.Column("avatar_url").FromTable("users");
}