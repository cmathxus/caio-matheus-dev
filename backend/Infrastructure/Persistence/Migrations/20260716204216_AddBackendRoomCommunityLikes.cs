using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CaioMatheusDev.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBackendRoomCommunityLikes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExpiresAt",
                table: "backend_room_community_posts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW() + INTERVAL '24 hours'");

            migrationBuilder.CreateTable(
                name: "backend_room_community_post_likes",
                columns: table => new
                {
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_backend_room_community_post_likes", x => new { x.PostId, x.UserId });
                    table.ForeignKey(
                        name: "FK_backend_room_community_post_likes_auth_users_UserId",
                        column: x => x.UserId,
                        principalTable: "auth_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_backend_room_community_post_likes_backend_room_community_po~",
                        column: x => x.PostId,
                        principalTable: "backend_room_community_posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_backend_room_community_posts_ExpiresAt",
                table: "backend_room_community_posts",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_backend_room_community_post_likes_UserId",
                table: "backend_room_community_post_likes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "backend_room_community_post_likes");

            migrationBuilder.DropIndex(
                name: "IX_backend_room_community_posts_ExpiresAt",
                table: "backend_room_community_posts");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "backend_room_community_posts");
        }
    }
}
