using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdOfficerManagements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Officers table and related constraints don't exist, so skip these operations

            // These columns were already renamed in the previous migration

            // These columns were already added in the previous migration

            // These foreign keys were already added in the previous migration
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This migration is now empty since all operations were moved to previous migration
        }
    }
}
