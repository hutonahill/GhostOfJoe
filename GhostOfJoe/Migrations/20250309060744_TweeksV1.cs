using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GhostOfJoe.Migrations
{
    /// <inheritdoc />
    public partial class TweeksV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bills",
                columns: table => new
                {
                    bill_id = table.Column<int>(type: "INTEGER", nullable: false),
                    short_name = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bills", x => x.bill_id);
                });

            migrationBuilder.CreateTable(
                name: "gender",
                columns: table => new
                {
                    gender_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gender", x => x.gender_id);
                });

            migrationBuilder.CreateTable(
                name: "metric",
                columns: table => new
                {
                    metric_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metric", x => x.metric_id);
                });

            migrationBuilder.CreateTable(
                name: "metric_score_set",
                columns: table => new
                {
                    score_set_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metric_score_set", x => x.score_set_id);
                });

            migrationBuilder.CreateTable(
                name: "metric_weight_set",
                columns: table => new
                {
                    weight_set_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metric_weight_set", x => x.weight_set_id);
                });

            migrationBuilder.CreateTable(
                name: "office",
                columns: table => new
                {
                    office_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_office", x => x.office_id);
                });

            migrationBuilder.CreateTable(
                name: "party",
                columns: table => new
                {
                    party_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_party", x => x.party_id);
                });

            migrationBuilder.CreateTable(
                name: "querks",
                columns: table => new
                {
                    querk_id = table.Column<int>(type: "INTEGER", nullable: false),
                    name = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_querks", x => x.querk_id);
                });

            migrationBuilder.CreateTable(
                name: "race",
                columns: table => new
                {
                    race_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_race", x => x.race_id);
                });

            migrationBuilder.CreateTable(
                name: "religon",
                columns: table => new
                {
                    religon_id = table.Column<int>(type: "INTEGER", nullable: false),
                    name = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_religon", x => x.religon_id);
                });

            migrationBuilder.CreateTable(
                name: "servers",
                columns: table => new
                {
                    server_id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    safeFlow = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servers", x => x.server_id);
                });

            migrationBuilder.CreateTable(
                name: "vote_type",
                columns: table => new
                {
                    vote_type_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vote_type", x => x.vote_type_id);
                });

            migrationBuilder.CreateTable(
                name: "bill_has_metric",
                columns: table => new
                {
                    score_set_id = table.Column<int>(type: "INTEGER", nullable: false),
                    bill_id = table.Column<int>(type: "INTEGER", nullable: false),
                    metric_id = table.Column<int>(type: "INTEGER", nullable: false),
                    score = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_bill_has_metric_bills_bill_id",
                        column: x => x.bill_id,
                        principalTable: "bills",
                        principalColumn: "bill_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bill_has_metric_metric_metric_id",
                        column: x => x.metric_id,
                        principalTable: "metric",
                        principalColumn: "metric_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bill_has_metric_metric_score_set_score_set_id",
                        column: x => x.score_set_id,
                        principalTable: "metric_score_set",
                        principalColumn: "score_set_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "metric_has_weights",
                columns: table => new
                {
                    metric_id = table.Column<int>(type: "INTEGER", nullable: false),
                    weight_set_id = table.Column<int>(type: "INTEGER", nullable: false),
                    weight = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_metric_has_weights_metric_metric_id",
                        column: x => x.metric_id,
                        principalTable: "metric",
                        principalColumn: "metric_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_metric_has_weights_metric_weight_set_weight_set_id",
                        column: x => x.weight_set_id,
                        principalTable: "metric_weight_set",
                        principalColumn: "weight_set_id");
                });

            migrationBuilder.CreateTable(
                name: "representitives",
                columns: table => new
                {
                    rep_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    first_name = table.Column<string>(type: "TEXT", nullable: false),
                    last_name = table.Column<string>(type: "TEXT", nullable: false),
                    phonetic = table.Column<string>(type: "TEXT", nullable: false),
                    ipa = table.Column<string>(type: "TEXT", nullable: false),
                    code = table.Column<string>(type: "TEXT", nullable: false),
                    first_elected = table.Column<string>(type: "TEXT", nullable: false),
                    birth_year = table.Column<int>(type: "INTEGER", nullable: false),
                    lgbt = table.Column<int>(type: "INTEGER", nullable: false),
                    race_id = table.Column<int>(type: "INTEGER", nullable: false),
                    office_id = table.Column<int>(type: "INTEGER", nullable: false),
                    party_id = table.Column<int>(type: "INTEGER", nullable: false),
                    gender_id = table.Column<int>(type: "INTEGER", nullable: false),
                    religon_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_representitives", x => x.rep_id);
                    table.ForeignKey(
                        name: "FK_representitives_gender_gender_id",
                        column: x => x.gender_id,
                        principalTable: "gender",
                        principalColumn: "gender_id");
                    table.ForeignKey(
                        name: "FK_representitives_office_office_id",
                        column: x => x.office_id,
                        principalTable: "office",
                        principalColumn: "office_id");
                    table.ForeignKey(
                        name: "FK_representitives_party_party_id",
                        column: x => x.party_id,
                        principalTable: "party",
                        principalColumn: "party_id");
                    table.ForeignKey(
                        name: "FK_representitives_race_race_id",
                        column: x => x.race_id,
                        principalTable: "race",
                        principalColumn: "race_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_representitives_religon_religon_id",
                        column: x => x.religon_id,
                        principalTable: "religon",
                        principalColumn: "religon_id");
                });

            migrationBuilder.CreateTable(
                name: "games",
                columns: table => new
                {
                    game_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    server_id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    title = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_games", x => x.game_id);
                    table.ForeignKey(
                        name: "FK_games_servers_server_id",
                        column: x => x.server_id,
                        principalTable: "servers",
                        principalColumn: "server_id");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    discordUser_id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    server_id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    preferred_score_set_id = table.Column<int>(type: "INTEGER", nullable: true),
                    preferred_weight_set_id = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_users_metric_score_set_preferred_score_set_id",
                        column: x => x.preferred_score_set_id,
                        principalTable: "metric_score_set",
                        principalColumn: "score_set_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_users_metric_weight_set_preferred_weight_set_id",
                        column: x => x.preferred_weight_set_id,
                        principalTable: "metric_weight_set",
                        principalColumn: "weight_set_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_users_servers_server_id",
                        column: x => x.server_id,
                        principalTable: "servers",
                        principalColumn: "server_id");
                });

            migrationBuilder.CreateTable(
                name: "rep_has_querk",
                columns: table => new
                {
                    rep_id = table.Column<int>(type: "INTEGER", nullable: false),
                    querk_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_rep_has_querk_querks_querk_id",
                        column: x => x.querk_id,
                        principalTable: "querks",
                        principalColumn: "querk_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rep_has_querk_representitives_rep_id",
                        column: x => x.rep_id,
                        principalTable: "representitives",
                        principalColumn: "rep_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "representitive_has_bills",
                columns: table => new
                {
                    rep_id = table.Column<int>(type: "INTEGER", nullable: false),
                    vote_type_id = table.Column<int>(type: "INTEGER", nullable: false),
                    bill_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_representitive_has_bills_bills_bill_id",
                        column: x => x.bill_id,
                        principalTable: "bills",
                        principalColumn: "bill_id");
                    table.ForeignKey(
                        name: "FK_representitive_has_bills_representitives_rep_id",
                        column: x => x.rep_id,
                        principalTable: "representitives",
                        principalColumn: "rep_id");
                    table.ForeignKey(
                        name: "FK_representitive_has_bills_vote_type_vote_type_id",
                        column: x => x.vote_type_id,
                        principalTable: "vote_type",
                        principalColumn: "vote_type_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    game_id = table.Column<int>(type: "INTEGER", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    unit = table.Column<string>(type: "TEXT", nullable: false),
                    higherBetter = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.category_id);
                    table.ForeignKey(
                        name: "FK_categories_games_game_id",
                        column: x => x.game_id,
                        principalTable: "games",
                        principalColumn: "game_id");
                });

            migrationBuilder.CreateTable(
                name: "titles",
                columns: table => new
                {
                    title_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    title = table.Column<string>(type: "TEXT", nullable: false),
                    user_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_titles", x => x.title_id);
                    table.ForeignKey(
                        name: "FK_titles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "scores",
                columns: table => new
                {
                    score_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    value = table.Column<double>(type: "NUMERIC (10, 1)", nullable: false),
                    category_id = table.Column<int>(type: "INTEGER", nullable: false),
                    user_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scores", x => x.score_id);
                    table.ForeignKey(
                        name: "FK_scores_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "category_id");
                    table.ForeignKey(
                        name: "FK_scores_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_bill_has_metric_bill_id",
                table: "bill_has_metric",
                column: "bill_id");

            migrationBuilder.CreateIndex(
                name: "IX_bill_has_metric_metric_id",
                table: "bill_has_metric",
                column: "metric_id");

            migrationBuilder.CreateIndex(
                name: "IX_bill_has_metric_score_set_id",
                table: "bill_has_metric",
                column: "score_set_id");

            migrationBuilder.CreateIndex(
                name: "IX_categories_game_id",
                table: "categories",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_games_server_id",
                table: "games",
                column: "server_id");

            migrationBuilder.CreateIndex(
                name: "IX_metric_has_weights_metric_id",
                table: "metric_has_weights",
                column: "metric_id");

            migrationBuilder.CreateIndex(
                name: "IX_metric_has_weights_weight_set_id",
                table: "metric_has_weights",
                column: "weight_set_id");

            migrationBuilder.CreateIndex(
                name: "IX_rep_has_querk_querk_id",
                table: "rep_has_querk",
                column: "querk_id");

            migrationBuilder.CreateIndex(
                name: "IX_rep_has_querk_rep_id",
                table: "rep_has_querk",
                column: "rep_id");

            migrationBuilder.CreateIndex(
                name: "IX_representitive_has_bills_bill_id",
                table: "representitive_has_bills",
                column: "bill_id");

            migrationBuilder.CreateIndex(
                name: "IX_representitive_has_bills_rep_id",
                table: "representitive_has_bills",
                column: "rep_id");

            migrationBuilder.CreateIndex(
                name: "IX_representitive_has_bills_vote_type_id",
                table: "representitive_has_bills",
                column: "vote_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_representitives_gender_id",
                table: "representitives",
                column: "gender_id");

            migrationBuilder.CreateIndex(
                name: "IX_representitives_office_id",
                table: "representitives",
                column: "office_id");

            migrationBuilder.CreateIndex(
                name: "IX_representitives_party_id",
                table: "representitives",
                column: "party_id");

            migrationBuilder.CreateIndex(
                name: "IX_representitives_race_id",
                table: "representitives",
                column: "race_id");

            migrationBuilder.CreateIndex(
                name: "IX_representitives_religon_id",
                table: "representitives",
                column: "religon_id");

            migrationBuilder.CreateIndex(
                name: "IX_scores_category_id",
                table: "scores",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_scores_user_id",
                table: "scores",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_titles_user_id",
                table: "titles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_preferred_score_set_id",
                table: "users",
                column: "preferred_score_set_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_preferred_weight_set_id",
                table: "users",
                column: "preferred_weight_set_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_server_id",
                table: "users",
                column: "server_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bill_has_metric");

            migrationBuilder.DropTable(
                name: "metric_has_weights");

            migrationBuilder.DropTable(
                name: "rep_has_querk");

            migrationBuilder.DropTable(
                name: "representitive_has_bills");

            migrationBuilder.DropTable(
                name: "scores");

            migrationBuilder.DropTable(
                name: "titles");

            migrationBuilder.DropTable(
                name: "metric");

            migrationBuilder.DropTable(
                name: "querks");

            migrationBuilder.DropTable(
                name: "bills");

            migrationBuilder.DropTable(
                name: "representitives");

            migrationBuilder.DropTable(
                name: "vote_type");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "gender");

            migrationBuilder.DropTable(
                name: "office");

            migrationBuilder.DropTable(
                name: "party");

            migrationBuilder.DropTable(
                name: "race");

            migrationBuilder.DropTable(
                name: "religon");

            migrationBuilder.DropTable(
                name: "games");

            migrationBuilder.DropTable(
                name: "metric_score_set");

            migrationBuilder.DropTable(
                name: "metric_weight_set");

            migrationBuilder.DropTable(
                name: "servers");
        }
    }
}
