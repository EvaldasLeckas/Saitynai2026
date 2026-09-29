using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Saitynai.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToUserResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "UserResults",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_UserResults_UserId",
                table: "UserResults",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserResults_Users_UserId",
                table: "UserResults",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserResults_Users_UserId",
                table: "UserResults");

            migrationBuilder.DropIndex(
                name: "IX_UserResults_UserId",
                table: "UserResults");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserResults");
        }
    }
}
