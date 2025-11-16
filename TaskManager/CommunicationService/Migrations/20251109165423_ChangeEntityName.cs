using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommunicationService.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEntityName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CommentEntity",
                table: "CommentEntity");

            migrationBuilder.RenameTable(
                name: "CommentEntity",
                newName: "Comments");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Comments",
                table: "Comments",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Comments",
                table: "Comments");

            migrationBuilder.RenameTable(
                name: "Comments",
                newName: "CommentEntity");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommentEntity",
                table: "CommentEntity",
                column: "Id");
        }
    }
}
