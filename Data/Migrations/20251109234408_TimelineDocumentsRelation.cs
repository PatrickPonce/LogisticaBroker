using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogisticaBroker.Migrations
{
    /// <inheritdoc />
    public partial class TimelineDocumentsRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DispatchTimelineDocument",
                columns: table => new
                {
                    DocumentsId = table.Column<int>(type: "integer", nullable: false),
                    TimelinesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispatchTimelineDocument", x => new { x.DocumentsId, x.TimelinesId });
                    table.ForeignKey(
                        name: "FK_DispatchTimelineDocument_DispatchTimelines_TimelinesId",
                        column: x => x.TimelinesId,
                        principalTable: "DispatchTimelines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DispatchTimelineDocument_Documents_DocumentsId",
                        column: x => x.DocumentsId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DispatchTimelineDocument_TimelinesId",
                table: "DispatchTimelineDocument",
                column: "TimelinesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DispatchTimelineDocument");
        }
    }
}
