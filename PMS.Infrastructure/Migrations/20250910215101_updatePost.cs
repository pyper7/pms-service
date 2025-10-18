using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatePost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RoleId",
                table: "Posts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "PostId1",
                table: "PostOccupancies",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_RoleId",
                table: "Posts",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PostOccupancies_PostId1",
                table: "PostOccupancies",
                column: "PostId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PostOccupancies_Posts_PostId1",
                table: "PostOccupancies",
                column: "PostId1",
                principalTable: "Posts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Roles_RoleId",
                table: "Posts",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostOccupancies_Posts_PostId1",
                table: "PostOccupancies");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Roles_RoleId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_RoleId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_PostOccupancies_PostId1",
                table: "PostOccupancies");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PostId1",
                table: "PostOccupancies");
        }
    }
}
