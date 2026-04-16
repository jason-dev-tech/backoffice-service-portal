using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackofficeServicePortal.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveServiceRequestAuditLogForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequestAuditLogEntries_ServiceRequests_ServiceRequestId",
                table: "ServiceRequestAuditLogEntries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequestAuditLogEntries_ServiceRequests_ServiceRequestId",
                table: "ServiceRequestAuditLogEntries",
                column: "ServiceRequestId",
                principalTable: "ServiceRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
