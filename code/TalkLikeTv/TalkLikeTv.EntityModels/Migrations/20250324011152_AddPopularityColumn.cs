using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalkLikeTv.EntityModels.Migrations
{
    /// <inheritdoc />
    public partial class AddPopularityColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Popularity",
                table: "Titles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Popularity",
                table: "Titles");
        }
    }
}
