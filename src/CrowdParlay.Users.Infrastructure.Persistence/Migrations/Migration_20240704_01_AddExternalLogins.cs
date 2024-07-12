using FluentMigrator;

namespace CrowdParlay.Users.Infrastructure.Persistence.Migrations;

[Migration(2024070401)]
public class Migration_20240704_01_AddExternalLogins : Migration
{
    public override void Up()
    {
        Create.Table("external_login_providers")
            .WithColumn("id").AsString().NotNullable().PrimaryKey()
            .WithColumn("display_name").AsString().NotNullable()
            .WithColumn("logo_url").AsString().NotNullable();

        Create.Table("external_logins")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("user_id").AsGuid().NotNullable().Indexed().ForeignKey("users", "id")
            .WithColumn("provider_id").AsString().NotNullable().Indexed().ForeignKey("external_login_providers", "id")
            .WithColumn("identity").AsString().NotNullable().Indexed();
        
        Create.Index("provider_user_idx").OnTable("external_logins")
            .OnColumn("provider_id").Ascending()
            .OnColumn("user_id").Unique();
        
        Create.Index("provider_identity_idx").OnTable("external_logins")
            .OnColumn("provider_id").Ascending()
            .OnColumn("identity").Unique();
        
        Insert.IntoTable("external_login_providers").Row(new
        {
            id = "google",
            display_name = "Google",
            logo_url = "https://lh3.googleusercontent.com/COxitqgJr1sJnIDe8-jiKhxDx1FrYbtRHKJ9z_hELisAlapwE9LUPh6fcXIfb5vwpbMl4xl9H9TRFPc5NOO8Sb3VSgIBrfRYvW6cUA"
        });
    }

    public override void Down()
    {
        Delete.Table("external_login_services");
        Delete.Table("external_logins");
        Delete.Index("service_identity_idx");
    }
}