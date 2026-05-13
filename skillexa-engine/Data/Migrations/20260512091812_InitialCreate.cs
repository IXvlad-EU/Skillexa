using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Skillexa.Engine.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "provider_quotas",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    day_key = table.Column<DateOnly>(type: "date", nullable: false),
                    used = table.Column<int>(type: "integer", nullable: false),
                    limit = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_provider_quotas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "templates",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    template_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_templates", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_provider_quotas_provider_day_key",
                table: "provider_quotas",
                columns: new[] { "provider", "day_key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "provider_quotas");

            migrationBuilder.DropTable(
                name: "templates");
        }
    }
}
