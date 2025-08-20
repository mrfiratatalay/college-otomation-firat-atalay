using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElasoftCommunityManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class migo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValidDaysSerialized = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisabledDatesSerialized = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailySlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartHour = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndHour = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailySlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailySlots_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    StartHour = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndHour = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeSlots_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DisabledSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    SlotId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisabledSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisabledSlots_DailySlots_SlotId",
                        column: x => x.SlotId,
                        principalTable: "DailySlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DisabledSlots_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventReservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    TimeSlotId = table.Column<int>(type: "int", nullable: false),
                    ReservationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeSlotId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventReservations_DailySlots_TimeSlotId",
                        column: x => x.TimeSlotId,
                        principalTable: "DailySlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventReservations_Event_EventId",
                        column: x => x.EventId,
                        principalTable: "Event",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventReservations_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventReservations_TimeSlots_TimeSlotId1",
                        column: x => x.TimeSlotId1,
                        principalTable: "TimeSlots",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailySlots_LocationId",
                table: "DailySlots",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_DisabledSlots_LocationId",
                table: "DisabledSlots",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_DisabledSlots_SlotId",
                table: "DisabledSlots",
                column: "SlotId");

            migrationBuilder.CreateIndex(
                name: "IX_EventReservations_EventId",
                table: "EventReservations",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventReservations_LocationId",
                table: "EventReservations",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_EventReservations_TimeSlotId",
                table: "EventReservations",
                column: "TimeSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_EventReservations_TimeSlotId1",
                table: "EventReservations",
                column: "TimeSlotId1");

            migrationBuilder.CreateIndex(
                name: "IX_TimeSlots_LocationId",
                table: "TimeSlots",
                column: "LocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DisabledSlots");

            migrationBuilder.DropTable(
                name: "EventReservations");

            migrationBuilder.DropTable(
                name: "DailySlots");

            migrationBuilder.DropTable(
                name: "TimeSlots");

            migrationBuilder.DropTable(
                name: "Locations");
        }
    }
}
