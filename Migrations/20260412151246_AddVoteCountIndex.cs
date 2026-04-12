using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TMDB_API.Migrations
{
    /// <inheritdoc />
    public partial class AddVoteCountIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Movies_vote_count",
                table: "Movies",
                column: "vote_count");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Movies_vote_count",
                table: "Movies");
        }
    }
}
