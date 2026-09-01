using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FunEvents.Infrastructure.Sql.Migrations
{
    /// <inheritdoc />
    public partial class Idempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TbIdempotency",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    key = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    tableName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    tableKeyValue = table.Column<long>(type: "bigint", nullable: false),
                    isActive = table.Column<bool>(type: "bit", nullable: false),
                    isDeleted = table.Column<bool>(type: "bit", nullable: false),
                    createdBy = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modifiedBy = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    modifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    remarks = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TbIdempotency", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TbIdempotency_key",
                table: "TbIdempotency",
                column: "key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TbIdempotency");
        }
    }
}
