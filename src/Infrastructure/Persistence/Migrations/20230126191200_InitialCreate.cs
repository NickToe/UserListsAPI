#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace UserListsAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Developers = table.Column<List<string>>(type: "text[]", nullable: true),
                    Genres = table.Column<List<string>>(type: "text[]", nullable: true),
                    Poster = table.Column<string>(type: "text", nullable: true),
                    MetacriticScore = table.Column<short>(type: "smallint", nullable: true),
                    MetacriticUrl = table.Column<string>(type: "text", nullable: true),
                    Publishers = table.Column<List<string>>(type: "text[]", nullable: true),
                    ComingSoon = table.Column<bool>(type: "boolean", nullable: true),
                    ReleaseDate = table.Column<string>(type: "text", nullable: true),
                    ShortDescription = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    ReviewScore = table.Column<string>(type: "text", nullable: true),
                    TotalPositive = table.Column<int>(type: "integer", nullable: true),
                    TotalNegative = table.Column<int>(type: "integer", nullable: true),
                    TotalReviews = table.Column<int>(type: "integer", nullable: true),
                    ItemStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Movies",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    FullTitle = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    Year = table.Column<string>(type: "text", nullable: true),
                    Poster = table.Column<string>(type: "text", nullable: false),
                    ReleaseDate = table.Column<string>(type: "text", nullable: true),
                    RuntimeMins = table.Column<string>(type: "text", nullable: true),
                    RuntimeStr = table.Column<string>(type: "text", nullable: true),
                    Plot = table.Column<string>(type: "text", nullable: true),
                    Directors = table.Column<string>(type: "text", nullable: true),
                    Stars = table.Column<string>(type: "text", nullable: true),
                    Genres = table.Column<string>(type: "text", nullable: true),
                    Companies = table.Column<string>(type: "text", nullable: true),
                    Countries = table.Column<string>(type: "text", nullable: true),
                    ContentRating = table.Column<string>(type: "text", nullable: true),
                    ImdbRating = table.Column<string>(type: "text", nullable: true),
                    ImdbRatingVotes = table.Column<string>(type: "text", nullable: true),
                    MetascriticRating = table.Column<string>(type: "text", nullable: true),
                    ItemStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Games_Id",
                table: "Games",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Games_Title",
                table: "Games",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_Id",
                table: "Movies",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_Title",
                table: "Movies",
                column: "Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Movies");
        }
    }
}
