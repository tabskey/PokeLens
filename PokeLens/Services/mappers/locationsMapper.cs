using PokeLens.Models;

namespace PokeLens.Services.mappers;

public static class HeartGoldSoulSilverMapper
{
    public static readonly Dictionary<int, LocationArea> Locations = new()
    {
             { 1, new LocationArea {
                 Name = "new-bark-town",
                 DisplayName = "New Bark Town"
             }},
             { 2, new LocationArea
             {
                 Name = "johto-route-29",
                 DisplayName = "Route 29"
             }},
            { 3, new LocationArea { 
                Name = "cherrygrove-city", 
                DisplayName = "Cherrygrove City"
            }},
            { 4, new LocationArea { 
                Name = "johto-route-30", 
                DisplayName = "Route 30"
            }},
            //Mr. Pokémon's House, Professor Elm's Lab
            { 5, new LocationArea { 
                Name = "johto-route-31", 
                DisplayName = "Route 31"
            }},
            { 6, new LocationArea { 
                Name = "violet-city", 
                DisplayName = "Violet City"
            }},
            { 7, new LocationArea { 
                Name = "sprout-tower", 
                DisplayName = "Sprout Tower"
            }},
            { 8, new LocationArea { 
                Name = "johto-route-32", 
                DisplayName = "Route 32"
            }},
            { 9, new LocationArea { 
                Name = "ruins-of-alph", 
                DisplayName = "Ruins of Alph"
            }},
            { 10, new LocationArea { 
                Name = "union-cave", 
                DisplayName = "Union Cave"
            }},
            {11, new LocationArea {
                Name = "johto-route-33",
                DisplayName = "Route 33"
            }},
            { 12, new LocationArea { 
                Name = "azalea-town", 
                DisplayName = "Azalea Town"
            }},
            { 13, new LocationArea { 
                Name = "slowpoke-well", 
                DisplayName = "Slowpoke Well"
            }},
            { 14, new LocationArea { 
                Name = "ilex-forest", 
                DisplayName = "Ilex Forest"
            }},
            { 15, new LocationArea { 
                Name = "johto-route-34", 
                DisplayName = "Route 34"
            }},
            { 16, new LocationArea { 
                Name = "goldenrod-city", 
                DisplayName = "Goldenrod City"
            }},
            { 17, new LocationArea { 
                Name = "johto-route-35", 
                DisplayName = "Route 35"
            }},
            { 18, new LocationArea { 
                Name = "national-park", 
                DisplayName = "National Park"
            }},
            { 19, new LocationArea { 
                Name = "johto-route-36", 
                DisplayName = "Route 36"
            }},
            { 20, new LocationArea { 
                Name = "johto-route-37", 
                DisplayName = "Route 37"
            }},
            { 20, new LocationArea { 
                Name = "ecruteak-city", 
                DisplayName = "Ecruteak City"
            }},
            { 21, new LocationArea { 
                Name = "burned-tower", 
                DisplayName = "Burned Tower"
            }},
            { 23, new LocationArea { 
                Name = "johto-route-38", 
                DisplayName = "Route 38"
            }},
            { 24, new LocationArea { 
                Name = "johto-route-39", 
                DisplayName = "Route 39"
            }},
            { 25, new LocationArea { 
                Name = "olivine-city", 
                DisplayName = "Olivine City"
            }},
            { 26, new LocationArea { 
                Name = "johto-lighthouse", 
                DisplayName = "Lighthouse"
            }},
            { 27, new LocationArea { 
                Name = "johto-route-40", 
                DisplayName = "Route 40"
            }},
            { 29, new LocationArea { 
                Name = "johto-route-41", 
                DisplayName = "Route 41"
            }},
            { 30, new LocationArea { 
                Name = "cianwood-city", 
                DisplayName = "Cianwood City"
            }},
            { 31, new LocationArea { 
                Name = "johto-route-42", 
                DisplayName = "Route 42"
            }},
            { 32, new LocationArea { 
                Name = "mt-mortar", 
                DisplayName = "Mt. Mortar"
            }},
            { 33, new LocationArea { 
                Name = "mahogany-town", 
                DisplayName = "Mahogany Town"
            }},
            { 34, new LocationArea { 
                Name = "johto-route-43", 
                DisplayName = "Route 43"
            }},
            { 35, new LocationArea { 
                Name = "lake-of-rage", 
                DisplayName = "Lake of Rage"
            }},
            
            { 36, new LocationArea { 
                Name = "team-rocket-hq", 
                DisplayName = "Team Rocket HQ"
            }},
            {37, new LocationArea
            { Name = "goldenrod-tunnel",
                DisplayName = "Goldenrod Tunnel"
            }},
            { 38, new LocationArea { 
                Name = "johto-route-44", 
                DisplayName = "Route 44"
            }},
            { 39, new LocationArea { 
                Name = "ice-path", 
                DisplayName = "Ice Path"
            }},
            { 40, new LocationArea { 
                Name = "blackthorn-city", 
                DisplayName = "Blackthorn City"
            }},
            { 41, new LocationArea { 
                Name = "dragons-den", 
                DisplayName = "Dragon's Den"
            }},
            { 42, new LocationArea { 
                Name = "johto-route-45", 
                DisplayName = "Route 45"
            }},
            {43, new LocationArea{
                Name = "dark-cave",
                DisplayName = "Dark Cave"
            }},
            { 44, new LocationArea { 
                Name = "johto-route-46", 
                DisplayName = "Route 46"
            }},
             {45, new LocationArea {
                    Name = "bell-tower",
                    DisplayName = "Bell Tower" 
             }},
            { 46, new LocationArea { 
                Name = "whirl-islands", 
                DisplayName = "Whirl Islands"
            }},
            { 47, new LocationArea { 
                Name = "kanto-route-27", 
                DisplayName = "Route 27"
            }},
            { 48, new LocationArea { 
                Name = "kanto-route-26", 
                DisplayName = "Route 26"
            }},
            { 49, new LocationArea
            {
                Name = "vermillion-city",
                DisplayName = "Vermillion City"
            }},
            { 50, new LocationArea { 
                Name = "kanto-route-6", 
                DisplayName = "Route 06"
            }},
            { 51, new LocationArea {
                Name = "saffron-city",
                DisplayName = "Saffron City"
            }},
            { 52, new LocationArea { 
                Name = "kanto-route-8", 
                DisplayName = "Route 08"
            }},
            { 53, new LocationArea {
                Name = "lavender-town",
                DisplayName = "Lavender Town" 
            }},
            { 54, new LocationArea { 
                Name = "kanto-route-10", 
                DisplayName = "Route 10"
            }},
            { 55, new LocationArea {
               Name = "rock-tunnel",
               DisplayName = "Rock Tunnel"
            }},
            { 56, new LocationArea { 
                Name = "kanto-route-9", 
                DisplayName = "Route 09"
            }},
            {57, new LocationArea {
                Name = "power-plant",
                DisplayName = "Power Plant"
            }},
            {58, new LocationArea {
                Name = "cerulean-city",
                DisplayName = "Cerulean City"
            }},
            //TODO: Verificar se pode entrar direto.
            {59, new LocationArea
            {
                Name = "cerulean-cave",
                DisplayName = "Cerulean Cave"
            }},
            
            { 60, new LocationArea { 
                Name = "kanto-route-24", 
                DisplayName = "Route 24"
            }},
            { 61, new LocationArea { 
                Name = "kanto-route-25", 
                DisplayName = "Route 25"
            }},
            {62, new LocationArea {
                Name = "kanto-route-5",
                DisplayName = "Route 05"
            }},
            {63, new LocationArea {
                Name = "kanto-route-7",
                DisplayName = "Route 07"
            }},
            {64, new LocationArea {
                Name = "celadon-city",
                DisplayName = "Celadon City"
            }},
            {65, new LocationArea {
                Name = "kanto-route-16",
                DisplayName = "Route 16"
            }},
            {66, new LocationArea {
                Name = "kanto-route-17",
                DisplayName = "Route 17"
            }},
            {67, new LocationArea
            {
                Name = "kanto-route-18",
                DisplayName = "Route 18"
            }},
            {68, new LocationArea {
                Name = "fuchsia-city",
                DisplayName = "Fuchsia City"
            }},
            {69, new LocationArea {
                Name = "kanto-route-15",
                DisplayName = "Route 15"
            }},
            {70, new LocationArea {
                Name = "kanto-route-14",
                DisplayName = "Route 14"
            }},
            {71, new LocationArea
            {
                Name = "kanto-route-13",
                DisplayName = "Route 13"
            }},
            {72, new LocationArea
            {Name = "kanto-route-12",
                DisplayName = "Route 12"
            }},
            {73, new LocationArea
            {
                Name = "kanto-route-11",
                DisplayName = "Route 11"
            }},
            { 74, new LocationArea
            {
                Name = "digletts-cave",
                DisplayName = "Diglett's Cave"
            }},
            {75, new LocationArea
            {Name = "kanto-route-2",
                DisplayName = "Route 02"
            }},
            {76, new LocationArea
            {
                Name = "pewter-city",
                DisplayName = "Pewter City"
            }},
            {77, new LocationArea
            {
            Name = "kanto-route-3",
                DisplayName = "Route 03"
            }},
            {78, new LocationArea
            {
                Name = "mt-moon",
                DisplayName = "Mt. Moon"
            }},
            {79, new LocationArea
            {
                Name = "kanto-route-4",
                DisplayName = "Route 04"
            }},
            {80, new LocationArea
            {
                Name = "viridian-city",
                DisplayName = "Viridian City"
            }
            },
            { 81, new LocationArea
            {
                Name = "kanto-route-1",
                DisplayName = "Route 01"
            }},
            {82, new LocationArea
            {
                Name = "pallet-town",
                DisplayName = "Pallet Town"
                
            }},
            {83, new LocationArea
            {
                Name = "kanto-route-21",
                DisplayName = "Route 21"
            }},
            {84, new LocationArea
            {
                Name = "cinnabar-island",
                DisplayName = "Cinnabar Island"
            }},
            {85, new LocationArea
            {
                Name = "kanto-route-20",
                DisplayName = "Route 20"
            }},
            {86, new LocationArea
            {
                Name = "seafoam-islands",
                DisplayName = "Seafoam Islands"
            }},
            {87, new LocationArea
            {
                Name = "kanto-route-19",
                DisplayName = "Route 19"
            }},
            {88, new LocationArea {
                Name = "kanto-route-22",
                DisplayName = "Route 22"
                
            }},
            {89, new LocationArea
            {
                Name = "kanto-route-28",
                DisplayName = "Route 28"
            }},
            { 90, new LocationArea { 
                Name = "silver-cave", 
                DisplayName = "Silver Cave"
            }},
            //TODO: Ajustar a formatação,
            //TODO: Pesquisar as rotas de mar: "name": "kanto-sea-route-19",
            //"url": "https://pokeapi.co/api/v2/location/98/"
        };
    public static LocationArea? GetLocation(int id) => 
        Locations.GetValueOrDefault(id);

    public static int? GetLocationId(string name) =>
        Locations.FirstOrDefault(x => x.Value.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Key;


}