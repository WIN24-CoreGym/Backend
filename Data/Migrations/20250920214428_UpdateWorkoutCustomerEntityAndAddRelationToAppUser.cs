using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWorkoutCustomerEntityAndAddRelationToAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BookingDate",
                table: "WorkoutCustomer",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutCustomer_UserId",
                table: "WorkoutCustomer",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutCustomer_AspNetUsers_UserId",
                table: "WorkoutCustomer",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutCustomer_AspNetUsers_UserId",
                table: "WorkoutCustomer");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutCustomer_UserId",
                table: "WorkoutCustomer");

            migrationBuilder.DropColumn(
                name: "BookingDate",
                table: "WorkoutCustomer");
        }
    }
}
