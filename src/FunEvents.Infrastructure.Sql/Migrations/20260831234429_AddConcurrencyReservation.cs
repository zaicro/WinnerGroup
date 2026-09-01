using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FunEvents.Infrastructure.Sql.Migrations
{
    /// <inheritdoc />
    public partial class AddConcurrencyReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "TbEvents",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 6)
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AddColumn<int>(
                name: "availableCapacity",
                table: "TbEvents",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "availableCapacity",
                table: "TbEvents");

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "TbEvents",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 5)
                .OldAnnotation("Relational:ColumnOrder", 6);
        }
    }
}
