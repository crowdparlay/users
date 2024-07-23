using FluentMigrator;

namespace CrowdParlay.Users.Infrastructure.Persistence.Migrations;

[Migration(2024072201)]
public class Migration_20240722_01_AllowNullPasswordHash : Migration
{
    public override void Up() => Alter.Column("password_hash").OnTable("users").AsString(100).Nullable();
    public override void Down() => Alter.Column("password_hash").OnTable("users").AsString(100).NotNullable();
}