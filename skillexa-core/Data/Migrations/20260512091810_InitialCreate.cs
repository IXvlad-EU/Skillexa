using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Skillexa.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "document_statuses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'"),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "provider_usages",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    period_key = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    used = table.Column<int>(type: "integer", nullable: false),
                    remaining = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_provider_usages", x => x.id);
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

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entra_object_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    display_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "documents",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    status_id = table.Column<int>(type: "integer", nullable: false),
                    template_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    template_version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    payload = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    pdf_storage_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    snapshot_storage_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    error_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    correlation_id = table.Column<long>(type: "bigint", nullable: false),
                    idempotency_key = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_documents", x => x.id);
                    table.ForeignKey(
                        name: "fk_documents_document_statuses_status_id",
                        column: x => x.status_id,
                        principalTable: "document_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_documents_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "document_statuses",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "Queued" },
                    { 2, "Processing" },
                    { 3, "Succeeded" },
                    { 4, "Failed" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_document_statuses_name",
                table: "document_statuses",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_documents_created_at",
                table: "documents",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_documents_idempotency_key",
                table: "documents",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_documents_status_id",
                table: "documents",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_user_id_status_id",
                table: "documents",
                columns: new[] { "user_id", "status_id" });

            migrationBuilder.CreateIndex(
                name: "ix_provider_usages_provider_period_key",
                table: "provider_usages",
                columns: new[] { "provider", "period_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_entra_object_id",
                table: "users",
                column: "entra_object_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "documents");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "provider_usages");

            migrationBuilder.DropTable(
                name: "templates");

            migrationBuilder.DropTable(
                name: "document_statuses");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
