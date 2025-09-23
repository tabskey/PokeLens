using PokeLens.Models;

namespace PokeLens.Services.mappers;

public static class HeartGoldSoulSilverMapper
{
    public static readonly Dictionary<int, LocationArea> Locations = new()
    {
        { 1, new LocationArea {
            Name = "new-bark-town-area",
            DisplayName = "New Bark Town"
        }},
        { 2, new LocationArea {
            Name = "johto-route-29-area",
            DisplayName = "Route 29"
        }},
        { 3, new LocationArea { 
            Name = "cherrygrove-city-area", 
            DisplayName = "Cherrygrove City"
        }},
        { 4, new LocationArea { 
            Name = "johto-route-30-area", 
            DisplayName = "Route 30"
        }},
        { 5, new LocationArea { 
            Name = "johto-route-31-area", 
            DisplayName = "Route 31"
        }},
        { 6, new LocationArea { 
            Name = "violet-city-area", 
            DisplayName = "Violet City"
        }},
        { 7, new LocationArea { 
            Name = "sprout-tower-area", 
            DisplayName = "Sprout Tower"
        }},
        { 8, new LocationArea { 
            Name = "johto-route-32-area", 
            DisplayName = "Route 32"
        }},
        { 9, new LocationArea { 
            Name = "ruins-of-alph-interior-d", 
            DisplayName = "Ruins of Alph"
        }},
        { 10, new LocationArea { 
            Name = "union-cave", 
            DisplayName = "Union Cave"
        }},
        { 11, new LocationArea {
            Name = "johto-route-33-area",
            DisplayName = "Route 33"
        }},
        { 12, new LocationArea { 
            Name = "azalea-town-area", 
            DisplayName = "Azalea Town"
        }},
        { 13, new LocationArea { 
            Name = "slowpoke-well", 
            DisplayName = "Slowpoke Well"
        }},
        { 14, new LocationArea { 
            Name = "ilex-forest-area", 
            DisplayName = "Ilex Forest"
        }},
        { 15, new LocationArea { 
            Name = "johto-route-34-area", 
            DisplayName = "Route 34"
        }},
        { 16, new LocationArea { 
            Name = "goldenrod-city-area", 
            DisplayName = "Goldenrod City"
        }},
        { 17, new LocationArea { 
            Name = "johto-route-35-area", 
            DisplayName = "Route 35"
        }},
        { 18, new LocationArea { 
            Name = "national-park-area", 
            DisplayName = "National Park"
        }},
        { 19, new LocationArea { 
            Name = "johto-route-36-area", 
            DisplayName = "Route 36"
        }},
        { 20, new LocationArea { 
            Name = "johto-route-37-area", 
            DisplayName = "Route 37"
        }},
        { 21, new LocationArea { 
            Name = "ecruteak-city-area", 
            DisplayName = "Ecruteak City"
        }},
        { 22, new LocationArea { 
            Name = "burned-tower-1f", 
            DisplayName = "Burned Tower"
        }},
        { 23, new LocationArea { 
            Name = "burned-tower-b1f", 
            DisplayName = "Burned Tower(B)"
        }},
        { 24, new LocationArea { 
            Name = "johto-route-38-area", 
            DisplayName = "Route 38"
        }},
        { 25, new LocationArea { 
            Name = "johto-route-39-area", 
            DisplayName = "Route 39"
        }},
        { 26, new LocationArea { 
            Name = "olivine-city-area", 
            DisplayName = "Olivine City"
        }},
        { 27, new LocationArea { 
            Name = "johto-lighthouse", 
            DisplayName = "Lighthouse"
        }},
        { 28, new LocationArea { 
            Name = "johto-route-40-area", 
            DisplayName = "Route 40"
        }},
        { 29, new LocationArea { 
            Name = "johto-route-41-area", 
            DisplayName = "Route 41"
        }},
        { 30, new LocationArea { 
            Name = "cianwood-city-area", 
            DisplayName = "Cianwood City"
        }},
        { 31, new LocationArea { 
            Name = "johto-route-42-area", 
            DisplayName = "Route 42"
        }},
        { 32, new LocationArea { 
            Name = "mt-mortar", 
            DisplayName = "Mt. Mortar"
        }},
        { 33, new LocationArea { 
            Name = "mahogany-town-area", 
            DisplayName = "Mahogany Town"
        }},
        { 34, new LocationArea { 
            Name = "johto-route-43-area", 
            DisplayName = "Route 43"
        }},
        { 35, new LocationArea { 
            Name = "lake-of-rage", 
            DisplayName = "Lake of Rage"
        }},
        { 36, new LocationArea { 
            Name = "team-rocket-hq-area", 
            DisplayName = "Team Rocket HQ"
        }},
        { 37, new LocationArea { 
            Name = "goldenrod-tunnel", 
            DisplayName = "Goldenrod Tunnel"
        }},
        { 38, new LocationArea { 
            Name = "johto-route-44-area", 
            DisplayName = "Route 44"
        }},
        { 39, new LocationArea { 
            Name = "ice-path", 
            DisplayName = "Ice Path"
        }},
        { 40, new LocationArea { 
            Name = "blackthorn-city-area", 
            DisplayName = "Blackthorn City"
        }},
        { 41, new LocationArea { 
            Name = "dragons-den-area", 
            DisplayName = "Dragon's Den"
        }},
        { 42, new LocationArea { 
            Name = "johto-route-45-area", 
            DisplayName = "Route 45"
        }},
        { 43, new LocationArea {
            Name = "dark-cave-violet-city-entrance",
            DisplayName = "Dark Cave (Violet City entrance)"
        }},
        { 44, new LocationArea {
            Name = "dark-cave-blackthorn-city-entrance",
            DisplayName = "Dark Cave (Blackthorn City entrance)"
        }},
        { 45, new LocationArea { 
            Name = "johto-route-46-area", 
            DisplayName = "Route 46"
        }},
        //Pokemon-tower??? kkkkkkk
        { 46, new LocationArea {
            Name = "bell-tower-1f",
            DisplayName = "Bell Tower" 
        }},
        
        { 47, new LocationArea {
            Name = "bell-tower-2f",
            DisplayName = "Bell Tower(2F)" 
        }},
        { 48, new LocationArea {
            Name = "bell-tower-3f",
            DisplayName = "Bell Tower(3F)" 
        }},
        { 49, new LocationArea {
            Name = "bell-tower-4f",
            DisplayName = "Bell Tower(4F)" 
        }},
        { 50, new LocationArea {
            Name = "bell-tower-5f",
            DisplayName = "Bell Tower(5F)" 
        }},
        { 51, new LocationArea {
            Name = "bell-tower-6f",
            DisplayName = "Bell Tower(6F)" 
        }},
        { 52, new LocationArea {
            Name = "bell-tower-7f",
            DisplayName = "Bell Tower(7F)" 
        }},
        { 53, new LocationArea {
            Name = "bell-tower-8f",
            DisplayName = "Bell Tower(8F)" 
        }},
        { 54, new LocationArea {
            Name = "bell-tower-9f",
            DisplayName = "Bell Tower(9F)" 
        }},
        { 55, new LocationArea {
            Name = "bell-tower-10f",
            DisplayName = "Bell Tower(10F)" 
        }},
        { 56, new LocationArea {
            Name = "bell-tower-roof",
            DisplayName = "Bell Tower Roof" 
        }},
        { 57, new LocationArea { 
            Name = "whirl-islands-1f", 
            DisplayName = "Whirl Islands(1F)"
        }},
        { 58, new LocationArea { 
            Name = "whirl-islands-b1f", 
            DisplayName = "Whirl Islands(B1F)"
        }},
        { 59, new LocationArea { 
            Name = "whirl-islands-empty-floor-x", 
            DisplayName = "Whirl Islands(Empty Floor X)"
        }},
        { 60, new LocationArea { 
            Name = "whirl-islands-b2f", 
            DisplayName = "Whirl Islands(B2F)"
        }},
        { 61, new LocationArea { 
            Name = "whirl-islands-empty-floor-y", 
            DisplayName = "Whirl Islands(Empty Floor Y)"
        }},
        { 62, new LocationArea { 
            Name = "whirl-islands-b3f", 
            DisplayName = "Whirl Islands(B3F)"
        }},
{ 63, new LocationArea { 
    Name = "kanto-route-27-area", 
    DisplayName = "Route 27"
}},
{ 64, new LocationArea {
    Name = "ss-aqua",
    DisplayName = "SS Aqua"
}},
{ 65, new LocationArea {
    Name = "kanto-victory-road-1-3f",
    DisplayName = "Victory Road"
}},
{ 66, new LocationArea { 
    Name = "indigo-plateau", 
    DisplayName = "Indigo Plateau"
}},
{ 67, new LocationArea { 
    Name = "kanto-route-6-area", 
    DisplayName = "Route 06"
}},
{ 68, new LocationArea {
    Name = "saffron-city-area",
    DisplayName = "Saffron City"
}},
{ 69, new LocationArea { 
    Name = "kanto-route-8-area", 
    DisplayName = "Route 08"
}},
{ 70, new LocationArea {
    Name = "lavender-town-area",
    DisplayName = "Lavender Town" 
}},
{ 71, new LocationArea { 
    Name = "kanto-route-10-area", 
    DisplayName = "Route 10"
}},
{ 72, new LocationArea {
    Name = "rock-tunnel-1f",
    DisplayName = "Rock Tunnel"
}},
{ 73, new LocationArea {
    Name = "rock-tunnel-b1f",
    DisplayName = "Rock Tunnel (B1F)"
}},
{ 74, new LocationArea {
    Name = "rock-tunnel-b2f",
    DisplayName = "Rock Tunnel (B2F)"
}},
{ 75, new LocationArea { 
    Name = "kanto-route-9-area", 
    DisplayName = "Route 09"
}},
{ 76, new LocationArea {
    Name = "power-plant-area",
    DisplayName = "Power Plant"
}},
{ 77, new LocationArea {
    Name = "cerulean-city-area",
    DisplayName = "Cerulean City"
}},
{ 78, new LocationArea {
    Name = "cerulean-cave",
    DisplayName = "Cerulean Cave"
}},
{ 79, new LocationArea {
    Name = "cerulean-cave-1f",
    DisplayName = "Cerulean Cave(1F)"
}},
{ 80, new LocationArea {
    Name = "cerulean-cave-b1f",
    DisplayName = "Cerulean Cave(B1F)"
}},
{ 81, new LocationArea {
    Name = "cerulean-cave-2f",
    DisplayName = "Cerulean Cave(2F)"
}},
{ 82, new LocationArea { 
    Name = "kanto-route-24-area", 
    DisplayName = "Route 24"
}},
{ 83, new LocationArea { 
    Name = "kanto-route-25-area", 
    DisplayName = "Route 25"
}},
{ 84, new LocationArea {
    Name = "kanto-route-5-area",
    DisplayName = "Route 05"
}},
{ 85, new LocationArea {
    Name = "kanto-route-7-area",
    DisplayName = "Route 07"
}},
{ 86, new LocationArea {
    Name = "celadon-city-area-area",
    DisplayName = "Celadon City"
}},
{ 87, new LocationArea {
    Name = "kanto-route-16-area",
    DisplayName = "Route 16"
}},
{ 88, new LocationArea {
    Name = "kanto-route-17-area",
    DisplayName = "Route 17"
}},
{ 89, new LocationArea {
    Name = "kanto-route-18-area",
    DisplayName = "Route 18"
}},
{ 90, new LocationArea {
    Name = "fuchsia-city-area",
    DisplayName = "Fuchsia City"
}},
{ 91, new LocationArea {
    Name = "kanto-route-15-area",
    DisplayName = "Route 15"
}},
{ 92, new LocationArea {
    Name = "kanto-route-14-area",
    DisplayName = "Route 14"
}},
{ 93, new LocationArea {
    Name = "kanto-route-13-area",
    DisplayName = "Route 13"
}},
{ 94, new LocationArea {
    Name = "kanto-route-12-area",
    DisplayName = "Route 12"
}},
{ 95, new LocationArea {
    Name = "kanto-route-11-area",
    DisplayName = "Route 11"
}},
{ 96, new LocationArea {
    Name = "digletts-cave-area",
    DisplayName = "Diglett's Cave"
}},
{ 97, new LocationArea {
    Name = "kanto-route-2-area",
    DisplayName = "Route 02"
}},
{ 98, new LocationArea {
    Name = "pewter-city-area",
    DisplayName = "Pewter City"
}},
{ 99, new LocationArea {
    Name = "kanto-route-3-area",
    DisplayName = "Route 03"
}},
{ 100, new LocationArea {
    Name = "mt-moon-mt-moon-square",
    DisplayName = "Mt. Moon"
}},
{ 101, new LocationArea {
    Name = "mt-moon-1f",
    DisplayName = "Mt. Moon(1F)"
}},
{ 102, new LocationArea {
    Name = "mt-moon-2f",
    DisplayName = "Mt. Moon(2F)"
}},
{ 103, new LocationArea {
    Name = "mt-moon-b1f",
    DisplayName = "Mt. Moon(B1F)"
}},
{ 104, new LocationArea {
    Name = "mt-moon-b2f",
    DisplayName = "Mt. Moon(B2F)"
}},
{ 105, new LocationArea {
    Name = "kanto-route-4-area",
    DisplayName = "Route 04"
}},
{ 106, new LocationArea {
    Name = "viridian-city-area",
    DisplayName = "Viridian City"
}},
{ 107, new LocationArea {
    Name = "viridian-forest-area",
    DisplayName = "Viridian Forest"
}},
{ 108, new LocationArea {
    Name = "kanto-route-1-area",
    DisplayName = "Route 01"
}},
{ 109, new LocationArea {
    Name = "pallet-town-area",
    DisplayName = "Pallet Town"
}},
{ 110, new LocationArea {
    Name = "kanto-sea-route-21-area",
    DisplayName = "Route 21"
}},
{ 111, new LocationArea {
    Name = "cinnabar-island-area",
    DisplayName = "Cinnabar Island"
}},
{ 112, new LocationArea {
    Name = "seafoam-islands",
    DisplayName = "Seafoam Islands"
}},
{ 113, new LocationArea {
    Name = "seafoam-islands-1f",
    DisplayName = "Seafoam Islands"
}},
{ 114, new LocationArea {
    Name = "seafoam-islands-b1f",
    DisplayName = "Seafoam Islands(B1F)"
}},
{ 115, new LocationArea {
    Name = "seafoam-islands-b2f",
    DisplayName = "Seafoam Islands(B2F)"
}},
{ 116, new LocationArea {
    Name = "seafoam-islands-b3f",
    DisplayName = "Seafoam Islands(B3F)"
}},
{ 117, new LocationArea {
    Name = "seafoam-islands-b4f",
    DisplayName = "Seafoam Islands(B4F)"
}},
{ 118, new LocationArea {
    Name = "kanto-sea-route-19-area",
    DisplayName = "Route 19"
}},
{ 119, new LocationArea {
    Name = "kanto-route-22-area",
    DisplayName = "Route 22"
}},
{ 120, new LocationArea {
    Name = "kanto-route-28-area",
    DisplayName = "Route 28"
}},
{ 121, new LocationArea { 
    Name = "mt-silver-cave", 
    DisplayName = "Mt.Silver Cave"
}}
    };
    public static LocationArea? GetLocation(int id) => 
        Locations.GetValueOrDefault(id);

    public static int? GetLocationId(string name) =>
        Locations.FirstOrDefault(x => x.Value.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Key;


}