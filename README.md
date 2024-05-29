# Simulation #

## Projektsbeskrivning ##
En interaktiv SPH 3D realtidssimulering av vätskor, gaser, och softbody-objekt.
Rendreringen sker med en ray-marcher-shader, medan en marching squares-shader skapar det rendrerade mesh:et.
Programmet som helhet använder ett paralleliserat neighboor-search-system för att kunna sköta beräkningar i parallel på GPUn med hög prestanda. Även rendreringen sker med en parallel shader-kod (liknande en pixel-shader).
Det som simuleras är främst viskositet, elastisitet, plastisitet, ytspänning, gravitation, "klibbighet", temperatursöverföring, fasövergångar, och interaktioner (muspekare).

## Projektstruktur och kod ##
All shaderkod ligger under FluidSim3D/Assets/Scripts/Compute shaders.
All tillhörande c#-managerkod ligger under FluidSim3D/Assets/Scripts/SubManagers.
c#-koden använder vissa hjälpfunktioner - dessa ligger under FluidSim3D/Assets/Scripts/Helpers.

## Demofilmer med beskrivningar ##
(Inspelningarna är tagna i realtid med ett nvidia rtx 4070ti grafikkort).

### Demo 1 ###
 - Beskrivning: Två rigid body-objekt och en vätska med låg viskositet vid start. Sedan ökas vätskans viskositet. Sist minskas platisiteten vilket gör vätskan svårrörlig. röd färg <=> hög fart.
 - Tekniska detaljer: 2D, 30000partiklar, ~620FPS
[Watch the video](https://drive.google.com/open?id=1kJEpSKBCAE8BCXwzlHZaufPYNMzmQJNb&usp=drive_copy)

### Demo 2 ###
 - Beskrivning: Vatten(60ºC) som kokas av en varm vätska(500ºC), vilket bildar bubblor av vattenånga. Blå färg <=> vatten, gul färg <=> vattenånga, röd färg <=> varm vätska.
 - Tekniska detaljer: 2D, 60000partiklar, ~390FPS

### Demo 3 ###
 - Beskrivning: Vatten som påverkas av användaren. Ena videon rendreras varje partikel för sig, den andra använder ett mesh genererat av en marching squares-shader.
 - Tekniska detaljer: 2D, 60000partiklar, ~55FPS
