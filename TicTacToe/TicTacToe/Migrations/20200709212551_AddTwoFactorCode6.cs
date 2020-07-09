using Microsoft.EntityFrameworkCore.Migrations;

namespace TicTacToe.Migrations
{
    public partial class AddTwoFactorCode6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IdentityUserToken<Guid>",
                table: "IdentityUserToken<Guid>");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "IdentityUserToken<Guid>",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "IdentityUserToken<Guid>",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "IdentityUserToken<Guid>",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdentityUserToken<Guid>",
                table: "IdentityUserToken<Guid>",
                columns: new[] { "UserId", "LoginProvider", "Name" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IdentityUserToken<Guid>",
                table: "IdentityUserToken<Guid>");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "IdentityUserToken<Guid>",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "IdentityUserToken<Guid>",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "IdentityUserToken<Guid>",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdentityUserToken<Guid>",
                table: "IdentityUserToken<Guid>",
                column: "Value");
        }
    }
}
