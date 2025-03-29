using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalkLikeTv.EntityModels.Migrations
{
    /// <inheritdoc />
    public partial class InitialBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    LanguageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Platform = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    NativeName = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Language__B938558B157F7949", x => x.LanguageID);
                });

            migrationBuilder.CreateTable(
                name: "Personalities",
                columns: table => new
                {
                    PersonalityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonalityName = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Personal__CD053C54F187B8DC", x => x.PersonalityID);
                });

            migrationBuilder.CreateTable(
                name: "Scenarios",
                columns: table => new
                {
                    ScenarioID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScenarioName = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Scenario__0DF6D1A39A1B5B0E", x => x.ScenarioID);
                });

            migrationBuilder.CreateTable(
                name: "Styles",
                columns: table => new
                {
                    StyleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StyleName = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Styles__8AD147A057377767", x => x.StyleID);
                });

            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    TokenID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Hash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Tokens__658FEE8A40067671", x => x.TokenID);
                });

            migrationBuilder.CreateTable(
                name: "Titles",
                columns: table => new
                {
                    TitleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TitleName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NumPhrases = table.Column<int>(type: "int", nullable: false),
                    OriginalLanguageID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Titles__757589E6D98CFFD1", x => x.TitleID);
                    table.ForeignKey(
                        name: "FK__Titles__Original__5748DA5E",
                        column: x => x.OriginalLanguageID,
                        principalTable: "Languages",
                        principalColumn: "LanguageID");
                });

            migrationBuilder.CreateTable(
                name: "Voices",
                columns: table => new
                {
                    VoiceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Platform = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    LanguageID = table.Column<int>(type: "int", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    LocalName = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Locale = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    LocaleName = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SampleRateHertz = table.Column<int>(type: "int", nullable: false),
                    VoiceType = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    WordsPerMinute = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Voices__D870D587186CF99C", x => x.VoiceID);
                    table.ForeignKey(
                        name: "FK__Voices__Language__3F7150CD",
                        column: x => x.LanguageID,
                        principalTable: "Languages",
                        principalColumn: "LanguageID");
                });

            migrationBuilder.CreateTable(
                name: "Phrases",
                columns: table => new
                {
                    PhraseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TitleID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Phrases__0DBA0EA20BE30369", x => x.PhraseID);
                    table.ForeignKey(
                        name: "FK__Phrases__TitleID__5A254709",
                        column: x => x.TitleID,
                        principalTable: "Titles",
                        principalColumn: "TitleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VoicePersonalities",
                columns: table => new
                {
                    VoiceID = table.Column<int>(type: "int", nullable: false),
                    PersonalityID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__VoicePer__84A08642C51F02F2", x => new { x.VoiceID, x.PersonalityID });
                    table.ForeignKey(
                        name: "FK__VoicePers__Perso__5378497A",
                        column: x => x.PersonalityID,
                        principalTable: "Personalities",
                        principalColumn: "PersonalityID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__VoicePers__Voice__52842541",
                        column: x => x.VoiceID,
                        principalTable: "Voices",
                        principalColumn: "VoiceID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VoiceScenarios",
                columns: table => new
                {
                    VoiceID = table.Column<int>(type: "int", nullable: false),
                    ScenarioID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__VoiceSce__F8AFB89D9B0E5462", x => new { x.VoiceID, x.ScenarioID });
                    table.ForeignKey(
                        name: "FK__VoiceScen__Scena__4CCB4BEB",
                        column: x => x.ScenarioID,
                        principalTable: "Scenarios",
                        principalColumn: "ScenarioID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__VoiceScen__Voice__4BD727B2",
                        column: x => x.VoiceID,
                        principalTable: "Voices",
                        principalColumn: "VoiceID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VoiceStyles",
                columns: table => new
                {
                    VoiceID = table.Column<int>(type: "int", nullable: false),
                    StyleID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__VoiceSty__C0DDC1FD59FFE951", x => new { x.VoiceID, x.StyleID });
                    table.ForeignKey(
                        name: "FK__VoiceStyl__Style__461E4E5C",
                        column: x => x.StyleID,
                        principalTable: "Styles",
                        principalColumn: "StyleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__VoiceStyl__Voice__452A2A23",
                        column: x => x.VoiceID,
                        principalTable: "Voices",
                        principalColumn: "VoiceID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Translates",
                columns: table => new
                {
                    PhraseID = table.Column<int>(type: "int", nullable: false),
                    LanguageID = table.Column<int>(type: "int", nullable: false),
                    Phrase = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PhraseHint = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Translat__A6298BFA0F5D3F2A", x => new { x.PhraseID, x.LanguageID });
                    table.ForeignKey(
                        name: "FK__Translate__Langu__5DF5D7ED",
                        column: x => x.LanguageID,
                        principalTable: "Languages",
                        principalColumn: "LanguageID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__Translate__Phras__5D01B3B4",
                        column: x => x.PhraseID,
                        principalTable: "Phrases",
                        principalColumn: "PhraseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ__Language__737584F60620E8D3",
                table: "Languages",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Language__C4516413E29C3320",
                table: "Languages",
                column: "Tag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Personal__6DD1E173CBB571F5",
                table: "Personalities",
                column: "PersonalityName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Phrases_TitleID",
                table: "Phrases",
                column: "TitleID");

            migrationBuilder.CreateIndex(
                name: "UQ__Scenario__ADC5B11D78260FCB",
                table: "Scenarios",
                column: "ScenarioName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Styles__23564EE60ED94D48",
                table: "Styles",
                column: "StyleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Titles_OriginalLanguageID",
                table: "Titles",
                column: "OriginalLanguageID");

            migrationBuilder.CreateIndex(
                name: "UQ__Titles__252BE89C0BF9B926",
                table: "Titles",
                column: "TitleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Translates_LanguageID",
                table: "Translates",
                column: "LanguageID");

            migrationBuilder.CreateIndex(
                name: "IX_VoicePersonalities_PersonalityID",
                table: "VoicePersonalities",
                column: "PersonalityID");

            migrationBuilder.CreateIndex(
                name: "IX_Voices_LanguageID",
                table: "Voices",
                column: "LanguageID");

            migrationBuilder.CreateIndex(
                name: "UQ__Voices__A6160FD1C3922DD5",
                table: "Voices",
                column: "ShortName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VoiceScenarios_ScenarioID",
                table: "VoiceScenarios",
                column: "ScenarioID");

            migrationBuilder.CreateIndex(
                name: "IX_VoiceStyles_StyleID",
                table: "VoiceStyles",
                column: "StyleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tokens");

            migrationBuilder.DropTable(
                name: "Translates");

            migrationBuilder.DropTable(
                name: "VoicePersonalities");

            migrationBuilder.DropTable(
                name: "VoiceScenarios");

            migrationBuilder.DropTable(
                name: "VoiceStyles");

            migrationBuilder.DropTable(
                name: "Phrases");

            migrationBuilder.DropTable(
                name: "Personalities");

            migrationBuilder.DropTable(
                name: "Scenarios");

            migrationBuilder.DropTable(
                name: "Styles");

            migrationBuilder.DropTable(
                name: "Voices");

            migrationBuilder.DropTable(
                name: "Titles");

            migrationBuilder.DropTable(
                name: "Languages");
        }
    }
}
