# Simulation #

## Projektsbeskrivning ##
En interaktiv SPH 3D realtidssimulering av vätskor, gaser, och softbody-objekt.
Rendreringen sker med en ray-marcher-shader, medan en marching squares-shader skapar det rendrerade mesh:et.
Programmet som helhet använder ett paralleliserat neighboor-search-system för att kunna sköta beräkningar i parallel på GPUn med hög prestanda. Även rendreringen sker med en parallel shader-kod (liknande en pixel-shader).
Det som simuleras är främst viskositet, elastisitet, plastisitet, gravitation, "klibbighet", temperatursöverföring, fasövergångar, och interaktioner (muspekare).

## Projektstruktur och kod ##
All shaderkod ligger under: FluidSim3D/Assets/Scripts/Compute shaders
All tillhörande c#-managerkod ligger under: FluidSim3D/Assets/Scripts/SubManagers
c#-koden använder vissa hjälpfunktioner - dessa ligger under: FluidSim3D/Assets/Scripts/Helpers

## Demofilmer med beskrivningar ##
(Inspelningarna är tagna i realtid med ett nvidia rtx 4070ti grafikkort)

## Demo 1 ##
Beskrivning: Två rigid body-objekt och en vätska med låg viskositet. Sedan ökas vätskans viskositet. Sist minskas platisiteten vilket gör vätskan svårrörlig.
Tekniska detaljer: 2D, 60000partiklar, ~620FPS

## Demo 2 ##
Beskrivning: Vatten som kokas av "magma", och bildar bubblor av vattenånga
Tekniska detaljer: 2D, 60000partiklar, ~620FPS

## Demo 3 ##
Beskrivning: Vatten som påverkas av användaren. Först rendreras varje partikel för sig. Sedan aktiveras en ray-marcher-shader för att rendrera ett fullständigt mesh.
Tekniska detaljer: 2D, 60000partiklar, ~60FPS
