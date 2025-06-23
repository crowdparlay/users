using System.Data;
using FluentMigrator;

namespace CrowdParlay.Users.Infrastructure.Persistence.Migrations;

[Migration(2024072301)]
public class Migration_20240723_01_MakeExternalLoginsToUsersFkCascade : Migration
{
    public override void Up()
    {
        Delete.ForeignKey("FK_external_logins_user_id_users_id").OnTable("external_logins");
        Create.ForeignKey("FK_external_logins_user_id_users_id")
            .FromTable("external_logins").ForeignColumn("user_id")
            .ToTable("users").PrimaryColumn("id")
            .OnDeleteOrUpdate(Rule.Cascade);
    }

    public override void Down()
    {
        Delete.ForeignKey("FK_external_logins_user_id_users_id").OnTable("external_logins");
        Create.ForeignKey("FK_external_logins_user_id_users_id")
            .FromTable("external_logins").ForeignColumn("user_id")
            .ToTable("users").PrimaryColumn("id");
    }
}