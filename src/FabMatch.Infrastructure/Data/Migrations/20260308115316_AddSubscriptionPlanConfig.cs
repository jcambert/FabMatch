using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FabMatch.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionPlanConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubscriptionPlanConfigs",
                columns: table => new
                {
                    Tier = table.Column<int>(type: "integer", nullable: false),
                    MonthlyPrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    MaxProjects = table.Column<int>(type: "integer", nullable: false),
                    MaxAnalysesPerMonth = table.Column<int>(type: "integer", nullable: false),
                    UnlimitedMatching = table.Column<bool>(type: "boolean", nullable: false),
                    PrioritySupport = table.Column<bool>(type: "boolean", nullable: false),
                    Features = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlanConfigs", x => x.Tier);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriptionPlanConfigs");
        }
    }
}
