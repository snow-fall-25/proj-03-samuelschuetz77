using System.Data;
using Dapper;

public static class DbSeeder
{
    public static void EnsureTables(IDbConnection db)
    {
        db.Execute("""
            CREATE TABLE IF NOT EXISTS People(
                Id INTEGER PRIMARY KEY,
                FullName TEXT,
                Alias TEXT NULL,
                Affiliation TEXT,
                Category TEXT,
                LocationId INT,
                RealPersonWiki TEXT NULL,
                AccessLevel INT
            );
        """);

        db.Execute("""
            CREATE TABLE IF NOT EXISTS Locations(
                Id INTEGER PRIMARY KEY,
                Name TEXT NULL,
                City TEXT,
                Country TEXT,
                Currency TEXT
            );
        """);

        db.Execute("""
            CREATE TABLE IF NOT EXISTS Protocols(
                Id INTEGER PRIMARY KEY,
                LocationId INT,
                Title TEXT,
                ConciseGuideline TEXT
            );
        """);
    }

    public static void SeedIfEmpty(IDbConnection db)
    {
        var count = db.ExecuteScalar<long>("SELECT COUNT(*) FROM People;");
        if (count > 0) return;

        // ---------- LOCATIONS (25) ----------
        var locations = new[]
        {
            new { Id=1,  Name=(string?)null, City="Belgrade", Country="Federal Republic of Yugoslavia", Currency="Yugoslav Dinar" },
            new { Id=2,  Name=(string?)null, City="Pale", Country="Bosnia and Herzegovina", Currency="Bosnian Dinar" },
            new { Id=3,  Name=(string?)null, City="Han Pijesak", Country="Bosnia and Herzegovina", Currency="Bosnian Dinar" },
            new { Id=4,  Name=(string?)null, City="Zagreb", Country="Croatia", Currency="Croatian Kuna" },
            new { Id=5,  Name=(string?)null, City="Sarajevo", Country="Bosnia and Herzegovina", Currency="Bosnian Dinar" },
            new { Id=6,  Name=(string?)null, City="Banja Luka", Country="Bosnia and Herzegovina", Currency="Bosnian Dinar" },
            new { Id=7,  Name=(string?)null, City="Knin", Country="Croatia", Currency="Croatian Kuna" },
            new { Id=8,  Name=(string?)null, City="Dečani", Country="Federal Republic of Yugoslavia", Currency="Yugoslav Dinar" },
            new { Id=9,  Name=(string?)null, City="Vukovar", Country="Croatia", Currency="Croatian Kuna" },
            new { Id=10, Name=(string?)null, City="Tuzla", Country="Bosnia and Herzegovina", Currency="Bosnian Dinar" },
            new { Id=11, Name=(string?)null, City="Bern", Country="Switzerland", Currency="Swiss Franc" },
            new { Id=12, Name=(string?)null, City="Zürich", Country="Switzerland", Currency="Swiss Franc" },
            new { Id=13, Name=(string?)null, City="Dayton", Country="United States", Currency="US Dollar" },
            new { Id=14, Name=(string?)null, City="New York", Country="United States", Currency="US Dollar" },
            new { Id=15, Name=(string?)null, City="Brussels", Country="Belgium", Currency="Belgian Franc" },
            new { Id=16, Name=(string?)null, City="Vlasenica", Country="Bosnia and Herzegovina", Currency="Bosnian Dinar" },
            new { Id=17, Name=(string?)null, City="Višegrad", Country="Bosnia and Herzegovina", Currency="Bosnian Dinar" },
            new { Id=18, Name=(string?)null, City="Zenica", Country="Bosnia and Herzegovina", Currency="Bosnian Dinar" },
            new { Id=19, Name=(string?)null, City="Mostar", Country="Bosnia and Herzegovina", Currency="Bosnian Dinar" },
            new { Id=20, Name=(string?)null, City="Bocinja", Country="Bosnia and Herzegovina", Currency="Bosnian Dinar" },
            new { Id=21, Name=(string?)"Holiday Inn", City="Sarajevo", Country="Bosnia and Herzegovina", Currency="Bosnian Dinar" },
            new { Id=22, Name=(string?)"Hotel Moskva Café", City="Belgrade", Country="Federal Republic of Yugoslavia", Currency="Yugoslav Dinar" },
            new { Id=23, Name=(string?)"Vilina Vlas Hotel", City="Višegrad", Country="Bosnia and Herzegovina", Currency="Bosnian Dinar" },
            new { Id=24, Name=(string?)"Kajak Club", City="Banja Luka", Country="Bosnia and Herzegovina", Currency="Bosnian Dinar" },
            new { Id=25, Name=(string?)"British Embassy", City="Zagreb", Country="Croatia", Currency="Croatian Kuna" }
        };
        db.Execute("INSERT INTO Locations (Id,Name,City,Country,Currency) VALUES (@Id,@Name,@City,@Country,@Currency);", locations);

        // ---------- PEOPLE (50) ----------
        var people = new[]
        {
            new { Id=1, FullName="Slobodan Milošević", Alias=(string?)"Sloba", Affiliation="Hostile", Category="Political Leader", LocationId=1, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Slobodan_Milo%C5%A1evi%C4%87", AccessLevel=5 },
            new { Id=2, FullName="Radovan Karadžić", Alias=(string?)null, Affiliation="Hostile", Category="Political Leader", LocationId=2, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Radovan_Karad%C5%BEi%C4%87", AccessLevel=9 },
            new { Id=3, FullName="Ratko Mladić", Alias=(string?)null, Affiliation="Hostile", Category="Military Leader", LocationId=3, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Ratko_Mladi%C4%87", AccessLevel=9 },
            new { Id=4, FullName="Vojislav Šešelj", Alias=(string?)null, Affiliation="Hostile", Category="Paramilitary Leader", LocationId=1, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Vojislav_%C5%A0e%C5%A1elj", AccessLevel=5 },
            new { Id=5, FullName="Franjo Tuđman", Alias=(string?)null, Affiliation="Friendly", Category="Political Leader", LocationId=4, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Franjo_Tu%C4%91man", AccessLevel=3 },
            new { Id=6, FullName="Alija Izetbegović", Alias=(string?)null, Affiliation="Friendly", Category="Political Leader", LocationId=5, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Alija_Izetbegovi%C4%87", AccessLevel=3 },
            new { Id=7, FullName="Biljana Plavšić", Alias=(string?)null, Affiliation="Hostile", Category="Political Leader", LocationId=6, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Biljana_Plav%C5%A1i%C4%87", AccessLevel=5 },
            new { Id=8, FullName="Ante Gotovina", Alias=(string?)null, Affiliation="Friendly", Category="Military Officer", LocationId=7, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Ante_Gotovina", AccessLevel=5 },
            new { Id=9, FullName="Ramush Haradinaj", Alias=(string?)null, Affiliation="Neutral", Category="Paramilitary Leader", LocationId=8, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Ramush_Haradinaj", AccessLevel=3 },
            new { Id=10, FullName="Goran Hadžić", Alias=(string?)null, Affiliation="Hostile", Category="Political Leader", LocationId=9, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Goran_Had%C5%BEi%C4%87", AccessLevel=5 },
            new { Id=11, FullName="Milan Babić", Alias=(string?)null, Affiliation="Hostile", Category="Political Leader", LocationId=7, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Milan_Babi%C4%87", AccessLevel=5 },
            new { Id=12, FullName="Momčilo Krajišnik", Alias=(string?)null, Affiliation="Hostile", Category="Political Leader", LocationId=2, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Mom%C4%8Dilo_Kraji%C5%A1nik", AccessLevel=5 },
            new { Id=13, FullName="Željko Ražnatović", Alias=(string?)"Arkan", Affiliation="Hostile", Category="Paramilitary Commander", LocationId=1, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Arkan", AccessLevel=5 },
            new { Id=14, FullName="Jovica Stanišić", Alias=(string?)null, Affiliation="Hostile", Category="Intelligence Officer", LocationId=1, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Jovica_Stani%C5%A1i%C4%87", AccessLevel=9 },
            new { Id=15, FullName="Franko Simatović", Alias=(string?)"Frenki", Affiliation="Hostile", Category="Intelligence Officer", LocationId=1, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Franko_Simatovi%C4%87", AccessLevel=9 },
            new { Id=16, FullName="Naser Orić", Alias=(string?)null, Affiliation="Friendly", Category="Military Officer", LocationId=10, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Naser_Ori%C4%87", AccessLevel=5 },
            new { Id=17, FullName="Fikret Abdić", Alias=(string?)"Babo", Affiliation="Unfriendly", Category="Warlord", LocationId=1, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Fikret_Abdi%C4%87", AccessLevel=3 },
            new { Id=18, FullName="Mirjana Marković", Alias=(string?)"Mira", Affiliation="Hostile", Category="Political Figure", LocationId=1, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Mirjana_Markovi%C4%87", AccessLevel=5 },
            new { Id=19, FullName="Madeleine Albright", Alias=(string?)null, Affiliation="Ally", Category="Diplomat", LocationId=14, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Madeleine_Albright", AccessLevel=3 },
            new { Id=20, FullName="Sonja Biserko", Alias=(string?)null, Affiliation="Friendly", Category="Activist", LocationId=1, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Sonja_Biserko", AccessLevel=5 },
            new { Id=21, FullName="Nataša Kandić", Alias=(string?)null, Affiliation="Friendly", Category="Activist", LocationId=1, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Nata%C5%A1a_Kandi%C4%87", AccessLevel=5 },
            new { Id=22, FullName="Svetlana Ražnatović", Alias=(string?)"Ceca", Affiliation="Unfriendly", Category="Public Figure", LocationId=1, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Svetlana_Ra%C5%BEnatovi%C4%87", AccessLevel=3 },
            new { Id=23, FullName="Carla Del Ponte", Alias=(string?)null, Affiliation="Friendly", Category="Prosecutor", LocationId=11, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Carla_Del_Ponte", AccessLevel=5 },
            new { Id=24, FullName="Hashim Thaçi", Alias=(string?)"The Snake", Affiliation="Neutral", Category="Paramilitary Leader", LocationId=12, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Hashim_Tha%C3%A7i", AccessLevel=3 },
            new { Id=25, FullName="Zoran Đinđić", Alias=(string?)null, Affiliation="Friendly", Category="Political Opposition", LocationId=1, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Zoran_%C4%90in%C4%91i%C4%87", AccessLevel=5 },
            new { Id=26, FullName="Milan Martić", Alias=(string?)null, Affiliation="Hostile", Category="Paramilitary Leader", LocationId=7, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Milan_Marti%C4%87", AccessLevel=5 },
            new { Id=27, FullName="Richard Holbrooke", Alias=(string?)null, Affiliation="Ally", Category="Diplomat", LocationId=13, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Richard_Holbrooke", AccessLevel=3 },
            new { Id=28, FullName="Wesley Clark", Alias=(string?)null, Affiliation="Ally", Category="Military Officer", LocationId=15, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Wesley_Clark", AccessLevel=3 },
            new { Id=29, FullName="Viktor Bout", Alias=(string?)"Merchant of Death", Affiliation="Hostile", Category="Arms Dealer", LocationId=1, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Viktor_Bout", AccessLevel=5 },
            new { Id=30, FullName="Radislav Krstić", Alias=(string?)null, Affiliation="Hostile", Category="Military Officer", LocationId=16, RealPersonWiki=(string?)"https://en.wikipedia.org/wiki/Radislav_Krsti%C4%87", AccessLevel=5 },
            new { Id=31, FullName="Samantha Collins", Alias=(string?)null, Affiliation="Friendly", Category="MI6 Field Agent", LocationId=21, RealPersonWiki=(string?)null, AccessLevel=9 },
            new { Id=32, FullName="Ian Richards", Alias=(string?)null, Affiliation="Friendly", Category="MI6 Station Chief", LocationId=25, RealPersonWiki=(string?)null, AccessLevel=9 },
            new { Id=33, FullName="Dragan Vukić", Alias=(string?)null, Affiliation="Hostile", Category="Paramilitary Commander", LocationId=23, RealPersonWiki=(string?)null, AccessLevel=5 },
            new { Id=34, FullName="Milica Petrova", Alias=(string?)null, Affiliation="Hostile", Category="GRU Operative", LocationId=22, RealPersonWiki=(string?)null, AccessLevel=9 },
            new { Id=35, FullName="James Wheeler", Alias=(string?)null, Affiliation="Friendly", Category="CIA Officer", LocationId=5, RealPersonWiki=(string?)null, AccessLevel=5 },
            new { Id=36, FullName="Fatima al-Sayeed", Alias=(string?)null, Affiliation="Neutral", Category="Foreign Volunteer Liaison", LocationId=18, RealPersonWiki=(string?)null, AccessLevel=3 },
            new { Id=37, FullName="Miroslav Petrić", Alias=(string?)null, Affiliation="Friendly", Category="HVO Officer", LocationId=19, RealPersonWiki=(string?)null, AccessLevel=3 },
            new { Id=38, FullName="Zoran Lukić", Alias=(string?)null, Affiliation="Hostile", Category="Crime Boss", LocationId=24, RealPersonWiki=(string?)null, AccessLevel=5 },
            new { Id=39, FullName="Ahmed Zubair", Alias=(string?)null, Affiliation="Neutral", Category="Foreign Fighter", LocationId=20, RealPersonWiki=(string?)null, AccessLevel=5 },
            new { Id=40, FullName="Dragomir Petrović", Alias=(string?)null, Affiliation="Hostile", Category="RS Intelligence Officer", LocationId=2, RealPersonWiki=(string?)null, AccessLevel=5 },
            new { Id=41, FullName="Lejla Kovačević", Alias=(string?)null, Affiliation="Friendly", Category="Informant", LocationId=5, RealPersonWiki=(string?)null, AccessLevel=5 },
            new { Id=42, FullName="Katarina Marković", Alias=(string?)null, Affiliation="Friendly", Category="Journalist (MI5 Source)", LocationId=1, RealPersonWiki=(string?)null, AccessLevel=5 },
            new { Id=43, FullName="Aida Selmanović", Alias=(string?)null, Affiliation="Friendly", Category="Witness / Informant", LocationId=10, RealPersonWiki=(string?)null, AccessLevel=3 },
            new { Id=44, FullName="Elena Ivanova", Alias=(string?)null, Affiliation="Hostile", Category="Military Advisor", LocationId=2, RealPersonWiki=(string?)null, AccessLevel=5 },
            new { Id=45, FullName="Ivanka Marinović", Alias=(string?)null, Affiliation="Friendly", Category="Liaison Officer", LocationId=4, RealPersonWiki=(string?)null, AccessLevel=3 },
            new { Id=46, FullName="Petar Jovanović", Alias=(string?)null, Affiliation="Hostile", Category="State Security Officer", LocationId=1, RealPersonWiki=(string?)null, AccessLevel=5 },
            new { Id=47, FullName="Marija Kovač", Alias=(string?)null, Affiliation="Friendly", Category="Counterintelligence Agent", LocationId=4, RealPersonWiki=(string?)null, AccessLevel=5 },
            new { Id=48, FullName="John Anderson", Alias=(string?)null, Affiliation="Friendly", Category="War Correspondent", LocationId=5, RealPersonWiki=(string?)null, AccessLevel=3 },
            new { Id=49, FullName="Mirela Hasanović", Alias=(string?)null, Affiliation="Friendly", Category="Aid Worker", LocationId=10, RealPersonWiki=(string?)null, AccessLevel=3 },
            new { Id=50, FullName="Jean Dupont", Alias=(string?)null, Affiliation="Neutral", Category="UN Observer", LocationId=5, RealPersonWiki=(string?)null, AccessLevel=3 }
        };
        db.Execute("INSERT INTO People (Id,FullName,Alias,Affiliation,Category,LocationId,RealPersonWiki,AccessLevel) VALUES (@Id,@FullName,@Alias,@Affiliation,@Category,@LocationId,@RealPersonWiki,@AccessLevel);", people);

        // ---------- PROTOCOLS (sample 3) ----------
        var protocols = new[]
        {
            new { Id=1, LocationId=21, Title="Sarajevo Press Zone", ConciseGuideline="Avoid contact outside Holiday Inn lobby; use journalist cover." },
            new { Id=2, LocationId=22, Title="Belgrade Café Watch", ConciseGuideline="Assume hostile surveillance at Hotel Moskva Café; meets <=15min." },
            new { Id=3, LocationId=25, Title="Zagreb Embassy Comms", ConciseGuideline="Only meet assets inside compound; no personal electronics." }
        };
        db.Execute("INSERT INTO Protocols (Id,LocationId,Title,ConciseGuideline) VALUES (@Id,@LocationId,@Title,@ConciseGuideline);", protocols);
    }
}
