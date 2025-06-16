using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BorrowingSystemAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddQuantityToRequestItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "RequestItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "RequestItems");
        }
    }
}
