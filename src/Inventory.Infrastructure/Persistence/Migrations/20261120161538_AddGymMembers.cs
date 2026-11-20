using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGymMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GymMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MembershipStartDate = table.Column<DateTime>(type: "date", nullable: false),
                    MembershipEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GymMembers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GymMembers_MemberNumber",
                table: "GymMembers",
                column: "MemberNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GymMembers_Status",
                table: "GymMembers",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GymMembers");
        }
    }
}
