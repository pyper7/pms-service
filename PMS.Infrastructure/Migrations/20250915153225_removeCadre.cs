using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class removeCadre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Cadres_CadreId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Cadres_CadreId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Cadres");

            migrationBuilder.DropIndex(
                name: "IX_Users_CadreId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Posts_CadreId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "CadreId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CadreId",
                table: "Posts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CadreId",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CadreId",
                table: "Posts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "Cadres",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    MaxGradeLevel = table.Column<int>(type: "int", nullable: false),
                    MinGradeLevel = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ServiceCategory = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cadres", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_CadreId",
                table: "Users",
                column: "CadreId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CadreId",
                table: "Posts",
                column: "CadreId");

            migrationBuilder.CreateIndex(
                name: "IX_Cadres_Name",
                table: "Cadres",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Cadres_CadreId",
                table: "Posts",
                column: "CadreId",
                principalTable: "Cadres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Cadres_CadreId",
                table: "Users",
                column: "CadreId",
                principalTable: "Cadres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
