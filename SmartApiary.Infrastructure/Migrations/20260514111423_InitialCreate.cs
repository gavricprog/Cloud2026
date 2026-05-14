using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartApiary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ActivationToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActivationTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordResetTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WeightDropThreshold = table.Column<double>(type: "float", nullable: false, defaultValue: 10.0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApiaryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HiveId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alerts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Apiaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeekeeperId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apiaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Apiaries_Users_BeekeeperId",
                        column: x => x.BeekeeperId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Parcels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    FarmerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parcels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parcels_Users_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HiveType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FrameColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    QueenAge = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApiaryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hives_Apiaries_ApiaryId",
                        column: x => x.ApiaryId,
                        principalTable: "Apiaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Crops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CropType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BloomingPeriod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdditionalInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Crops_Parcels_ParcelId",
                        column: x => x.ParcelId,
                        principalTable: "Parcels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SprayingAnnouncements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlannedStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationHours = table.Column<int>(type: "int", nullable: false),
                    SubstanceType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NotifiedBeekeeperCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SprayingAnnouncements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SprayingAnnouncements_Parcels_ParcelId",
                        column: x => x.ParcelId,
                        principalTable: "Parcels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HiveDiaryEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HiveId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FloorColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HoneyFrames = table.Column<int>(type: "int", nullable: false),
                    HoneyAmount = table.Column<double>(type: "float", nullable: false),
                    BroodFrames = table.Column<int>(type: "int", nullable: false),
                    QueenPresent = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HiveDiaryEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HiveDiaryEntries_Hives_HiveId",
                        column: x => x.HiveId,
                        principalTable: "Hives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IoTDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UniqueDeviceId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DeviceAccessToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BatteryAlertSent = table.Column<bool>(type: "bit", nullable: false),
                    HiveId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IoTDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IoTDevices_Hives_HiveId",
                        column: x => x.HiveId,
                        principalTable: "Hives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SprayingLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SprayingAnnouncementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActualStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActualEndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ParcelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CropType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubstanceUsed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WindSpeed = table.Column<double>(type: "float", nullable: false),
                    Precipitation = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SprayingLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SprayingLogs_SprayingAnnouncements_SprayingAnnouncementId",
                        column: x => x.SprayingAnnouncementId,
                        principalTable: "SprayingAnnouncements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TelemetryReadings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HiveId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false),
                    Humidity = table.Column<double>(type: "float", nullable: false),
                    InternalTemperature = table.Column<double>(type: "float", nullable: false),
                    BatteryLevel = table.Column<double>(type: "float", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelemetryReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TelemetryReadings_Hives_HiveId",
                        column: x => x.HiveId,
                        principalTable: "Hives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TelemetryReadings_IoTDevices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "IoTDevices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_UserId",
                table: "Alerts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Apiaries_BeekeeperId",
                table: "Apiaries",
                column: "BeekeeperId");

            migrationBuilder.CreateIndex(
                name: "IX_Crops_ParcelId",
                table: "Crops",
                column: "ParcelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HiveDiaryEntries_HiveId",
                table: "HiveDiaryEntries",
                column: "HiveId");

            migrationBuilder.CreateIndex(
                name: "IX_Hives_ApiaryId",
                table: "Hives",
                column: "ApiaryId");

            migrationBuilder.CreateIndex(
                name: "IX_IoTDevices_HiveId",
                table: "IoTDevices",
                column: "HiveId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IoTDevices_SerialNumber",
                table: "IoTDevices",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IoTDevices_UniqueDeviceId",
                table: "IoTDevices",
                column: "UniqueDeviceId",
                unique: true,
                filter: "[UniqueDeviceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Parcels_FarmerId",
                table: "Parcels",
                column: "FarmerId");

            migrationBuilder.CreateIndex(
                name: "IX_SprayingAnnouncements_ParcelId",
                table: "SprayingAnnouncements",
                column: "ParcelId");

            migrationBuilder.CreateIndex(
                name: "IX_SprayingLogs_SprayingAnnouncementId",
                table: "SprayingLogs",
                column: "SprayingAnnouncementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryReadings_DeviceId",
                table: "TelemetryReadings",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryReadings_HiveId_RecordedAt",
                table: "TelemetryReadings",
                columns: new[] { "HiveId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "Crops");

            migrationBuilder.DropTable(
                name: "HiveDiaryEntries");

            migrationBuilder.DropTable(
                name: "SprayingLogs");

            migrationBuilder.DropTable(
                name: "TelemetryReadings");

            migrationBuilder.DropTable(
                name: "SprayingAnnouncements");

            migrationBuilder.DropTable(
                name: "IoTDevices");

            migrationBuilder.DropTable(
                name: "Parcels");

            migrationBuilder.DropTable(
                name: "Hives");

            migrationBuilder.DropTable(
                name: "Apiaries");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
