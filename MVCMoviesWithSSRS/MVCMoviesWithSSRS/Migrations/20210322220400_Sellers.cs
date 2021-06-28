using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MVCMoviesWithSSRS.Migrations
{
    public partial class Sellers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Movie",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: true),
                    ReleaseDate = table.Column<DateTime>(nullable: false),
                    Genre = table.Column<string>(nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18, 2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movie", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Seller",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    URL = table.Column<string>(nullable: true),
                    Address1 = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    State = table.Column<string>(nullable: true),
                    Zip = table.Column<string>(nullable: true),
                    Phone = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seller", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MoviePrices",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovieId = table.Column<int>(nullable: false),
                    DateEntered = table.Column<DateTime>(nullable: false),
                    SellerId = table.Column<int>(nullable: false),
                    Price = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoviePrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MoviePrices_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MoviePrices_MovieId",
                table: "MoviePrices",
                column: "MovieId");
//Ensure Stored Procs
            migrationBuilder.Sql(@"
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[AddNewMovieFromPage]
	@Title nvarchar(max), 
	@ReleaseDate datetime2(7),
	@Genre nvarchar(max),
	@MSRP decimal(18,2),
	@SellerName varchar(150),
	@URL varchar(max),
	@Address nvarchar(200),
	@City nvarchar(200),
	@State nvarchar(50),
	@Zip nvarchar(20),
	@Phone nvarchar(20),
	@SellerPrice decimal(18,2)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @sellerID int;
	declare @movieId int;

   INSERT INTO [dbo].[Movie]
           ([Title]
           ,[ReleaseDate]
           ,[Genre]
           ,[Price])
     VALUES
           (@Title,
		   @ReleaseDate,
		   @Genre,
		   @MSRP);
	

	set @movieId = SCOPE_IDENTITY();

	--If seller infomration was entered, save that as well
	if @SellerName IS NOT NULL
	BEGIN
			

		INSERT INTO [dbo].[Seller]
           ([Name]
           ,[URL]
           ,[Address1]
           ,[City]
           ,[State]
           ,[Zip]
           ,[Phone])
		VALUES
           (@SellerName,
		   @URL,
		   @Address,
		   @City,
		   @State,
		   @Zip,
		   @Phone)

		   set @sellerID = SCOPE_IDENTITY()

		   if @SellerPrice IS NOT NULL
			BEGIN
	
				INSERT INTO [dbo].[MoviePrices]
				   ([MovieId]
				   ,[DateEntered]
				   ,[SellerId]
				   ,[Price])
				 VALUES
					   (@movieId,
					   Getdate(),
					   @sellerID,
					   @SellerPrice)

			 END
	END


END
GO");

            migrationBuilder.Sql(@"CREATE OR ALTER PROCEDURE GetDuplicatesForSeller
(@sellerID int)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.

    SET NOCOUNT ON;
            declare @targetName varchar(max)

    declare @duplicates as Table(
    SellerId int,
    SellerName varchar(max)
	)
    --get target name

    SELECT @targetName = LOWER(LTRIM(RTRIM(Name)))

    FROM Seller

    WHERE Id = @sellerID

    --find others with same name
    Insert into @duplicates

    Select id, name from Seller
    WHERE id <> @sellerID

    AND LOWER(LTRIM(RTRIM(name))) = @targetName

    --TODO Insert other more advanced matching techniques, 
	--like address matches


    --return results

    select distinct s.*
    from @duplicates d INNER JOIN Seller s
    on d.SellerId = s.Id

END
GO");

            migrationBuilder.Sql(@"
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER FUNCTION GetSellerMoviesReport
(
)
RETURNS TABLE
AS
RETURN
(
      Select s.id as SellerId, s.Name as SellerName,
      m.Title as MovieTitle, m.Id as MovieId,
      p.Price as MoviePrice

      from
      seller s inner
      join MoviePrices p

on s.id = p.SellerId

inner join movie m

on p.MovieId = m.Id
)
GO");

        }

        

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MoviePrices");

            migrationBuilder.DropTable(
                name: "Seller");

            migrationBuilder.DropTable(
                name: "Movie");
        }
    }
}
