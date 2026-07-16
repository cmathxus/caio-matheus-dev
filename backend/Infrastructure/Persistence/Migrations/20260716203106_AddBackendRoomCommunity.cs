using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CaioMatheusDev.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBackendRoomCommunity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "backend_room_community_posts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Caption = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    DataUrl = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_backend_room_community_posts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_backend_room_community_posts_auth_users_UserId",
                        column: x => x.UserId,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_backend_room_community_posts_CreatedAt",
                table: "backend_room_community_posts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_backend_room_community_posts_UserId_CreatedAt",
                table: "backend_room_community_posts",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "backend_room_community_posts");
        }
    }
}
