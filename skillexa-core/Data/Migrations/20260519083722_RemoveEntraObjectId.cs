using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skillexa.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEntraObjectId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_entra_object_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "entra_object_id",
                table: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "entra_object_id",
                table: "users",
                type: "character varying(36)",
                maxLength: 36,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_users_entra_object_id",
                table: "users",
                column: "entra_object_id",
                unique: true);
        }
    }
}
