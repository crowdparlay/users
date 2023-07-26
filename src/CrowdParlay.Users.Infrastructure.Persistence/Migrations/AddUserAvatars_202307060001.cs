using FluentMigrator;

namespace CrowdParlay.Users.Infrastructure.Persistence.Migrations;

[Migration(202307060001)]
public class AddUserAvatars_202307060001 : Migration
{
    public override void Up() => Create.Column("avatar_url").OnTable("users").AsString(100).Nullable();
    public override void Down() => Delete.Column("avatar_url").FromTable("users");
}