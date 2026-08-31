using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FunEvents.Infrastructure.Sql.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TbEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    eventDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    capacity = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_TbEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TbUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    passwordHash = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_TbUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TbReservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    reservationCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    userId = table.Column<int>(type: "int", nullable: false),
                    eventId = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    channel = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_TbReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TbReservations_TbEvents_eventId",
                        column: x => x.eventId,
                        principalTable: "TbEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TbReservations_TbUsers_userId",
                        column: x => x.userId,
                        principalTable: "TbUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TbEvents_code",
                table: "TbEvents",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TbReservations_eventId",
                table: "TbReservations",
                column: "eventId");

            migrationBuilder.CreateIndex(
                name: "IX_TbReservations_reservationCode",
                table: "TbReservations",
                column: "reservationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TbReservations_userId",
                table: "TbReservations",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_TbUsers_username",
                table: "TbUsers",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TbReservations");

            migrationBuilder.DropTable(
                name: "TbEvents");

            migrationBuilder.DropTable(
                name: "TbUsers");
        }
    }
}
