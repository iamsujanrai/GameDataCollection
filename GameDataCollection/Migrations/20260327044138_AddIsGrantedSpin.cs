using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDataCollection.Migrations
{
    /// <inheritdoc />
    public partial class AddIsGrantedSpin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsGrantedSpin",
                table: "SpinHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGrantedSpin",
                table: "SpinHistories");
        }
    }
}
